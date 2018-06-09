
CREATE TABLE TEST
(
	ID									smallint			NOT NULL		IDENTITY(1,1) PRIMARY KEY CLUSTERED,
	TYPE_TEST							nvarchar(3)		NOT NULL,
	DATE_TEST							date	NOT NULL,

	FOREIGN KEY (DATE_TEST) REFERENCES CALENDRIER (JOUR) ON DELETE CASCADE,
);

CREATE TABLE dbo.Employee   
(    
  [EmployeeID] int NOT NULL PRIMARY KEY CLUSTERED   
  , [Name] nvarchar(100) NOT NULL  
  , [Position] varchar(100) NOT NULL   
  , [Department] varchar(100) NOT NULL  
  , [Address] nvarchar(1024) NOT NULL  
  , [AnnualSalary] decimal (10,2) NOT NULL  
  , [ValidFrom] datetime2 (2) GENERATED ALWAYS AS ROW START  
  , [ValidTo] datetime2 (2) GENERATED ALWAYS AS ROW END  
  , PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)  
 )    
 WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.EmployeeHistory));

SELECT * FROM CALENDRIER;
