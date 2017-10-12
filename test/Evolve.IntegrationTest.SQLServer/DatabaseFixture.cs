using System;
using System.Data.SqlClient;
using System.Threading;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.IntegrationTest.SQLServer
{
    public class DatabaseFixture : IDisposable
    {
        private readonly DockerContainer _container;

        public DatabaseFixture()
        {
#if DEBUG
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "microsoft/mssql-server-linux",
                Tag = "latest",
                Name = "mssql-evolve",
                Env = new[] { $"ACCEPT_EULA=Y", $"SA_PASSWORD={DbPwd}" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort
            }).Build();

            _container.Start();

            Thread.Sleep(10000);
#endif
        }

        public string ExposedPort => "1433";
        public string HostPort => "1433";
        public string DbName => "my_database";
        public string DbPwd => "Password12!"; // AppVeyor
        public string DbUser => "sa";

        public void CreateTestDatabase(string name)
        {
            var cnn = new SqlConnection($"Server=127.0.0.1;Database=master;User Id={DbUser};Password={DbPwd};");
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
            _container.Dispose();
#endif
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
