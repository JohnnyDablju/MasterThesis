import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.Producer;
import org.apache.kafka.clients.producer.ProducerConfig;
import org.apache.kafka.clients.producer.ProducerRecord;

import java.io.File;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import java.util.Scanner;

public class WordCountProducer {
    public static void main(String[] args) throws Exception {
        Properties props = new Properties();
        props.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, "localhost:9092");
        props.put(ProducerConfig.ACKS_CONFIG, "0");
        props.put(ProducerConfig.RETRIES_CONFIG, 0);
        props.put(ProducerConfig.BATCH_SIZE_CONFIG, 16384);
        props.put(ProducerConfig.LINGER_MS_CONFIG, 1);
        props.put(ProducerConfig.BUFFER_MEMORY_CONFIG, 33554432);
        props.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, "org.apache.kafka.common.serialization.StringSerializer");
        props.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, "org.apache.kafka.common.serialization.StringSerializer");

        System.out.println("Loading input started...");
        List<String> tweets = new ArrayList<>();
        Scanner input = new Scanner(new File("C:\\Git\\MasterThesis\\Experiments\\WordCount\\input.txt"));
        while (input.hasNextLine()){
            tweets.add(input.nextLine());
        }
        System.out.printf("Loading input finished. %d records loaded.\n", tweets.size());

        Producer<String, String> producer = new KafkaProducer<>(props);
        for (int i = 0; i < 3000000/*tweets.size()*/; i++){
            if (i % 10 == 0)
                System.out.printf("%d records have been produced.\n", i+1);
            producer.send(new ProducerRecord<String, String>("WordCountInput",/*0, System.currentTimeMillis(),*/ null, tweets.get(i)));
        }
        producer.close();
    }
}
