import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.Producer;
import org.apache.kafka.clients.producer.ProducerRecord;

import java.io.File;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import java.util.Scanner;

public class WordCountProducer {
    public static void main(String[] args) throws Exception {
        Properties props = new Properties();
        props.put("bootstrap.servers", "localhost:9092");
        props.put("acks", "0");
        props.put("retries", 0);
        props.put("batch.size", 16384);
        props.put("linger.ms", 1);
        props.put("buffer.memory", 33554432);
        props.put("key.serializer", "org.apache.kafka.common.serialization.StringSerializer");
        props.put("value.serializer", "org.apache.kafka.common.serialization.StringSerializer");

        System.out.println("Loading input started...");
        List<String> tweets = new ArrayList<>();
        Scanner input = new Scanner(new File("C:\\Git\\MasterThesis\\WordCount\\input.txt"));
        while (input.hasNextLine()){
            tweets.add(input.nextLine());
        }
        System.out.println("Loading input finished.");

        Producer<String, String> producer = new KafkaProducer<>(props);
        for (int i = 0; i < tweets.size(); i++){
            if (i % 49999 == 0)
                System.out.printf("%d records have been produced.\n", i+1);
            producer.send(new ProducerRecord<String, String>("WordCountInput",0, System.currentTimeMillis(), null, tweets.get(i)));
        }
        producer.close();
    }
}
