import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.apache.kafka.common.serialization.StringDeserializer;
import org.apache.spark.SparkConf;
import org.apache.spark.api.java.Optional;
import org.apache.spark.api.java.function.FlatMapFunction;
import org.apache.spark.api.java.function.Function3;
import org.apache.spark.api.java.function.PairFunction;
import org.apache.spark.streaming.Duration;
import org.apache.spark.streaming.State;
import org.apache.spark.streaming.StateSpec;
import org.apache.spark.streaming.api.java.JavaInputDStream;
import org.apache.spark.streaming.api.java.JavaPairDStream;
import org.apache.spark.streaming.api.java.JavaStreamingContext;
import org.apache.spark.streaming.dstream.InputDStream;
import org.apache.spark.streaming.kafka010.ConsumerStrategies;
import org.apache.spark.streaming.kafka010.KafkaUtils;
import org.apache.spark.streaming.kafka010.LocationStrategies;
import scala.Tuple2;

import java.io.File;
import java.util.*;

public class StockTweetJoin {
    public static void main(String[] args) throws Exception {
        if (args == null || args.length == 0){
            args = new String[8];
            args[0] = "localhost:9092"; // brokers
            args[1] = "TwitterS,NasdaqS"; // topic
            args[2] = "C:\\Git\\MasterThesis\\experiments\\_singleStockTweetJoin\\Spark\\"; // output directory
            args[3] = "C:\\Git\\MasterThesis\\deployment\\data\\companies"; // companies file path
            args[4] = "local[*]"; // master address
            args[5] = "1000"; // batch interval
            args[6] = "/tmp/spark-checkpoint"; // checkpoints directory
            args[7] = "C:\\Apache\\hadoop\\"; // hadoop directory
        }
        final String outputDirectory = args[2];
        System.setProperty("hadoop.home.dir", args[7]);
        String[] topics = args[1].split(",");

        Map<String, Object> kafkaParams = new HashMap<>();
        kafkaParams.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, args[0]);
        kafkaParams.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class);
        kafkaParams.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class);
        kafkaParams.put(ConsumerConfig.GROUP_ID_CONFIG, "SWCG_" + System.currentTimeMillis());
        kafkaParams.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        kafkaParams.put("enable.auto.commit", false);

        Map<String, String[]> companies = new HashMap<>();
        Scanner scanner = new Scanner(new File(args[3]));
        while (scanner.hasNextLine()){
            String[] fields = scanner.nextLine().split("\t");
            companies.put(fields[0], fields[1].split(","));
        }

        SparkConf sparkConfig = new SparkConf()
            .setMaster(args[4])
            .setAppName("SWCA_" + System.currentTimeMillis());

        JavaStreamingContext streamingContext = new JavaStreamingContext(sparkConfig, new Duration(Integer.parseInt(args[5])));
        streamingContext.checkpoint(args[6]);

        JavaPairDStream<String, Long> tweets = KafkaUtils
            .createDirectStream(streamingContext, LocationStrategies.PreferConsistent(), ConsumerStrategies.<String, String>Subscribe(Arrays.asList(topics[0]), kafkaParams))
            .mapToPair(new PairFunction<ConsumerRecord<String, String>, String, Long>() {
                @Override
                public Tuple2<String, Long> call(ConsumerRecord<String, String> tweet) throws Exception {
                    long currentTimestamp = System.currentTimeMillis();
                    String companyIndex = null;
                    List<String> tweetWords = Arrays.asList(tweet.value().toLowerCase().split("\\W+"));
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
            .filter(record -> record._1 != null)
            .window(new Duration(10000));

        JavaPairDStream<String, Long> stocks = KafkaUtils
            .createDirectStream(streamingContext, LocationStrategies.PreferConsistent(), ConsumerStrategies.<String, String>Subscribe(Arrays.asList(topics[1]), kafkaParams))
            .mapToPair(new PairFunction<ConsumerRecord<String,String>, String, Long>() {
                @Override
                public Tuple2<String, Long> call(ConsumerRecord<String, String> record) throws Exception {
                    return new Tuple2<>(record.value().split("\t")[0], System.currentTimeMillis());
                }
            })
            .window(new Duration(10000));

        tweets
            .join(stocks)
            .mapValues(record -> String.format("%d\t%d\t%d", record._1, record._2, System.currentTimeMillis()))
            .foreachRDD(rdd -> rdd.saveAsTextFile(outputDirectory));

        Runtime.getRuntime().addShutdownHook(new Thread() {
            @Override
            public void run(){
                streamingContext.stop(true, true);
            }
        });

        streamingContext.start();
        streamingContext.awaitTermination();
    }
}
