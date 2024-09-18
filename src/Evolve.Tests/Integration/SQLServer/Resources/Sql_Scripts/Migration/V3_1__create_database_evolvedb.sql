-- evolve-tx-off
IF(db_id(N'evolvedb') IS NOT NULL)
BEGIN
	DROP DATABASE evolvedb;
END;

CREATE DATABASE evolvedb;
