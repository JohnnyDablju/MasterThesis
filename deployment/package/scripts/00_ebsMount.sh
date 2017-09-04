# mounting ebs
sudo mkdir /mt
sudo mount /dev/xvdf /mt
sudo chown -R ec2-user /mt
sudo chmod -R 700 /mt