# formatting ebs
sudo mkfs -t ext4 /dev/xvdf
# counting number of messages per broker
${packageDir}/kafka/bin/kafka-run-class.sh kafka.tools.GetOffsetShell \
--broker-list ${kafkaHosts} \
--topic ${kafkaTopic} \
--time -1