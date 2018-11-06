using System.Collections.Generic;
using System.Linq;
using Evolve.Dialect.SQLServer;
using Xunit;

namespace Evolve.Tests.Dialect.SQLServer
{
    public class SQLServerStatementBuilderTest
    {
        [Fact]
        public void SQLServerStatementBuilder_LoadSqlStatements_SplitsScriptCorrectly()
        {
            string sql = @"PRINT 'CREATE TYPE'

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

GO";

            string sql1 = @"PRINT 'CREATE TYPE'

/* Create a user-defined table type */  
CREATE TYPE LocationTableType AS TABLE   
    ( LocationName VARCHAR(50)  
    , CostRate INT );";

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
