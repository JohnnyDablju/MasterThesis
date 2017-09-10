import org.apache.flink.api.common.functions.FlatMapFunction;
import org.apache.flink.api.common.functions.MapFunction;
import org.apache.flink.api.common.functions.ReduceFunction;
import org.apache.flink.api.java.tuple.Tuple3;
import org.apache.flink.core.fs.FileSystem;
import org.apache.flink.streaming.api.TimeCharacteristic;
import org.apache.flink.streaming.api.environment.StreamExecutionEnvironment;
import org.apache.flink.streaming.connectors.kafka.FlinkKafkaConsumer010;
import org.apache.flink.streaming.util.serialization.SimpleStringSchema;
import org.apache.flink.util.Collector;

import java.util.Properties;

public class WordCount {
    public static void main(String[] args) throws Exception {
        if (args == null || args.length == 0){
            args = new String[4];
            args[0] = "localhost:9092";
            args[1] = "WordCountInput";
            args[2] = "C:\\Git\\MasterThesis\\experiments\\_singleWordCount\\Flink";
            args[3] = "4";
        }

        Properties properties = new Properties();
        properties.setProperty("bootstrap.servers", args[0]);
        properties.setProperty("group.id", "FWCG_" + System.currentTimeMillis());
        properties.setProperty("auto.offset.reset", "earliest");

        StreamExecutionEnvironment environment = StreamExecutionEnvironment.getExecutionEnvironment();
        environment.setParallelism(Integer.parseInt(args[3]));
        environment.setStreamTimeCharacteristic(TimeCharacteristic.IngestionTime);

        environment
            .addSource(new FlinkKafkaConsumer010<>(args[1], new SimpleStringSchema(), properties))
            .flatMap(new FlatMapFunction<String, Tuple3<String, Integer, Long>>() {
                @Override
                public void flatMap(String message, Collector<Tuple3<String, Integer, Long>> record) {
                    Long currentTimestamp = System.currentTimeMillis();
                    for (String word : message.toLowerCase().split("\\W+")) {
                        record.collect(new Tuple3<>(word, 1, currentTimestamp));
                    }
                }
            })
            .keyBy(0)
            .reduce(new ReduceFunction<Tuple3<String, Integer, Long>>() {
                @Override
                public Tuple3<String, Integer, Long> reduce(Tuple3<String, Integer, Long> a, Tuple3<String, Integer, Long> b) throws Exception {
                    return new Tuple3<>(a.f0, a.f1 + b.f1, Math.max(a.f2, b.f2));
                }
            })
            .map(new MapFunction<Tuple3<String, Integer, Long>, String>() {
                @Override
                public String map(Tuple3<String, Integer, Long> record) throws Exception {
                    return String.format("%d\t%d\t%s\t%d", record.f2, System.currentTimeMillis(), record.f0, record.f1);
                }
            })
            .writeAsText(args[2], FileSystem.WriteMode.OVERWRITE);

        environment.execute("FWCA_" + System.currentTimeMillis());
    }
}
