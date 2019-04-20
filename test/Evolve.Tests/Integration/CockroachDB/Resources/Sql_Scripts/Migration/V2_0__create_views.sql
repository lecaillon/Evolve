CREATE VIEW v_names AS
    SELECT id, full_name
    FROM names
    WHERE id > 1;

-----------------------------------------------

CREATE VIEW v_orders AS
	SELECT c.name, o.id
	FROM orders o
	JOIN customers c 
		ON o.customer_id = c.id;

SELECT * FROM v_orders;