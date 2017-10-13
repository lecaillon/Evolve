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

            if (!TestContext.Travis && !TestContext.AppVeyor) // AppVeyor and Windows 2016 does not support linux docker images
            {
                MySql.Start();
                Thread.Sleep(10000);
            }
        }

        public MySqlDockerContainer MySql { get; }

        public void Dispose()
        {
            if (!TestContext.Travis && !TestContext.AppVeyor)
            {
                MySql.Dispose();
            }
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
