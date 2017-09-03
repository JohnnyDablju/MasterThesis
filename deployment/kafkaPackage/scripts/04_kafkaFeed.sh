# topic creation
${packageDir}/kafka/bin/kafka-topics.sh --create \
--zookeeper ${zookeeperHost} \
--replication-factor 1 \
--partitions 16 \
--topic ${kafkaTopic}
# feeding data to kafka
java -jar ${packageDir}/jars/Kafka.jar \
${kafkaHosts} ${dataDir}/00 ${kafkaTopic}