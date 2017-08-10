import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.apache.kafka.common.serialization.StringDeserializer;
import org.apache.spark.SparkConf;
import org.apache.spark.api.java.function.FlatMapFunction;
import org.apache.spark.api.java.function.Function;
import org.apache.spark.streaming.Duration;
import org.apache.spark.streaming.api.java.JavaStreamingContext;
import org.apache.spark.streaming.kafka010.ConsumerStrategies;
import org.apache.spark.streaming.kafka010.KafkaUtils;
import org.apache.spark.streaming.kafka010.LocationStrategies;
import scala.Tuple2;

import java.util.*;

public final class WordCount {
    public static void main(String[] args) throws Exception {
        Map<String, Object> kafkaParams = new HashMap<>();
        kafkaParams.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, "localhost:9092");
        kafkaParams.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class);
        kafkaParams.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class);
        kafkaParams.put(ConsumerConfig.GROUP_ID_CONFIG, UUID.randomUUID().toString());
        kafkaParams.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        kafkaParams.put("enable.auto.commit", false);

        System.setProperty("hadoop.home.dir", "C:/Apache/hadoop/");

        Collection<String> topics = Arrays.asList("WordCountInput");

        SparkConf sparkConfig = new SparkConf()
            .setMaster("local[*]")
            .setAppName("SparkWordCount");
        JavaStreamingContext streamingContext = new JavaStreamingContext(sparkConfig, new Duration(2000));

        KafkaUtils
            .createDirectStream(streamingContext, LocationStrategies.PreferConsistent(), ConsumerStrategies.<String, String>Subscribe(topics, kafkaParams))
            .flatMap(new FlatMapFunction<ConsumerRecord<String,String>, Tuple2<String, Long>>() {
                @Override
                public Iterator<Tuple2<String, Long>> call(ConsumerRecord<String, String> message) throws Exception {
                    ArrayList<Tuple2<String, Long>> list = new ArrayList<>();
                    Long currentTimestamp = System.currentTimeMillis();
                    for (String word : message.value().toLowerCase().split("\\W+")){
                        list.add(new Tuple2<>(message.value(), currentTimestamp));
                    }
                    return list.iterator();
                }
            })
            .mapToPair(record -> new Tuple2<>(record._1, new Tuple2<>(1, record._2)))
            .reduceByKey((a, b) -> new Tuple2<>(a._1 + b._1, Math.max(a._2, b._2)))
            .map(record -> String.format("%d\t%d\t%s\t%d", record._2._2, System.currentTimeMillis(), record._1, record._2._1))
            .dstream()
            .saveAsTextFiles("C:\\Git\\MasterThesis\\Experiments\\WordCount\\spark\\", "");

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