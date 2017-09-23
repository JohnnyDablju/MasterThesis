import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.Producer;
import org.apache.kafka.clients.producer.ProducerConfig;
import org.apache.kafka.clients.producer.ProducerRecord;

import java.io.File;
import java.util.*;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

public class StocksProducer {
    public static void main(String[] args) throws Exception {
        if (args == null || args.length == 0) {
            args = new String[3];
            args[0] = "localhost:9092"; // brokers
            args[1] = "NasdaqKSd"; // topic
            args[2] = "C:\\Git\\MasterThesis\\deployment\\data\\companies"; // input path
        }

        Properties properties = new Properties();
        properties.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, args[0]);
        properties.put(ProducerConfig.ACKS_CONFIG, "0");
        properties.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, "org.apache.kafka.common.serialization.StringSerializer");
        properties.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, "org.apache.kafka.common.serialization.StringSerializer");

        final String topic = args[1];

        System.out.println("Loading companies...");
        Map<String, Float> companies = new HashMap<>();
        Scanner scanner = new Scanner(new File(args[2]));
        while (scanner.hasNextLine()){
            String[] fields = scanner.nextLine().split("\t");
            companies.put(fields[0], Float.parseFloat(fields[2]));
        }
        System.out.println("Companies loaded.");

        Runnable produceStocks = new Runnable() {
            public void run() {
                System.out.println("Producing stocks...");
                Producer<String, String> producer = new KafkaProducer<>(properties);
                Random random = new Random();
                Iterator<Map.Entry<String, Float>> companyIterator = companies.entrySet().iterator();
                while (companyIterator.hasNext()){
                    Map.Entry<String, Float> companyIndex = companyIterator.next();
                    Float price = companyIndex.getValue() + random.nextFloat() * 0.1f - 0.05f;
                    String value = String.format("%s\t%s", companyIndex.getKey(), price.toString());
                    producer.send(new ProducerRecord<String, String>(topic, value));
                }
                producer.close();
                System.out.println("Stocks produced.");
            }
        };
        ScheduledExecutorService executor = Executors.newScheduledThreadPool(1);
        executor.scheduleAtFixedRate(produceStocks, 0, 10, TimeUnit.SECONDS);
    }
}