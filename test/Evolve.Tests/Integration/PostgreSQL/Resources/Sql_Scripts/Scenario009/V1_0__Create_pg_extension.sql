create extension if not exists citext;
create domain non_empty_citext as citext check (value ~ '\S');
create table scenario009.test (
     did    integer PRIMARY KEY,
     name   non_empty_citext
);
insert into scenario009.test values (1, 'PSG');
