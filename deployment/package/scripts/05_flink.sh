# starting flink cluster
ssh -i ${keyPairPath} ${masterHost} \
${packageDir}/flink/bin/start-cluster.sh
# creating Flink output folder
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
mkdir ${dataDir}/Flink
# starting WordCount application
ssh -i ${keyPairPath} ${masterHost} \
${packageDir}/flink/bin/flink run -d ${packageDir}/jars/Flink.jar \
${kafkaHosts} ${kafkaTopic}Fa ${dataDir}/Flink 16
# starting StockTweetJoin application
ssh -i ${keyPairPath} ${masterHost} \
${packageDir}/flink/bin/flink run -d ${packageDir}/jars/Flink.jar \
${kafkaHosts} ${tweetsTopic}Fa,${stocksTopic}Fa ${dataDir}/Flink ${dataDir}/companies 16
# checking status
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
ls -l ${dataDir}/Flink
# stopping cluster
ssh -i ${keyPairPath} ${masterHost} \
${packageDir}/flink/bin/stop-cluster.sh
