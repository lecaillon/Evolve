using System;
using System.Threading;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.IntegrationTest.PostgreSQL
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
            Pg = new PostgreSqlDockerContainer();
            if (!TestContext.Travis && !TestContext.AppVeyor) // AppVeyor and Windows 2016 does not support linux docker images
            {
                Pg.Start();
                Thread.Sleep(5000);
            }
        }

        public PostgreSqlDockerContainer Pg { get; }

        public void Dispose()
        {
            if (!TestContext.Travis && !TestContext.AppVeyor)
            {
                Pg.Dispose();
            }
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
