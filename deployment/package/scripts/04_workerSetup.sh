# copying package
copy(){
ssh -i ${keyPairPath} -o StrictHostKeyChecking=no ${1} "sudo mkdir -p /mt/data; sudo chown -R ec2-user /mt" < /dev/null
scp -i ${keyPairPath} -o StrictHostKeyChecking=no -pqr ${packageDir} ${1}:/mt
ssh -i ${keyPairPath} ${1} "sudo chown -R ec2-user /mt" < /dev/null
}
copy ec2-user@${flinkMasterHost}
fileNames=(kafka stream)
for fileName in ${fileNames[@]};
do
while read host;
copy ${host}
do
done < ${packageDir}/config/${fileName}.hosts
done
# applying global setup
pssh -i \
-h ${kafkaHostsPath} \
-h ${streamHostsPath} \
-H ec2-user@${flinkMasterHost} \
-x "-i ${keyPairPath}" \
${packageDir}/scripts/01_setup.sh