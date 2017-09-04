# starting kafka servers
pssh -i \
-h ${kafkaHostsPath} \
-x "-i ${keyPairPath}" \
screen -d -m ${packageDir}/kafka/bin/kafka-server-start.sh \
${packageDir}/config/server'${PSSH_NODENUM}'.properties
#topic creation
${packageDir}/kafka/bin/kafka-topics.sh --create \
--zookeeper ${zookeeperHost} \
--replication-factor 1 \
--partitions 32 \
--topic ${kafkaTopic}
# feeding data to kafka
java -jar ${packageDir}/jars/Kafka.jar \
${kafkaHosts} ${dataDir}/00 ${kafkaTopic}