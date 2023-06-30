using System.Data.Common;
using System.Threading.Tasks;
using MySqlConnector;

namespace EvolveDb.Tests.Infrastructure
{
    public class MySQLContainer : IDbContainer
    {
        public const string ExposedPort = "3306";
        public const string HostPort = "3306";
        public const string DbName = "my_database";
        public static string DbPwd = TestContext.AzureDevOps ? "root" : "Password12!";
        public const string DbUser = "root";

        private DockerContainer _container;

        public string Id => _container?.Id;
        public string CnxStr => $"Server=127.0.0.1;Port={HostPort};Database={DbName};Uid={DbUser};Pwd={DbPwd};SslMode=none;Allow User Variables=True";
        public int TimeOutInSec => 25;

        public async Task<bool> Start(bool fromScratch = false)
        {
            _container = await new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "mariadb",
                Tag = "latest",
                Name = "mariadb-evolve",
                Env = new[] { $"MYSQL_ROOT_PASSWORD={DbPwd}", $"MYSQL_DATABASE={DbName}" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort,
                RemovePreviousContainer = fromScratch
            }).Build();

            return await _container.Start();
        }

        public DbConnection CreateDbConnection() => new MySqlConnection(CnxStr);
    }
}
