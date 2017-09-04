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
        if (args == null){
            args = new String[3];
            args[0] = "localhost:9092";
            args[1] = "C:\\Git\\MasterThesis\\Experiments\\WordCount\\input.txt";
            args[2] = "WordCountInput";
        }

        Properties properties = new Properties();
        properties.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, args[0]);
        properties.put(ProducerConfig.ACKS_CONFIG, "0");
        properties.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, "org.apache.kafka.common.serialization.StringSerializer");
        properties.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, "org.apache.kafka.common.serialization.StringSerializer");

        System.out.println("Loading input started...");
        List<String> tweets = new ArrayList<>();
        Scanner input = new Scanner(new File(args[1]));
        while (input.hasNextLine()) {
            tweets.add(input.nextLine());
        }
        System.out.printf("Loading input finished. %d records loaded.\n", tweets.size());

        Producer<String, String> producer = new KafkaProducer<>(properties);
        for (int i = 0; i < tweets.size(); i++){
            if (i % 1000000 == 0)
                System.out.printf("%d records have been produced.\n", i+1);
            producer.send(new ProducerRecord<String, String>(args[2], tweets.get(i)));
        }
        producer.close();
        System.out.println("Producing records finished.");
    }
}
