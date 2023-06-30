using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EvolveDb.Tests.Infrastructure
{
    public class SQLServerContainer : IDbContainer
    {
        public const string ExposedPort = "1433";
        public const string HostPort = "1433";
        public const string DbName = "master";
        public const string DbPwd = "Password12!";
        public const string DbUser = "sa";

        private DockerContainer _container;

        public string Id => _container?.Id;
        public string CnxStr => $"Server=127.0.0.1;Database={DbName};User Id={DbUser};Password={DbPwd};TrustServerCertificate=True";
        public int TimeOutInSec => 60;

        public async Task<bool> Start(bool fromScratch = false)
        {
            _container = await new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "mcr.microsoft.com/mssql/server",
                Tag = "latest",
                Name = "mssql-evolve",
                Env = new[] { $"ACCEPT_EULA=Y", $"SA_PASSWORD={DbPwd}" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort,
                RemovePreviousContainer = fromScratch
            }).Build();

            return await _container.Start();
        }

        public DbConnection CreateDbConnection() => new SqlConnection(CnxStr);
    }
}
