using System;
using System.Runtime.InteropServices;
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
            MySql = new MySqlDockerContainer();
            Pg = new PostgreSqlDockerContainer();

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) // AppVeyor and Windows 2016 does not support linux docker images
            {
                MsSql.Start();
                MySql.Start();
                Pg.Start();

                Thread.Sleep(60000);
            }
        }

        public MySqlDockerContainer MySql { get; }
        public MsSqlDockerContainer MsSql { get; }
        public PostgreSqlDockerContainer Pg { get; }

        public void Dispose()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) // AppVeyor and Windows 2016 does not support linux docker images
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
