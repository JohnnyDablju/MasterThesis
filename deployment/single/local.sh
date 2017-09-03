# start zookeeper
./zookeeper-server-start C:\Git\MasterThesis\Config\zookeeper.properties
# start server
./kafka-server-start C:\Git\MasterThesis\Config\server.properties
# create topics
./kafka-topics --create --zookeeper localhost:2181 --replication-factor 1 --partitions 1 --topic WordCountInput
./kafka-topics --create --zookeeper localhost:2181 --replication-factor 1 --partitions 1 --topic WordCountOutput

cat C:\Git\MasterThesis\Helpers\TweetsParser\res\10\00.txt | ./kafka-console-producer --broker-list localhost:9092 --topic WordCountInput
./kafka-run-class C:\Git\MasterThesis\KafkaStreams\out\production\KafkaStreams\WordCount.class
./kafka-console-consumer --zookeeper localhost:2181 --topic WordCountOutput --from-beginning --formatter kafka.tools.DefaultMessageFormatter --property value.deserializer=org.apache.kafka.common.serialization.StringDeserializer --property print.timestamp=true > C:\Git\MasterThesis\WordCount\output.txt