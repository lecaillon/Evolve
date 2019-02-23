create table table_2(
id integer primary KEY
);

create table table_3(
id integer primary KEY ,
table_2_id integer not null REFERENCES table_2(id)
);
