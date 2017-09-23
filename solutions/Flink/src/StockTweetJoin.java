import org.apache.flink.api.common.functions.FlatMapFunction;
import org.apache.flink.api.common.functions.JoinFunction;
import org.apache.flink.api.common.functions.MapFunction;
import org.apache.flink.api.common.functions.ReduceFunction;
import org.apache.flink.api.java.functions.KeySelector;
import org.apache.flink.api.java.tuple.Tuple2;
import org.apache.flink.api.java.tuple.Tuple3;
import org.apache.flink.core.fs.FileSystem;
import org.apache.flink.streaming.api.TimeCharacteristic;
import org.apache.flink.streaming.api.datastream.SingleOutputStreamOperator;
import org.apache.flink.streaming.api.environment.StreamExecutionEnvironment;
import org.apache.flink.streaming.api.windowing.assigners.TumblingProcessingTimeWindows;
import org.apache.flink.streaming.api.windowing.time.Time;
import org.apache.flink.streaming.connectors.kafka.FlinkKafkaConsumer010;
import org.apache.flink.streaming.util.serialization.SimpleStringSchema;
import org.apache.flink.streaming.util.serialization.TypeInformationKeyValueSerializationSchema;
import org.apache.flink.util.Collector;

import java.io.File;
import java.util.*;

public class StockTweetJoin {
    public static void main(String[] args) throws Exception {
        if (args == null || args.length == 0){
            args = new String[5];
            args[0] = "localhost:9092"; // brokers
            args[1] = "TwitterFb,NasdaqFb"; // topics
            args[2] = "C:\\Git\\MasterThesis\\experiments\\_singleStockTweetJoin\\Flink"; // output directory
            args[3] = "C:\\Git\\MasterThesis\\deployment\\data\\companies"; // companies file path
            args[4] = "4"; // parallelism
        }

        Properties properties = new Properties();
        properties.setProperty("bootstrap.servers", args[0]);
        properties.setProperty("group.id", "FWCG_" + System.currentTimeMillis());
        properties.setProperty("auto.offset.reset", "earliest");

        String[] topics = args[1].split(",");

        Map<String, String[]> companies = new HashMap<>();
        Scanner scanner = new Scanner(new File(args[3]));
        while (scanner.hasNextLine()){
            String[] fields = scanner.nextLine().split("\t");
            companies.put(fields[0], fields[1].split(","));
        }

        StreamExecutionEnvironment environment = StreamExecutionEnvironment.getExecutionEnvironment();
        environment.setParallelism(Integer.parseInt(args[4]));
        environment.setStreamTimeCharacteristic(TimeCharacteristic.IngestionTime);

        SingleOutputStreamOperator<Tuple2<String, Long>> tweets = environment
            .addSource(new FlinkKafkaConsumer010<>(topics[0], new SimpleStringSchema(), properties))
            .map(new MapFunction<String, Tuple2<String, Long>>() {
                @Override
                public Tuple2<String, Long> map(String tweet) throws Exception {
                    long currentTimestamp = System.currentTimeMillis();
                    String companyIndex = null;
                    List<String> tweetWords = Arrays.asList(tweet.toLowerCase().split("\\W+"));
                    Iterator<Map.Entry<String, String[]>> companyIterator = companies.entrySet().iterator();
                    while (companyIterator.hasNext() && companyIndex == null){
                        Map.Entry<String, String[]> company = companyIterator.next();
                        for (String phrase : company.getValue()){
                            List<String> nameWords = new ArrayList<String>(Arrays.asList(phrase.split(" ")));
                            int wordCount = nameWords.size();
                            nameWords.retainAll(tweetWords);
                            if (nameWords.size() == wordCount){
                                companyIndex = company.getKey();
                                break;
                            }
                        }
                    }
                    return new Tuple2<>(companyIndex, currentTimestamp);
                }
            })
            .filter(record -> record.f0 != null);

        SingleOutputStreamOperator<Tuple2<String, Long>> stocks = environment
            .addSource(new FlinkKafkaConsumer010<>(topics[1], new SimpleStringSchema(), properties))
            .map(new MapFunction<String, Tuple2<String, Long>>() {
                @Override
                public Tuple2<String, Long> map(String record) throws Exception {
                    return new Tuple2<>(record.split("\t")[0], System.currentTimeMillis());
                }
            });

        tweets
            .join(stocks)
            .where(new KeySelector<Tuple2<String,Long>, String>() {
                @Override
                public String getKey(Tuple2<String, Long> record) throws Exception {
                    return record.f0;
                }
            })
            .equalTo(new KeySelector<Tuple2<String,Long>, String>() {
                @Override
                public String getKey(Tuple2<String, Long> record) throws Exception {
                    return record.f0;
                }
            })
            .window(TumblingProcessingTimeWindows.of(Time.seconds(10)))
            .apply(new JoinFunction<Tuple2<String,Long>, Tuple2<String,Long>, Tuple2<Long, Long>>() {
                @Override
                public Tuple2<Long, Long> join(Tuple2<String, Long> stock, Tuple2<String, Long> tweet) throws Exception {
                    return new Tuple2<>(stock.f1, tweet.f1);
                }
            })
            .map(new MapFunction<Tuple2<Long, Long>, String>() {
                @Override
                public String map(Tuple2<Long, Long> record) throws Exception {
                    return String.format("%d\t%d\t%d", record.f0, record.f1, System.currentTimeMillis());
                }
            })
            .writeAsText(args[2], FileSystem.WriteMode.OVERWRITE);

        environment.execute("FWCA_" + System.currentTimeMillis());
    }
}
