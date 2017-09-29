# setting master in flink
sed -ie '$d' ${packageDir}/flink/conf/flink-conf.yaml
echo 'jobmanager.rpc.address: '${masterIp} >> ${packageDir}/flink/conf/flink-conf.yaml
echo ${masterIp}:8081 > ${packageDir}/flink/conf/masters
# setting spark config
sparkConfigPath=${packageDir}/spark/conf/spark-defaults.conf
sed -ie '$d' ${sparkConfigPath}
sed -ie '$d' ${sparkConfigPath}
sed -ie '$d' ${sparkConfigPath}
printf "spark.master spark://${masterIp}:7077\n" >> ${sparkConfigPath}
printf "spark.driver.extraClassPath ${packageDir}/jars/Spark/*\n" >> ${sparkConfigPath}
printf "spark.executor.extraClassPath ${packageDir}/jars/Spark/*" >> ${sparkConfigPath}
# setting slaves ip
dirNames=(spark flink)
for dirName in ${dirNames[@]};
do
>${packageDir}/${dirName}/conf/slaves
while read host;
do
printf "${host#*@}\n" >> ${packageDir}/${dirName}/conf/slaves
done < ${packageDir}/config/stream.hosts
done
# copying package
copyPackage(){
ssh -i ${keyPairPath} -o StrictHostKeyChecking=no ${1} "sudo mkdir -p /mt/data; sudo chown -R ${user} /mt" < /dev/null
scp -i ${keyPairPath} -o StrictHostKeyChecking=no -pqr ${packageDir} ${1}:/mt
scp -i ${keyPairPath} -o StrictHostKeyChecking=no -pqr ${dataDir}/companies ${1}:${dataDir}
ssh -i ${keyPairPath} ${1} "sudo chown -R ${user} /mt" < /dev/null
}
copyPackage ${masterHost}
fileNames=(stream kafka)
for fileName in ${fileNames[@]};
do
while read host;
do
copyPackage ${host}
done < ${packageDir}/config/${fileName}.hosts
done
# applying global setup
pssh -i \
-h ${kafkaHostsPath} \
-h ${streamHostsPath} \
-H ${masterHost} \
-x "-i ${keyPairPath} -o StrictHostKeyChecking=no" \
${packageDir}/scripts/01_setup.sh
# preparing authorization for processing cluster
ssh -i ${keyPairPath} ${masterHost} \
"ssh-keygen -b 2048 -P '' -f ~/.ssh/id_rsa; cat .ssh/id_rsa.pub >> .ssh/authorized_keys"
while read host;
do
ssh -i ${keyPairPath} ${masterHost} \
"scp -o StrictHostKeyChecking=no -i ${keyPairPath} .ssh/authorized_keys ${host}:~/.ssh/" \
< /dev/null
done < ${packageDir}/config/stream.hosts

