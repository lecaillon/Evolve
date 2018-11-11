CREATE PROCEDURE insert_data(a integer, b integer)
LANGUAGE SQL
AS $$
INSERT INTO distributors3 VALUES (a);
INSERT INTO distributors3 VALUES (b);
$$;

CALL insert_data(1, 2);
