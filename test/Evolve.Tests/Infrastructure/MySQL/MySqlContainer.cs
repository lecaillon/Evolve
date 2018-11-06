using System.Data;
using MySql.Data.MySqlClient;

namespace Evolve.Tests.Infrastructure
{
    public class MySQLContainer : IDbContainer
    {
        public const string ExposedPort = "3306";
        public const string HostPort = "3306";
        public const string DbName = "my_database";
        public const string DbPwd = "Password12!";
        public const string DbUser = "root";

        private DockerContainer _container;

        public string Id => _container?.Id;
        public string CnxStr => $"Server=127.0.0.1;Port={HostPort};Database={DbName};Uid={DbUser};Pwd={DbPwd};SslMode=none;";
        public int TimeOutInSec => 10;

        public bool Start(bool fromScratch = false)
        {
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "mariadb",
                Tag = "latest",
                Name = "mariadb-evolve",
                Env = new[] { $"MYSQL_ROOT_PASSWORD={DbPwd}", $"MYSQL_DATABASE={DbName}" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort,
                RemovePreviousContainer = fromScratch
            }).Build();

            return _container.Start();
        }

        public IDbConnection CreateDbConnection() => new MySqlConnection(CnxStr);

        public void Dispose() => _container?.Dispose();
    }
}
