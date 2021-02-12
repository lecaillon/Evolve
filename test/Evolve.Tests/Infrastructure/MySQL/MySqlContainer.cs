using System.Data;
using System.Diagnostics.CodeAnalysis;
using MySqlConnector;

namespace Evolve.Tests.Infrastructure
{
    public class MySQLContainer : IDbContainer
    {
        public const string ExposedPort = "3306";
        public const string HostPort = "3306";
        public const string DbName = "my_database";
        public static string DbPwd = TestContext.AzureDevOps ? "root" : "Password12!";
        public const string DbUser = "root";

        private DockerContainer _container;
        private bool _disposedValue = false;

        public string Id => _container?.Id;
        public string CnxStr => $"Server=127.0.0.1;Port={HostPort};Database={DbName};Uid={DbUser};Pwd={DbPwd};SslMode=none;Allow User Variables=True";
        public int TimeOutInSec => 25;

        [SuppressMessage("Qualité du code", "IDE0068: Utilisez le modèle de suppression recommandé")]
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _container?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
