# starting kafka servers
pssh -i \
-h ${kafkaHostsPath} \
-x "-i ${keyPairPath}" \
screen -d -m ${packageDir}/kafka/bin/kafka-server-start.sh \
${packageDir}/config/server'${PSSH_NODENUM}'.properties
#topic creation
${packageDir}/kafka/bin/kafka-topics.sh --create \
--zookeeper ${zookeeperFullIp} \
--replication-factor 1 \
--partitions 16 \
--topic ${kafkaTopic}
# feeding data to kafka
java -jar ${packageDir}/jars/Kafka.jar \
${kafkaHosts} ${kafkaTopic} ${dataDir}/0,${dataDir}/1,${dataDir}/2,${dataDir}/3