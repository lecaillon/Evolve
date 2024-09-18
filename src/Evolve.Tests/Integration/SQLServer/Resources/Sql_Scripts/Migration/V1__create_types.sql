PRINT 'CREATE TYPE'

/* Create a user-defined table type */  
CREATE TYPE LocationTableType AS TABLE   
    ( LocationName VARCHAR(50)  
    , CostRate INT );  
GO 

GO

-- =============================================
-- SSN
-- =============================================

CREATE TYPE SSN  
FROM varchar(11) NOT NULL ;

GO