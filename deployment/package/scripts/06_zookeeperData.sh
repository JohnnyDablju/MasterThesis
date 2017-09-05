# copying output data back to master node
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
scp -o StrictHostKeyChecking=no -i ${keyPairPath} -rp \
${dataDir}/Spark ${zookeeperHost}:${dataDir}/'${PSSH_NODENUM}'/
# zipping data
tar -cjf ${dataDir}/flinkoutput.tar.bz2 ${dataDir}/Flink