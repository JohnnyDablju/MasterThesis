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
${kafkaHosts} ${zookeeperFullIp} 8 ${dataDir}/KafkaStreams/ '${PSSH_NODENUM}' ${tmpDir}/kafka-streams
# checking status
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
ls -l ${dataDir}/KafkaStreams
