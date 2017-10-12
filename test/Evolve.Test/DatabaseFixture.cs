using System;
using System.Collections.Generic;
using System.Threading;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.Test
{
    public class DatabaseFixture : IDisposable
    {
        private readonly List<DockerContainer> _containers = new List<DockerContainer>();

        public DatabaseFixture()
        {
#if DEBUG
            _containers.Add(new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "mariadb",
                Tag = "latest",
                Name = "mariadb-evolve",
                Env = new[] { $"MYSQL_ROOT_PASSWORD={DbPwd}", $"MYSQL_DATABASE={DbName}" },
                ExposedPort = $"{MySqlExposedPort}/tcp",
                HostPort = MySqlHostPort
            }).Build());

            _containers[0].Start();

            _containers.Add(new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "microsoft/mssql-server-linux",
                Tag = "latest",
                Name = "mssql-evolve",
                Env = new[] { $"ACCEPT_EULA=Y", $"SA_PASSWORD={DbPwd}" },
                ExposedPort = $"{MsSqlExposedPort}/tcp",
                HostPort = MsSqlHostPort
            }).Build());

            _containers[1].Start();

            _containers.Add(new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "postgres",
                Tag = "alpine",
                Name = "postgres-evolve",
                Env = new[] { $"POSTGRES_PASSWORD={DbPwd}", $"POSTGRES_DB={DbName}" },
                ExposedPort = $"{PgExposedPort}/tcp",
                HostPort = PgHostPort
            }).Build());

            _containers[2].Start();

            Thread.Sleep(5000);
#endif
        }

        public string DbName => "my_database";
        public string DbPwd => "Password12!"; // AppVeyor

        public string PgExposedPort => "5432";
        public string PgHostPort => "5432";
        public string PgDbUser => "postgres";

        public string MySqlExposedPort => "3306";
        public string MySqlHostPort => "3306";
        public string MySqlDbUser => "root";

        public string MsSqlExposedPort => "1433";
        public string MsSqlHostPort => "1433";
        public string MsSqlDbUser => "sa";

        public void Dispose()
        {
#if DEBUG
            _containers.ForEach(x => x.Dispose());
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
