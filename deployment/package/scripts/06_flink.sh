# preparing authorization for flink cluster
ssh -i ${keyPairPath} ec2-user@${flinkMasterHost} \
"ssh-keygen -b 2048 -P '' -f ~/.ssh/id_rsa; cat .ssh/id_rsa.pub >> .ssh/authorized_keys"
while read host;
do
ssh -i ${keyPairPath} ec2-user@${flinkMasterHost} \
"scp -o StrictHostKeyChecking=no -i ${keyPairPath} .ssh/authorized_keys ${host}:~/.ssh/" \
< /dev/null
done < ${packageDir}/config/stream.hosts
# starting flink cluster
ssh -i ${keyPairPath} ec2-user@${flinkMasterHost} \
${packageDir}/flink/bin/start-cluster.sh
# creating Flink output folder
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
mkdir ${dataDir}/Flink
# starting application
ssh -i ${keyPairPath} ec2-user@${flinkMasterHost} \
${packageDir}/flink/bin/flink run -d ${packageDir}/jars/Flink.jar \
${kafkaHosts} 16 ${dataDir}/Flink
# checking status
pssh -i \
-h ${streamHostsPath} \
-x "-i ${keyPairPath}" \
ls -l ${dataDir}/Flink
