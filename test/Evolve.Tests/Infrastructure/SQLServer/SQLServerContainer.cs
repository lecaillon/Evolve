﻿using System.Data;
using System.Data.SqlClient;

namespace Evolve.Tests.Infrastructure
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
        public string CnxStr => $"Server=127.0.0.1;Database={DbName};User Id={DbUser};Password={DbPwd};";
        public int TimeOutInSec => 60;

        public bool Start(bool fromScratch = false)
        {
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "microsoft/mssql-server-linux",
                Tag = "latest",
                Name = "mssql-evolve",
                Env = new[] { $"ACCEPT_EULA=Y", $"SA_PASSWORD={DbPwd}" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort,
                RemovePreviousContainer = fromScratch
            }).Build();

            return _container.Start();
        }

        public IDbConnection CreateDbConnection() => new SqlConnection(CnxStr);

        public void Dispose() => _container?.Dispose();
    }
}
