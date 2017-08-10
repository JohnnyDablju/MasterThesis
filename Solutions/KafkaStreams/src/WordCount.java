import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.common.serialization.Serde;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.KeyValue;
import org.apache.kafka.streams.StreamsConfig;
import org.apache.kafka.streams.kstream.*;
import org.apache.kafka.streams.processor.WallclockTimestampExtractor;

import java.io.FileNotFoundException;
import java.io.PrintWriter;
import java.util.*;

public class WordCount {
    public static void main(String[] args) throws Exception {
        Properties props = new Properties();
        props.put(StreamsConfig.APPLICATION_ID_CONFIG, UUID.randomUUID().toString());//"WordCount");
        props.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, "localhost:9092");
        props.put(StreamsConfig.KEY_SERDE_CLASS_CONFIG, Serdes.String().getClass().getName());
        props.put(StreamsConfig.VALUE_SERDE_CLASS_CONFIG, Serdes.String().getClass().getName());
        props.put(StreamsConfig.TIMESTAMP_EXTRACTOR_CLASS_CONFIG, WallclockTimestampExtractor.class);
        //props.put(StreamsConfig.CACHE_MAX_BYTES_BUFFERING_CONFIG, 0);
        //props.put(ConsumerConfig.GROUP_ID_CONFIG, UUID.randomUUID().toString());
        props.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");

        KStreamBuilder builder = new KStreamBuilder();
        KStream<String, String> source = builder.stream(Serdes.String(), Serdes.String(),"WordCountInput");

        source
            .flatMap(new KeyValueMapper<String, String, Iterable<KeyValue<String, Tuple2>>>() {
                @Override
                public Iterable<KeyValue<String, Tuple2>> apply(String key, String message) {
                    Long currentTimestamp = System.currentTimeMillis();
                    ArrayList<KeyValue<String, Tuple2>> list = new ArrayList<>();
                    for (String word : message.toLowerCase().split("\\W+")){
                        list.add(new KeyValue<>(word, new Tuple2(1, currentTimestamp)));
                    }
                    return list;
                }
            })
            .groupByKey(Serdes.String(), Serdes.serdeFrom(new Tuple2Serializer(), new Tuple2Deserializer()))//OK
            .reduce(new Reducer<Tuple2>() {
                @Override
                public Tuple2 apply(Tuple2 a, Tuple2 b) {
                    //if (a != null && b != null) {
                    return new Tuple2(a.count + b.count, Math.max(a.timestamp, b.timestamp));
                    //}
                    //return null;
                }
            }, "reduction")
            .mapValues(record -> String.format("%d\t%d\t%d", record.timestamp, System.currentTimeMillis(), record.count))
            .writeAsText("C:\\Git\\MasterThesis\\Experiments\\WordCount\\KafkaStreams\\0");

        KafkaStreams streams = new KafkaStreams(builder, props);
        streams.cleanUp();
        streams.start();
        Runtime.getRuntime().addShutdownHook(new Thread(streams::close));
    }
}

