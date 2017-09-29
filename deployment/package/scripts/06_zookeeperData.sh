# copying output data back to master node
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
scp -o StrictHostKeyChecking=no -i ${keyPairPath} -rp \
${dataDir}/Spark ${zookeeperHost}:${dataDir}
# removing data from remote nodes
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
rm -rf ${dataDir}/Spark
# zipping data
tar -cjf ${dataDir}/spark.tar.bz2 ${dataDir}/Spark
