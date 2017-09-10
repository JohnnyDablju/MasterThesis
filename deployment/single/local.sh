# start zookeeper
./zookeeper-server-start C:\Git\MasterThesis\deployment\single\zookeeper.properties
# start server
./kafka-server-start C:\Git\MasterThesis\deployment\single\server.properties
# create topics
./kafka-topics --create --zookeeper localhost:2181 --replication-factor 1 --partitions 4 --topic WordCountInput