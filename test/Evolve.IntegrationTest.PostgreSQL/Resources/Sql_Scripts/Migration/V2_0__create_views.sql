CREATE VIEW vista AS SELECT text 'Hello World' AS hello;

CREATE VIEW comedies AS
    SELECT *
    FROM films
    WHERE kind = 'Comedy';

-- Create a recursive view
CREATE RECURSIVE VIEW public.nums_1_100 (n) AS
    VALUES (1)
UNION ALL
    SELECT n+1 FROM nums_1_100 WHERE n < 100;

