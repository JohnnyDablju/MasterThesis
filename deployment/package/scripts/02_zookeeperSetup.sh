# setting up variables
# zookeeper
masterIp="172.31.10.163"
zookeeperHost=${masterIp}":2181"
# other ips
flinkMasterHost="172.31.5.212"
# directories
packageDir="/mt/package"
tmpDir="/tmp"
dataDir="/mt/data"
# kafka hosts
kafkaHosts=""
while read host
do
kafkaHosts=${kafkaHosts}${host#*@}":9092,"
done < ${packageDir}/config/kafka.hosts
kafkaHosts=${kafkaHosts::-1}
# kafka
kafkaTopic="WordCountInput"
# paths
keyPairPath=${packageDir}"/keyPairs/MTKeyPairEC2Ohio.pem"
kafkaHostsPath=${packageDir}"/config/kafka.hosts"
streamHostsPath=${packageDir}"/config/stream.hosts"
# installing pssh
echo "y" | sudo yum install pssh
# allowing ssh calls from zookeeper
sudo chmod -R 600 ${keyPairPath}
# preparing kafka config
sed -ie '$d' ${packageDir}/config/server*.properties
echo 'zookeeper.connect='${zookeeperHost} | tee -a ${packageDir}/config/server*.properties
# starting the zookeeper server in separate tab
screen -d -m ${packageDir}/kafka/bin/zookeeper-server-start.sh \
${packageDir}/config/zookeeper.properties