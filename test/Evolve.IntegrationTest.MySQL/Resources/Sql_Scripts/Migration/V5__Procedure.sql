CREATE TABLE test_data (
  value VARCHAR(25) NOT NULL,
  PRIMARY KEY(value)
);

CREATE PROCEDURE AddData()
  BEGIN
    INSERT INTO test_data (value) VALUES ('Hello');
  END ;

  CALL AddData;

DROP PROCEDURE IF EXISTS sp_temp;
CREATE DEFINER=root@'localhost' PROCEDURE sp_temp()
DETERMINISTIC
  BEGIN
    DECLARE s VARCHAR(20);
    SELECT 1;
  END;

  CALL sp_temp;

DROP PROCEDURE IF EXISTS sp_temp;
