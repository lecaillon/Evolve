create table my_keyspace.my_other_table (
	id uuid,
	timestamp_unixepoch_ticks bigint,
	content text,
	primary key (id));