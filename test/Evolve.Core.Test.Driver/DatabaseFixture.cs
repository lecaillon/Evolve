using System;
using System.Threading;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.Core.Test
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
            MsSql = new MsSqlDockerContainer();
            MsSql.Start();

            MySql = new MySqlDockerContainer();
            MySql.Start();

            Pg = new PostgreSqlDockerContainer();
            Pg.Start();

            Thread.Sleep(60000);
        }

        public MySqlDockerContainer MySql { get; }
        public MsSqlDockerContainer MsSql { get; }
        public PostgreSqlDockerContainer Pg { get; }

        public void Dispose()
        {
            MySql.Dispose();
            MsSql.Dispose();
            Pg.Dispose();
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
