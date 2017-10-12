using System;
using System.Threading;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.IntegrationTest.MySQL
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
            MySql = new MySqlDockerContainer();
#if DEBUG
            MySql.Start();

            Thread.Sleep(10000);
#endif
        }

        public MySqlDockerContainer MySql { get; }

        public void Dispose()
        {
#if DEBUG
            MySql.Dispose();
#endif
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
