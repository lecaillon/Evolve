CREATE TABLE ${table4} (
  id INT NOT NULL,
  name VARCHAR(10) NOT NULL,
  PRIMARY KEY(name)
);

CREATE TRIGGER update_${table4} UPDATE OF name ON ${table4}
  BEGIN
    UPDATE ${table4} SET id = 100 WHERE name = old.name;
  END;