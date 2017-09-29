# starting kafka servers
pssh -i \
-h ${kafkaHostsPath} \
-x "-i ${keyPairPath}" \
screen -d -m ${packageDir}/kafka/bin/kafka-server-start.sh \
${packageDir}/config/server'${PSSH_NODENUM}'.properties
# WordCount topic creation
${packageDir}/kafka/bin/kafka-topics.sh --create \
--zookeeper ${zookeeperFullIp} \
--replication-factor 1 \
--partitions 32 \
--topic ${kafkaTopic}
# StockTweetJoin topics creation
${packageDir}/kafka/bin/kafka-topics.sh --create \
--zookeeper ${zookeeperFullIp} \
--replication-factor 1 \
--partitions 16 \
--topic ${tweetsTopic}Sa
${packageDir}/kafka/bin/kafka-topics.sh --create \
--zookeeper ${zookeeperFullIp} \
--replication-factor 1 \
--partitions 16 \
--topic ${stocksTopic}Sa
# feeding tweets to kafka
java -jar ${packageDir}/jars/TweetsProducer.jar \
${kafkaHosts} ${tweetsTopic}Sa ${dataDir}/0,${dataDir}/1,${dataDir}/2,${dataDir}/3 2
# feeding stocks to kafka
java -jar ${packageDir}/jars/StocksProducer.jar \
${kafkaHosts} ${stocksTopic}Sa ${dataDir}/companies
