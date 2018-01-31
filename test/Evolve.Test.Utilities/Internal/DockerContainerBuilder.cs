using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
            DelayAfterStartup = setupOptions.DelayAfterStartup;
            RemovePreviousContainer = setupOptions.RemovePreviousContainer;

            _client = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient()
                : new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        }

        public string FromImage { get; }
        public string Tag { get; }
        public string Name { get; }
        public IList<string> Env { get; }
        public string ExposedPort { get; }
        public string HostPort { get; }
        public TimeSpan? DelayAfterStartup { get; }
        public bool RemovePreviousContainer { get; }

        public DockerContainer Build()
        {
            var container = _client.Containers.ListContainersAsync(new ContainersListParameters { All = true }).ConfigureAwait(false).GetAwaiter().GetResult()
                                              .FirstOrDefault(x => x.Names.Any(n => n.Equals("/" + Name, StringComparison.OrdinalIgnoreCase)));
            if (container != null && !RemovePreviousContainer)
            {
                return new DockerContainer(container.ID, DelayAfterStartup);
            }

            if (RemovePreviousContainer)
            {
                var oldContainer = new DockerContainer(container.ID, DelayAfterStartup);
                oldContainer.Stop();
                oldContainer.Remove();
            }

            _client.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = FromImage, Tag = Tag }, null, new Progress<JSONMessage>()).ConfigureAwait(false).GetAwaiter().GetResult();

            var newContainer = _client.Containers.CreateContainerAsync(new CreateContainerParameters
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
            }).ConfigureAwait(false).GetAwaiter().GetResult();

            return new DockerContainer(newContainer.ID, DelayAfterStartup);
        }
    }
}
