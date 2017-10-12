using System;
using System.Collections.Generic;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Evolve.Test.Utilities
{
    internal class DockerContainerBuilder
    {
        private readonly DockerClient _client;

        public DockerContainerBuilder(DockerContainerBuilderOptions setupOptions)
        {
            FromImage = setupOptions.FromImage;
            Tag = setupOptions.Tag;
            Name = setupOptions.Name;
            Env = setupOptions.Env;
            ExposedPort = setupOptions.ExposedPort;
            HostPort = setupOptions.HostPort;

            _client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
        }

        public string FromImage { get; }
        public string Tag { get; }
        public string Name { get; }
        public IList<string> Env { get; }
        public string ExposedPort { get; }
        public string HostPort { get; }

        public DockerContainer Build()
        {
            _client.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = FromImage, Tag = Tag }, null, new Progress<JSONMessage>()).Wait();

            var container = _client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = $"{FromImage}:{Tag ?? "latest"}",
                Name = Name,
                Env = Env,
                ExposedPorts = new Dictionary<string, EmptyStruct> { { ExposedPort, new EmptyStruct() } },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { ExposedPort, new List<PortBinding> { new PortBinding { HostIP = "localhost", HostPort = HostPort } } }
                    }
                }
            }).Result;

            return new DockerContainer(container.ID);
        }
    }
}
