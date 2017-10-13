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
            MySql = new MySqlDockerContainer();
            MsSql = new MsSqlDockerContainer();
            Pg = new PostgreSqlDockerContainer();

            if(!TestContext.Travis && !TestContext.AppVeyor) // AppVeyor does not support Docker Linux images, and Travis CI runs on Linux
            {
                MySql.Start();
                MsSql.Start();
                Pg.Start();

                Thread.Sleep(10000);
            }
        }

        public MySqlDockerContainer MySql { get; }
        public MsSqlDockerContainer MsSql { get; }
        public PostgreSqlDockerContainer Pg { get; }

        public void Dispose()
        {
            if (!TestContext.Travis && !TestContext.AppVeyor)
            {
                MySql.Dispose();
                MsSql.Dispose();
                Pg.Dispose();
            }
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
