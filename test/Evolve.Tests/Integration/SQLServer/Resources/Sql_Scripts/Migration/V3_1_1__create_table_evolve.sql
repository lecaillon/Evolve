CREATE TABLE evolvedb.dbo.evolve
(
	ID smallint	NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED,
	TYPE_TEST nvarchar(3) NOT NULL,
);

INSERT INTO evolvedb.dbo.evolve (TYPE_TEST) VALUES('1');
