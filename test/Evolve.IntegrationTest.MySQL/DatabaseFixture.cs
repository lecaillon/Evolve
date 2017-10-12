using System;
using System.Threading;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.IntegrationTest.MySQL
{
    public class DatabaseFixture : IDisposable
    {
        private readonly DockerContainer _container;

        public DatabaseFixture()
        {
#if DEBUG
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "mariadb",
                Tag = "latest",
                Name = "mariadb-evolve",
                Env = new[] { $"MYSQL_ROOT_PASSWORD={DbPwd}", $"MYSQL_DATABASE={DbName}" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort
            }).Build();

            _container.Start();

            Thread.Sleep(10000);
#endif
        }
        public string ExposedPort => "3306";
        public string HostPort => "3306";
        public string DbName => "my_database";
        public string DbPwd => "Password12!"; // AppVeyor
        public string DbUser => "root";

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
