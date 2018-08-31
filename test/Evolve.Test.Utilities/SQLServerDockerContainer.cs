using System;

namespace Evolve.Test.Utilities
{
    public class SQLServerDockerContainer
    {
        private DockerContainer _container;

        public string Id => _container.Id;
        public string ExposedPort => "1433";
        public string HostPort => "1433";
        public string DbPwd => "Password12!"; // AppVeyor
        public string DbUser => "sa";
        public string CnxStr => $"Server=127.0.0.1;Database=master;User Id={DbUser};Password={DbPwd};";
        public TimeSpan DelayAfterStartup => TimeSpan.FromMinutes(1);

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
                DelayAfterStartup = DelayAfterStartup,
                RemovePreviousContainer = fromScratch
            }).Build();

            return _container.Start();
        }
        public void Remove() => _container.Remove();
        public bool Stop() => _container.Stop();
        public void Dispose() => _container?.Dispose();
    }
}
