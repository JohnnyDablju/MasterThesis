import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.common.serialization.Serde;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.KeyValue;
import org.apache.kafka.streams.StreamsConfig;
import org.apache.kafka.streams.kstream.*;

import java.io.File;
import java.util.*;
import java.util.concurrent.TimeUnit;

public class StockTweetJoin
{
    public static void main(String[] args) throws Exception {
        if (args == null || args.length == 0){
            args = new String[8];
            args[0] = "localhost:9092"; // brokers
            args[1] = "TwitterKSd,NasdaqKSd"; // tweets,stocks topics
            args[2] = "C:\\Git\\MasterThesis\\experiments\\_singleStockTweetJoin\\KafkaStreams\\"; // output directory
            args[3] = "C:\\Git\\MasterThesis\\deployment\\data\\companies"; // companies file
            args[4] = "0"; // client id / output file name
            args[5] = "2"; // threads number
            args[6] = "0"; // cache bytes
            args[7] = "0"; // processing id
        }

        String[] topics = args[1].split(",");

        Properties properties = new Properties();
        properties.put(StreamsConfig.APPLICATION_ID_CONFIG, "KSWCA_" + args[7]);
        properties.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, args[0]);
        properties.put(StreamsConfig.CLIENT_ID_CONFIG, args[4]);
        properties.put(StreamsConfig.KEY_SERDE_CLASS_CONFIG, Serdes.String().getClass().getName());
        properties.put(StreamsConfig.VALUE_SERDE_CLASS_CONFIG, Serdes.String().getClass().getName());
        properties.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        properties.put(StreamsConfig.NUM_STREAM_THREADS_CONFIG, args[5]);
        properties.put(StreamsConfig.CACHE_MAX_BYTES_BUFFERING_CONFIG, args[6]);

        Map<String, String[]> companies = new HashMap<>();
        Scanner scanner = new Scanner(new File(args[3]));
        while (scanner.hasNextLine()){
            String[] fields = scanner.nextLine().split("\t");
            companies.put(fields[0], fields[1].split(","));
        }

        KStreamBuilder builder = new KStreamBuilder();

        KStream<String, Long> tweets = builder
            .stream(Serdes.String(), Serdes.String(), topics[0])
            .map(new KeyValueMapper<String, String, KeyValue<String, Long>>() {
                @Override
                public KeyValue<String, Long> apply(String key, String tweet) {
                    long currentTimestamp = System.currentTimeMillis();
                    String companyIndex = null;
                    List<String> tweetWords = Arrays.asList(tweet.toLowerCase().split("\\W+"));
                    Iterator<Map.Entry<String, String[]>> companyIterator = companies.entrySet().iterator();
                    while (companyIterator.hasNext() && companyIndex == null){
                        Map.Entry<String, String[]> company = companyIterator.next();
                        for (String phrase : company.getValue()){
                            List<String> nameWords = new ArrayList<String>(Arrays.asList(phrase.split(" ")));
                            int wordCount = nameWords.size();
                            nameWords.retainAll(tweetWords);
                            if (nameWords.size() == wordCount){
                                companyIndex = company.getKey();
                                break;
                            }
                        }
                    }
                    return new KeyValue<>(companyIndex, currentTimestamp);
                }
            })
            .filter((key, value) -> key != null);

        KStream<String, Long> stocks = builder
            .stream(Serdes.String(), Serdes.String(), topics[1])
            .map(new KeyValueMapper<String, String, KeyValue<String, Long>>() {
                @Override
                public KeyValue<String, Long> apply(String key, String stockIndex) {
                    return new KeyValue<>(stockIndex.split("\t")[0], System.currentTimeMillis());
                }
            });

        tweets
            .join(stocks, new ValueJoiner<Long, Long, String>() {
                @Override
                public String apply(Long tweetTimestamp, Long stockTimestamp) {
                    return String.format("%d\t%d", tweetTimestamp, stockTimestamp);
                }
            }, JoinWindows.of(TimeUnit.SECONDS.toMillis(5)), Serdes.String(), Serdes.Long(), Serdes.Long())
            .mapValues(record -> String.format("%s\t%d", record, System.currentTimeMillis()))
            .writeAsText(args[2] + args[4]);

        KafkaStreams streams = new KafkaStreams(builder, properties);
        streams.cleanUp();
        streams.start();
        Runtime.getRuntime().addShutdownHook(new Thread(streams::close));
    }
}
