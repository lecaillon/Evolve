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
