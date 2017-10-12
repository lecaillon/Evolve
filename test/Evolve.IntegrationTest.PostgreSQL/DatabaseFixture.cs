using System;
using System.Threading;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.IntegrationTest.PostgreSQL
{
    public class DatabaseFixture : IDisposable
    {
        private readonly DockerContainer _container;

        public DatabaseFixture()
        {
#if DEBUG
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "postgres",
                Tag = "alpine",
                Name = "postgres-evolve",
                Env = new[] { $"POSTGRES_PASSWORD={DbPwd}", $"POSTGRES_DB={DbName}" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort
            }).Build();

            _container.Start();

            Thread.Sleep(5000);
#endif
        }
        public string ExposedPort => "5432";
        public string HostPort => "5432";
        public string DbName => "my_database";
        public string DbPwd => "Password12!"; // AppVeyor
        public string DbUser => "postgres";

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
