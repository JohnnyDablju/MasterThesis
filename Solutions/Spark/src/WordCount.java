import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.apache.kafka.common.serialization.StringDeserializer;
import org.apache.spark.SparkConf;
import org.apache.spark.api.java.function.FlatMapFunction;
import org.apache.spark.api.java.function.Function3;
import org.apache.spark.api.java.Optional;
import org.apache.spark.streaming.Duration;
import org.apache.spark.streaming.State;
import org.apache.spark.streaming.StateSpec;
import org.apache.spark.streaming.api.java.JavaStreamingContext;
import org.apache.spark.streaming.kafka010.ConsumerStrategies;
import org.apache.spark.streaming.kafka010.KafkaUtils;
import org.apache.spark.streaming.kafka010.LocationStrategies;
import scala.Tuple2;

import java.util.*;

public class WordCount {
    public static void main(String[] args) throws Exception {
        if (args == null || args.length == 0){
            args = new String[6];
            args[0] = "localhost:9092"; // brokers
            args[1] = "WordCountInput"; // topic
            args[2] = "C:\\Git\\MasterThesis\\experiments\\_singleWordCount\\Spark\\"; // output directory
            args[3] = "local[*]"; // master address
            args[4] = "1000"; // batch interval
            args[5] = "C:\\Apache\\hadoop\\"; // hadoop directory
        }

        Map<String, Object> kafkaParams = new HashMap<>();
        kafkaParams.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, args[0]);
        kafkaParams.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class);
        kafkaParams.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class);
        kafkaParams.put(ConsumerConfig.GROUP_ID_CONFIG, "SWCG_" + System.currentTimeMillis());
        kafkaParams.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        kafkaParams.put("enable.auto.commit", false);

        System.setProperty("hadoop.home.dir", args[5]);

        Collection<String> topics = Arrays.asList(args[1]);

        SparkConf sparkConfig = new SparkConf()
            .setMaster(args[3])
            .setAppName("SWCA_" + System.currentTimeMillis());

        JavaStreamingContext streamingContext = new JavaStreamingContext(sparkConfig, new Duration(Integer.parseInt(args[4])));
        streamingContext.checkpoint("/tmp/spark-checkpoint");


        KafkaUtils
            .createDirectStream(streamingContext, LocationStrategies.PreferConsistent(), ConsumerStrategies.<String, String>Subscribe(topics, kafkaParams))
            .flatMap(new FlatMapFunction<ConsumerRecord<String,String>, Tuple2<String, Long>>() {
                @Override
                public Iterator<Tuple2<String, Long>> call(ConsumerRecord<String, String> message) throws Exception {
                    ArrayList<Tuple2<String, Long>> list = new ArrayList<>();
                    Long currentTimestamp = System.currentTimeMillis();
                    for (String word : message.value().toLowerCase().split("\\W+")){
                        list.add(new Tuple2<>(word, currentTimestamp));
                    }
                    return list.iterator();
                }
            })
            .mapToPair(record -> new Tuple2<>(record._1, new Tuple2<>(1, record._2)))
            .mapWithState(StateSpec.function(new Function3<String, Optional<Tuple2<Integer, Long>>, State<Tuple2<Integer, Long>>, Tuple2<String, Tuple2<Integer, Long>>>() {
                @Override
                public Tuple2<String, Tuple2<Integer, Long>> call(String word, Optional<Tuple2<Integer, Long>> optionalValue, State<Tuple2<Integer, Long>> state) throws Exception {
                    Tuple2<Integer, Long> value = optionalValue.get();
                    if (state.exists()){
                        Tuple2<Integer, Long> existingValue = state.get();
                        value = new Tuple2<Integer, Long>(value._1 + existingValue._1, Math.max(value._2, existingValue._2));
                    }
                    state.update(value);
                    return new Tuple2<>(word, value);
                }
            }))
            //.reduceByKey((a, b) -> new Tuple2<>(a._1 + b._1, Math.max(a._2, b._2)))
            .map(record -> String.format("%d\t%d\t%s\t%d", record._2._2, System.currentTimeMillis(), record._1, record._2._1))
            .dstream()
            .saveAsTextFiles(args[2], "");


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
