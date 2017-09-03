# creating kafka streams directory
pssh -i \
-h ${kafkaHostsPath} \
-x "-i ${keyPairPath}" \
mkdir ${dataDir}/KafkaStreams
# starting kafka streams
pssh -i \
-h ${kafkaHostsPath} \
-x "-i ${keyPairPath}" \
screen -d -m java -jar ${packageDir}/jars/KafkaStreams.jar \
${kafkaHosts} ${zookeeperHost} 4 ${dataDir}/KafkaStreams/ '${PSSH_NODENUM}' ${tmpDir}/kafka-streams
# checking status
pssh -i \
-h ${kafkaHostsPath} \
-x "-i ${keyPairPath}" \
ls -l ${dataDir}/KafkaStreams
