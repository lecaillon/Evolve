﻿using System.Collections.Generic;
using System.Linq;
using EvolveDb.Dialect.SQLServer;
using Xunit;

namespace EvolveDb.Tests.Dialect.SQLServer
{
    public class SQLServerStatementBuilderTest
    {
        [Fact]
        [Category(Test.SQLServer)]
        public void SQLServerStatementBuilder_LoadSqlStatements_SplitsScriptCorrectly()
        {
            string sql = @"PRINT 'GO'

/* Create a user-defined table type. GO */  
CREATE TYPE LocationTableType AS TABLE   -- GO
    ( LocationName VARCHAR(50)  
    , CostRate INT );
/*
GO
*/

go 

GO

-- =============================================
-- SSN
-- =============================================

CREATE TYPE SSN  
FROM varchar(11) NOT NULL ;

GO";

            string sql1 = @"PRINT 'GO'

/* Create a user-defined table type. GO */  
CREATE TYPE LocationTableType AS TABLE   -- GO
    ( LocationName VARCHAR(50)  
    , CostRate INT );
/*
GO
*/";

            string sql2 = @"-- =============================================
-- SSN
-- =============================================

CREATE TYPE SSN  
FROM varchar(11) NOT NULL ;";

            var migrationScript = new FakeMigrationScript(sql);

            var sut = new SQLServerStatementBuilder();
            var statements = sut.LoadSqlStatements(migrationScript, new Dictionary<string, string>());

            Assert.Equal(2, statements.Count());
            Assert.Equal(sql1, statements.ElementAt(0).Sql);
            Assert.Equal(sql2, statements.ElementAt(1).Sql);
            Assert.True(statements.ElementAt(0).MustExecuteInTransaction);
            Assert.True(statements.ElementAt(1).MustExecuteInTransaction);
        }
    }
}
