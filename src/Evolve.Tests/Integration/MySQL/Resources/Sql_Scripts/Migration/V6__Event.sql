﻿CREATE TABLE mytable (
  mycol INT
);

CREATE EVENT myevent
  ON SCHEDULE AT CURRENT_TIMESTAMP + INTERVAL 1 HOUR
DO
  UPDATE mytable SET mycol = mycol + 1;