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
#if DEBUG
            Pg.Start();

            Thread.Sleep(5000);
#endif
        }

        public PostgreSqlDockerContainer Pg { get; }

        public void Dispose()
        {
#if DEBUG
            Pg.Dispose();
#endif
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
