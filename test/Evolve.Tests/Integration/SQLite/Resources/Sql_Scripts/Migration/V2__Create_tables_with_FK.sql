create table table_2(
id integer primary KEY
);

create table table_3(
id integer primary KEY ,
table_2_id integer not null REFERENCES table_2(id)
);

insert into table_2 values(1);
insert into table_3 values(1,1);