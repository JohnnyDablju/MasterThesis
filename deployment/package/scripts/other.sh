# mounting ebs
sudo mkdir /mt
sudo mount /dev/xvdf /mt
sudo chown -R ec2-user /mt
sudo chmod -R 700 /mt
# formatting ebs
sudo mkfs -t ext4 /dev/xvdf
# counting number of messages per broker
${packageDir}/kafka/bin/kafka-run-class.sh kafka.tools.GetOffsetShell \
--broker-list ${kafkaHosts} \
--topic ${kafkaTopic} \
--time -1
# checking available free disk space
pssh -i \
-h ${streamHostsPath} \
-h ${kafkaHostsPath} \
-H ${masterHost} \
-x "-i ${keyPairPath}" df
