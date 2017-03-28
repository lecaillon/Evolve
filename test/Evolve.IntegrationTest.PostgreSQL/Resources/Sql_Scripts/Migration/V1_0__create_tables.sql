
-- Create table films and table distributors
CREATE TABLE films (
    code        char(5) CONSTRAINT firstkey PRIMARY KEY,
    title       varchar(40) NOT NULL,
    did         integer NOT NULL,
    date_prod   date,
    kind        varchar(10),
    len         interval hour to minute
);
CREATE TABLE distributors (
     did    integer PRIMARY KEY,
     name   varchar(40) NOT NULL CHECK (name <> '')
);
COMMENT ON COLUMN films.title IS 'this is a comment;
this is another comment';

-- Define a primary key constraint for table distributors. The following two examples are equivalent
CREATE TABLE distributors1 (
    did     integer,
    name    varchar(40),
    PRIMARY KEY(did)
);
CREATE TABLE distributors2 (
    did     integer PRIMARY KEY,
    name    varchar(40)
);
INSERT INTO distributors2 VALUES(1, 'azerty');

-- Create the same table, specifying 70% fill factor for both the table and its unique index
CREATE TABLE distributors3 (
    did     integer,
    name    varchar(40),
    UNIQUE(name) WITH (fillfactor=70)
)
WITH (fillfactor=70);

-- Define a check table constraint
CREATE TABLE ${schema1}.distributors4 (
    did     integer,
    name    varchar(40)
    CONSTRAINT con1 CHECK (did > 100 AND name <> '')
);

-- Create table circles with an exclusion constraint that prevents any two circles from overlapping
CREATE TABLE circles (
    c circle,
    EXCLUDE USING gist (c WITH &&)
);

-- Create a composite type and a typed table
CREATE TYPE employee_type AS (name text, salary numeric);
CREATE TABLE employees OF employee_type (
    PRIMARY KEY (name),
    salary WITH OPTIONS DEFAULT 1000
);
