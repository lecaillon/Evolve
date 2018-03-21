create keyspace if not exists my_keyspace
with replication = {
	'class' : 'SimpleStrategy',
	'replication_factor' : '1'
};