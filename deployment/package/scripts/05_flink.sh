# starting flink cluster
ssh -i ${keyPairPath} ${masterHost} \
${packageDir}/flink/bin/start-cluster.sh
# creating Flink output folder
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
mkdir ${dataDir}/Flink
# starting application
ssh -i ${keyPairPath} ${masterHost} \
${packageDir}/flink/bin/flink run -d ${packageDir}/jars/Flink.jar \
${kafkaHosts} 16 ${dataDir}/Flink
# checking status
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
ls -l ${dataDir}/Flink
