# copying output data back to master node
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
scp -o StrictHostKeyChecking=no -i ${keyPairPath} -rp \
${dataDir}/Flink ec2-user@${masterIp}:${dataDir}
# zipping data
tar -cjf ${dataDir}/flinkoutput.tar.bz2 ${dataDir}/Flink