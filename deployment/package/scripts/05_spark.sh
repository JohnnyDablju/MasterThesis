# creating spark output folder
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
mkdir ${dataDir}/Spark
# starting cluster
ssh -i ${keyPairPath} ${masterHost} \
${packageDir}/spark/sbin/start-all.sh
# starting application 
ssh -i ${keyPairPath} ${masterHost} \
${packageDir}/spark/bin/spark-submit \
${packageDir}/jars/Spark/Spark.jar \
${kafkaHosts} 1000 ${packageDir}/hadoop/ ${dataDir}/Spark/
# checking status
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
ls -l ${dataDir}/Spark