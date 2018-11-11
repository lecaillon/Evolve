-- Creates a composite type and uses it in a function
CREATE TYPE compfoo AS (f1 int, f2 text);

CREATE FUNCTION getfoo() RETURNS SETOF compfoo AS $$
    SELECT did, name FROM distributors
$$ LANGUAGE SQL;

-- Creates an enumerated type and uses it in a table definition
CREATE TYPE bug_status AS ENUM ('new', 'open', 'closed');

CREATE TABLE bug (
    id serial,
    description text,
    status bug_status
);
