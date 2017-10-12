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
#if DEBUG
            MsSql = new MsSqlDockerContainer();
            MsSql.Start();

            Thread.Sleep(10000);
#endif
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
#if DEBUG
            MsSql.Dispose();
#endif
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
