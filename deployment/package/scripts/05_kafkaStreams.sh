# creating kafka streams directory
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
mkdir ${dataDir}/KafkaStreams
# starting WordCount application
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
screen -d -m java -jar ${packageDir}/jars/KafkaStreams.jar \
${kafkaHosts} ${kafkaTopic}KSa ${dataDir}/KafkaStreams/ '${PSSH_NODENUM}' 4 0 0
# starting StockTweetJoin application
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
screen -d -m java -jar ${packageDir}/jars/KafkaStreams.jar \
${kafkaHosts} ${tweetsTopic}KSa,${stocksTopic}KSa ${dataDir}/KafkaStreams/ ${dataDir}/companies '${PSSH_NODENUM}' 4 0 0
# checking status
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
ls -l ${dataDir}/KafkaStreams
