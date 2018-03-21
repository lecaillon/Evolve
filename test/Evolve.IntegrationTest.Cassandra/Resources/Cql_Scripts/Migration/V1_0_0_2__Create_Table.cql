create table if not exists my_keyspace.my_table (
	id text,
	timestamp_unixepoch_ticks bigint,
	content text,
	primary key ((id), timestamp_unixepoch_ticks));