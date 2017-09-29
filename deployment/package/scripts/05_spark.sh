# mounting efs
pssh -i \
-h ${streamHostsPath} \
-H ${masterHost} \
-x "-i ${keyPairPath}" \
${packageDir}/scripts/mountEfs.sh
# starting cluster
ssh -i ${keyPairPath} ${masterHost} \
${packageDir}/spark/sbin/start-all.sh
# logging into Spark to stop smoothly later on
ssh -i ${keyPairPath} ${masterHost}
# starting WordCount application
${packageDir}/spark/bin/spark-submit \
${packageDir}/jars/Spark/Spark.jar \
${kafkaHosts} ${kafkaTopic}Sa ${dataDir}/Spark/ spark://${masterIp}:7077 500 /efs/spark ${packageDir}/hadoop/
# starting StockTweetJoin application
${packageDir}/spark/bin/spark-submit \
${packageDir}/jars/Spark/Spark.jar \
${kafkaHosts} ${tweetsTopic}Sa,${stocksTopic}Sa{dataDir}/Spark/ ${dataDir}/companies spark://${masterIp}:7077 10000 /efs/spark ${packageDir}/hadoop/
# checking status
pssh -i \
-h ${streamHostsPath} \
-H ${masterHost} \
-x "-i ${keyPairPath}" \
du -sh ${dataDir}/Spark
# stopping cluster
ssh -i ${keyPairPath} ${masterHost} \
${packageDir}/spark/sbin/stop-all.sh
