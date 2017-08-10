import org.apache.flink.api.common.functions.FlatMapFunction;
import org.apache.flink.api.common.functions.MapFunction;
import org.apache.flink.api.common.functions.ReduceFunction;
import org.apache.flink.api.java.tuple.Tuple2;
import org.apache.flink.api.java.tuple.Tuple3;
import org.apache.flink.api.java.utils.ParameterTool;
import org.apache.flink.core.fs.FileSystem;
import org.apache.flink.streaming.api.TimeCharacteristic;
import org.apache.flink.streaming.api.datastream.DataStream;
import org.apache.flink.streaming.api.environment.StreamExecutionEnvironment;
import org.apache.flink.streaming.api.functions.timestamps.AscendingTimestampExtractor;
import org.apache.flink.streaming.connectors.kafka.FlinkKafkaConsumer010;
import org.apache.flink.streaming.connectors.kafka.FlinkKafkaProducer010;
import org.apache.flink.streaming.util.serialization.SimpleStringSchema;
import org.apache.flink.util.Collector;

import java.util.Arrays;
import java.util.Properties;
import java.util.UUID;

public class WordCount {
    public static void main(String[] args) throws Exception {
        Properties inputProperties = new Properties();
        inputProperties.setProperty("bootstrap.servers", "localhost:9092");
        inputProperties.setProperty("group.id", UUID.randomUUID().toString());
        inputProperties.setProperty("auto.offset.reset", "earliest");

        StreamExecutionEnvironment environment = StreamExecutionEnvironment.getExecutionEnvironment();
        environment.setStreamTimeCharacteristic(TimeCharacteristic.IngestionTime);
        // ingestion time doesnt need assigning time
        // event time works as expected
        // processing time fails without assignment
        //environment.setParallelism(1);

        environment
            .addSource(new FlinkKafkaConsumer010<>("WordCountInput", new SimpleStringSchema(), inputProperties))
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
            .writeAsText("C:\\Git\\MasterThesis\\Experiments\\WordCount\\flink", FileSystem.WriteMode.OVERWRITE);
        /*Properties outputProperties = new Properties();
        outputProperties.setProperty("bootstrap.servers", "localhost:9092");
        FlinkKafkaProducer010.FlinkKafkaProducer010Configuration producer = FlinkKafkaProducer010.writeToKafkaWithTimestamps(wordCount, "WordCountOutput", new SimpleStringSchema(), outputProperties);
        producer.setWriteTimestampToKafka(true); //obligatory*/

        environment.execute("FlinkWordCount");
    }
}
