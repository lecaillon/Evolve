namespace Evolve.Test.Utilities
{
    public class MsSqlDockerContainer : IDockerContainer
    {
        private readonly DockerContainer _container;

        public MsSqlDockerContainer()
        {
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "microsoft/mssql-server-linux",
                Tag = "latest",
                Name = "mssql-evolve",
                Env = new[] { $"ACCEPT_EULA=Y", $"SA_PASSWORD={DbPwd}" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort
            }).Build();
        }

        public string Id => _container.Id;
        public string ExposedPort => "1433";
        public string HostPort => "1433";
        public string DbName => "my_database";
        public string DbPwd => "Password12!"; // AppVeyor
        public string DbUser => "sa";

        public bool Start() => _container.Start();
        public void Remove() => _container.Remove();
        public bool Stop() => _container.Stop();
        public void Dispose() => _container.Dispose();
    }
}
