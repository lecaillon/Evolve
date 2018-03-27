//Limitation: if you use the same keyspace for log table and your tables, the keyspace will have been
// created by Evolve, so make sure to add "if not exists" to avoid errors.
create keyspace if not exists my_keyspace
with replication = {
	'class' : 'SimpleStrategy',
	'replication_factor' : '1'
};

//Limitation: if the keyspace does not exist prior to running Evolve, it will be created with the
// SimpleStrategy as replication. If you want to use another replication strategy, you can do so
// by altering the keyspace here
// https://docs.datastax.com/en/cassandra/3.0/cassandra/operations/opsChangeKSStrategy.html
//alter keyspace cycling
//with replication = {
//	'class' : 'NetworkTopologyStrategy',
//	'dc1' : 3
//};--added due to parsin limitation
//WARNING : this will not work in tests as we only have one instance of Cassandra

//Limitation; "use xxx;" seems to not be handeled by the Cassandra driver, so you need to prefix all your
// references with the target keyspace
create table my_keyspace.my_table (
	id uuid,
	timestamp_unixepoch_ticks bigint,
	content text,
	primary key ((id), timestamp_unixepoch_ticks));

insert into my_keyspace.my_table (id, timestamp_unixepoch_ticks, content)
values (uuid(), unixTimestampOf(now()), 'some content');

insert into my_keyspace.my_table (id, timestamp_unixepoch_ticks, content)
values (uuid(), unixTimestampOf(now()), 'more content');
