CREATE SEQUENCE CountBy1  
    START WITH 1  
    INCREMENT BY 1 ;  
GO

SELECT * FROM sys.sequences WHERE name = 'CountBy1' ;