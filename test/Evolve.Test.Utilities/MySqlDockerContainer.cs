namespace Evolve.Test.Utilities
{
    public class MySqlDockerContainer : IDockerContainer
    {
        private readonly DockerContainer _container;

        public MySqlDockerContainer()
        {
            _container = new DockerContainerBuilder(new DockerContainerBuilderOptions
            {
                FromImage = "mariadb",
                Tag = "latest",
                Name = "mariadb-evolve",
                Env = new[] { $"MYSQL_ROOT_PASSWORD={DbPwd}", $"MYSQL_DATABASE={DbName}" },
                ExposedPort = $"{ExposedPort}/tcp",
                HostPort = HostPort
            }).Build();
        }

        public string Id => _container.Id;
        public string ExposedPort => "3306";
        public string HostPort => "3306";
        public string DbName => "my_database";
        public string DbPwd => "Password12!"; // AppVeyor
        public string DbUser => "root";

        public bool Start() => _container.Start();
        public void Remove() => _container.Remove();
        public bool Stop() => _container.Stop();
        public void Dispose() => _container.Dispose();
    }
}
