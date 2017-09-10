# creating kafka streams directory
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
mkdir ${dataDir}/KafkaStreams
# starting kafka streams
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
screen -d -m java -jar ${packageDir}/jars/KafkaStreams.jar \
${kafkaHosts} ${kafkaTopic} ${dataDir}/KafkaStreams/ '${PSSH_NODENUM}' 4 0 0
# checking status
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
ls -l ${dataDir}/KafkaStreams
