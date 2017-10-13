using System;
using System.Data.SqlClient;
using System.Threading;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.IntegrationTest.SQLServer
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
            MsSql = new MsSqlDockerContainer();

            if (!TestContext.Travis && !TestContext.AppVeyor) // AppVeyor and Windows 2016 does not support linux docker images
            {
                MsSql.Start();
                Thread.Sleep(10000);
            }
        }

        public MsSqlDockerContainer MsSql { get; set; }

        public void CreateTestDatabase(string name)
        {
            var cnn = new SqlConnection($"Server=127.0.0.1;Database=master;User Id={MsSql.DbUser};Password={MsSql.DbPwd};");
            cnn.Open();

            using (var cmd = cnn.CreateCommand())
            {
                cmd.CommandText = $"CREATE DATABASE {name};";
                cmd.ExecuteNonQuery();
            }

            cnn.Close();
        }

        public void Dispose()
        {
            if (!TestContext.Travis && !TestContext.AppVeyor)
            {
                MsSql.Dispose();
            }
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
