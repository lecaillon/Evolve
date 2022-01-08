create extension if not exists citext WITH SCHEMA ${schema};
create domain ${schema}.non_empty_citext as ${schema}.citext check (value ~ '\S');
create table ${schema}.test (
     did    integer PRIMARY KEY,
     name   ${schema}.non_empty_citext
);
insert into ${schema}.test values (1, 'PSG');
