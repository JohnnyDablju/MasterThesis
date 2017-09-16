import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.Producer;
import org.apache.kafka.clients.producer.ProducerConfig;
import org.apache.kafka.clients.producer.ProducerRecord;

import java.io.File;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import java.util.Scanner;

import static java.lang.Thread.sleep;

public class WordCountProducer {
    public static void main(String[] args) throws Exception {
        if (args == null || args.length == 0) {
            args = new String[3];
            args[0] = "localhost:9092"; // brokers
            args[1] = "WordCountInput"; // topic
            args[2] = "C:\\Git\\MasterThesis\\deployment\\data\\0"; // input paths
            args[3] = "14"; // production rate
        }

        Properties properties = new Properties();
        properties.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, args[0]);
        properties.put(ProducerConfig.ACKS_CONFIG, "0");
        properties.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, "org.apache.kafka.common.serialization.StringSerializer");
        properties.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, "org.apache.kafka.common.serialization.StringSerializer");

        String[] files = args[2].split(",");
        int productionRate = Integer.parseInt(args[3]);
        int counter = 0;
        Producer<String, String> producer = new KafkaProducer<>(properties);

        Long startTimestamp = System.currentTimeMillis();
        for (int i = 0; i < files.length; i++) {
            System.out.printf("Loading input %d started...\n", i);
            Scanner input = new Scanner(new File(files[i]));
            while (input.hasNextLine()) {
                producer.send(new ProducerRecord<String, String>(args[1], input.nextLine()));
                if (counter % productionRate == 0){
                    sleep(1);
                    if (counter % 5000 == 0){
                        System.out.printf("%d records have been produced.\n", counter);
                    }
                }
                counter++;
            }
            System.out.printf("Loading input %d finished.\n", i);
        }
        producer.close();

        System.out.printf("Producing records finished in %d.\n", System.currentTimeMillis() - startTimestamp);
    }
}