1. deploy instances
2. attach volume (format if needed)
3. mount volume in master instance
sudo mkdir /mt
sudo mount /dev/xvdf /mt
sudo chown -R ec2-user /mt
sudo chmod -R 700 /mt
4. complete information about hosts in file and zookeeperSetup
5. sync those files
6. assign rights back to uploaded files
sudo chown -R ec2-user /mt
sudo chmod -R 700 /mt
7. run initial
/mt/package/scripts/01_setup.sh
source /mt/package/scripts/02_zookeeperSetup.sh
8. run scripts in order
source ${packageDir}/scripts/03_workerSetup.sh