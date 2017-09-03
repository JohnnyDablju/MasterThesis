# installing new java
echo "y" | sudo yum install java-1.8.0
echo "y" | sudo yum remove java-1.7.0-openjdk
# allowing pssh variables to be sent
sudo chmod 666 /etc/ssh/sshd_config
printf "\nAcceptEnv PSSH_NODENUM PSSH_HOST" >> /etc/ssh/sshd_config
sudo service sshd restart