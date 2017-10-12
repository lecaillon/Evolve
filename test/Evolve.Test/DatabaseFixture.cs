using System;
using System.Threading;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.Test
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
#if DEBUG
            MySql = new MySqlDockerContainer();
            MySql.Start();

            MsSql = new MsSqlDockerContainer();
            MsSql.Start();

            Pg = new PostgreSqlDockerContainer();
            Pg.Start();

            Thread.Sleep(10000);
#endif
        }

        public MySqlDockerContainer MySql { get; }
        public MsSqlDockerContainer MsSql { get; }
        public PostgreSqlDockerContainer Pg { get; }

        public void Dispose()
        {
#if DEBUG
            MySql.Dispose();
            MsSql.Dispose();
            Pg.Dispose();
#endif
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
