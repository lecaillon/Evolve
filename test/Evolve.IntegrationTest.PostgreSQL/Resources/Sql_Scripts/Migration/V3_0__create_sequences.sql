CREATE SEQUENCE serial START 101;

-- Use this sequence in an INSERT command
INSERT INTO distributors VALUES (nextval('serial'), 'nothing');
