using System.Collections.Generic;
using System.Linq;
using Evolve.Dialect;
using Xunit;

namespace Evolve.Tests.Dialect
{
    public class SimpleSqlStatementBuilderTest
    {
        [Fact]
        public void SimpleSqlStatementBuilder_LoadSqlStatements_SplitsScriptCorrectly()
        {
            var sql = @"-- Define a primary key constraint for table distributors. The following two examples are equivalent
CREATE TABLE distributors1 (
    did     integer,
    name    varchar(40),
    PRIMARY KEY(did)
);
CREATE TABLE distributors2 (
    did     integer PRIMARY KEY,
    name    varchar(40)
);
INSERT INTO distributors2 VALUES(1, 'azerty');";
            var migrationScript = new FakeMigrationScript(sql);

            var sut = new SimpleSqlStatementBuilder();
            var statements = sut.LoadSqlStatements(migrationScript, new Dictionary<string, string>());

            Assert.Single(statements);
            Assert.Equal(sql, statements.ElementAt(0).Sql);
            Assert.True(statements.ElementAt(0).MustExecuteInTransaction);
        }
    }
}
