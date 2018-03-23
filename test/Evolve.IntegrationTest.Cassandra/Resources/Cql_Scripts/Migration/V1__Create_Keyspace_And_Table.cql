//Limitation: if you use the same keyspace for log table and your tables, the keyspace might have been
// created by Evolve, so make sure to add "if not exists" to avoid errors.
create keyspace if not exists my_keyspace
with replication = {
	'class' : 'SimpleStrategy',
	'replication_factor' : '1'
};

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
