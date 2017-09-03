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
        if (args == null){
            args = new String[6];
            args[0] = "localhost:9092";
            args[1] = "localhost:2181";
            args[2] = "8";
            args[3] = "C:\\Git\\MasterThesis\\Experiments\\WordCount\\KafkaStreams\\";
            args[4] = "0";
            args[5] = "/tmp/kafka-streams";
        }

        Properties properties = new Properties();
        properties.put(StreamsConfig.APPLICATION_ID_CONFIG, "KafkaStreamsWordCountApplication");//UUID.randomUUID().toString()
        properties.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, args[0]);
        properties.put(StreamsConfig.ZOOKEEPER_CONNECT_CONFIG, args[1]);
        properties.put(StreamsConfig.CLIENT_ID_CONFIG, args[2]);
        properties.put(StreamsConfig.KEY_SERDE_CLASS_CONFIG, Serdes.String().getClass().getName());
        properties.put(StreamsConfig.VALUE_SERDE_CLASS_CONFIG, Serdes.String().getClass().getName());
        properties.put(StreamsConfig.STATE_DIR_CONFIG, args[5]);
        properties.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        properties.put(StreamsConfig.NUM_STREAM_THREADS_CONFIG, args[2]);
        //properties.put(StreamsConfig.REPLICATION_FACTOR_CONFIG, 0);
        //properties.put(StreamsConfig.TIMESTAMP_EXTRACTOR_CLASS_CONFIG, WallclockTimestampExtractor.class);
        //properties.put(StreamsConfig.CACHE_MAX_BYTES_BUFFERING_CONFIG, 0);

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
            .groupByKey(Serdes.String(), Serdes.serdeFrom(new Tuple2Serializer(), new Tuple2Deserializer()))
            .reduce(new Reducer<Tuple2>() {
                @Override
                public Tuple2 apply(Tuple2 a, Tuple2 b) {
                    return new Tuple2(a.count + b.count, Math.max(a.timestamp, b.timestamp));
                }
            }, "reduction")
            .mapValues(record -> String.format("%d\t%d\t%d", record.timestamp, System.currentTimeMillis(), record.count))
            .writeAsText(args[3] + args[4]);

        KafkaStreams streams = new KafkaStreams(builder, properties);
        streams.cleanUp();
        streams.start();
        Runtime.getRuntime().addShutdownHook(new Thread(streams::close));
    }
}

