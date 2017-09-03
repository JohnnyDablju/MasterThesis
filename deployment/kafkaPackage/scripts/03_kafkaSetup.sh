# preparing kafka config
sed -ie '$d' ${packageDir}/config/server*.properties
echo 'zookeeper.connect='${zookeeperHost} | tee -a ${packageDir}/config/server*.properties
# copying package
while read host
do
ssh -i ${keyPairPath} -o StrictHostKeyChecking=no ${host} "sudo mkdir -p /mt/data; sudo chown -R ec2-user /mt" < /dev/null
scp -i ${keyPairPath} -o StrictHostKeyChecking=no -pqr ${packageDir} ${host}:/mt
ssh -i ${keyPairPath} ${host} "sudo chown -R ec2-user /mt" < /dev/null
done < ${packageDir}/config/kafka.hosts
# applying global setup
pssh -i \
-h ${kafkaHostsPath} \
-x "-i ${keyPairPath}" \
${packageDir}/scripts/01_setup.sh
# starting kafka servers
pssh -i \
-h ${kafkaHostsPath} \
-x "-i ${keyPairPath}" \
screen -d -m ${packageDir}/kafka/bin/kafka-server-start.sh \
${packageDir}/config/server'${PSSH_NODENUM}'.properties