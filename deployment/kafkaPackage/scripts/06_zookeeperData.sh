# copying output data back to master node
pssh -i \
-h ${kafkaHostsPath} \
-x "-i ${keyPairPath}" \
scp -o StrictHostKeyChecking=no -i ${keyPairPath} -rp \
${dataDir}/KafkaStreams ec2-user@${masterIp}:${dataDir}
# zipping data
tar -cjf ${dataDir}/ksoutput.tar.bz2 ${dataDir}/KafkaStreams