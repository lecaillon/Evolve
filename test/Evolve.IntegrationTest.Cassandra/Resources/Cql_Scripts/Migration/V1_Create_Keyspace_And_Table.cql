create keyspace if not exists my_keyspace
with replication = {
	'class' : 'SimpleStrategy',
	'replication_factor' : 1
};

use my_keyspace;

create table if not exists my_table (
	id text,
	timestamp_unixepoch_ticks bigint,
	content text,
	primary key ((id), timestamp_unixepoch_ticks));