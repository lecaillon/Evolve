alter table ${keyspace}.my_table
add foo text;

insert into ${keyspace}.my_table (id, timestamp_unixepoch_ticks, content, foo)
values (uuid(), unixTimestampOf(now()), 'more content', 'bar!');