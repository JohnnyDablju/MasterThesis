# setting master ip
sed -ie '$d' ${packageDir}/flink/conf/flink-conf.yaml
echo 'jobmanager.rpc.address: '${flinkMasterHost} | tee -a ${packageDir}/flink/conf/flink-conf.yaml
echo ${flinkMasterHost}:8081 > ${packageDir}/flink/conf/masters
# setting slaves ip
>${packageDir}/flink/conf/slaves
while read host
do
printf "${host#*@}\n" >> ${packageDir}/flink/conf/slaves
done < ${packageDir}/config/stream.hosts