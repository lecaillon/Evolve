using Evolve.Dialect.Cassandra;
using Evolve.Migration;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Evolve.IntegrationTest.Cassandra
{
    public sealed class CqlStatementBuilderTest
    {
        [Fact]
        public void LoadSqlStatements_SplitsScriptCorrectly()
        {
            var cql = @"create keyspace my_keyspace
with replication = {
    'class' : 'SimpleStrategy',
    'replication_factor' : '1'
};

use my_keyspace;

create table my_table (
    id text,
    timestamp_unixepoch_ticks bigint,
    content text,
    primary key ((id), timestamp_unixepoch_ticks));";
            var migrationScript = new FakeMigrationScript(cql);

            var sut = new CqlStatementBuilder();
            var statements = sut.LoadSqlStatements(migrationScript, new Dictionary<string, string>());

            Assert.Equal(3, statements.Count());
            Assert.Equal(0, statements.ElementAt(0).LineNumber);
            Assert.Equal(5, statements.ElementAt(1).LineNumber);
            Assert.Equal(7, statements.ElementAt(2).LineNumber);
        }

        class FakeMigrationScript : MigrationScript
        {
            public FakeMigrationScript(string content)
                : base("1", "no description", "no name", content)
            { }
        }
    }
}
