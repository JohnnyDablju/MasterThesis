import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.KeyValue;
import org.apache.kafka.streams.StreamsConfig;
import org.apache.kafka.streams.kstream.*;

import java.util.*;

public class WordCount {
    public static void main(String[] args) throws Exception {
        if (args == null || args.length == 0){
            args = new String[7];
            args[0] = "localhost:9092"; // brokers
            args[1] = "WordCountInput"; // topic
            args[2] = "C:\\Git\\MasterThesis\\experiments\\_singleWordCount\\KafkaStreams\\"; // output directory
            args[3] = "0"; // client id / output file name
            args[4] = "2"; // threads number
            args[5] = "0"; // cache bytes
            args[6] = "0"; // processing id
        }

        Properties properties = new Properties();
        properties.put(StreamsConfig.APPLICATION_ID_CONFIG, "KSWCA_" + args[6]);
        properties.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, args[0]);
        properties.put(StreamsConfig.CLIENT_ID_CONFIG, args[3]);
        properties.put(StreamsConfig.KEY_SERDE_CLASS_CONFIG, Serdes.String().getClass().getName());
        properties.put(StreamsConfig.VALUE_SERDE_CLASS_CONFIG, Serdes.String().getClass().getName());
        properties.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        properties.put(StreamsConfig.NUM_STREAM_THREADS_CONFIG, args[4]);
        properties.put(StreamsConfig.CACHE_MAX_BYTES_BUFFERING_CONFIG, args[5]);
        //properties.put(ConsumerConfig.SESSION_TIMEOUT_MS_CONFIG, 30 * 60 * 1000);
        //properties.put(ConsumerConfig.REQUEST_TIMEOUT_MS_CONFIG, 30 * 60 * 1000 + 1);
        //properties.put(ConsumerConfig.MAX_POLL_INTERVAL_MS_CONFIG, 30 * 60 * 1000);
        //properties.put(StreamsConfig.REPLICATION_FACTOR_CONFIG, 0);
        //properties.put(StreamsConfig.TIMESTAMP_EXTRACTOR_CLASS_CONFIG, WallclockTimestampExtractor.class);


        KStreamBuilder builder = new KStreamBuilder();
        KStream<String, String> source = builder.stream(Serdes.String(), Serdes.String(), args[1]);

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
            .writeAsText(args[2] + args[3]);

        KafkaStreams streams = new KafkaStreams(builder, properties);
        streams.cleanUp();
        streams.start();
        Runtime.getRuntime().addShutdownHook(new Thread(streams::close));
    }
}

