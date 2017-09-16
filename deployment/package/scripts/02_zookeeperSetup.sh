# setting up variables
user="ec2-user"
efsDns="fs-5c22ca25.efs.us-east-2.amazonaws.com"
# zookeeper
zookeeperIp="172.31.8.242"
zookeeperFullIp=${zookeeperIp}":2181"
zookeeperHost=${user}@${zookeeperIp}
# other ips
masterIp="172.31.4.138"
masterHost=${user}@${masterIp}
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
echo 'zookeeper.connect='${zookeeperFullIp} | tee -a ${packageDir}/config/server*.properties
# starting the zookeeper server in separate tab
screen -d -m ${packageDir}/kafka/bin/zookeeper-server-start.sh \
${packageDir}/config/zookeeper.properties