using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace EvolveDb.Tests.Infrastructure
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
            RemovePreviousContainer = setupOptions.RemovePreviousContainer;
            Cmd = setupOptions.Cmd;

            try
            {
                _client = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient()
                    : new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                { // hack wsl
                    _client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
                }
            }

        }

        public string FromImage { get; }
        public string Tag { get; }
        public string Name { get; }
        public IList<string> Env { get; }
        public string ExposedPort { get; }
        public string HostPort { get; }
        public bool RemovePreviousContainer { get; }
        public IList<string> Cmd { get; }

        public async Task<DockerContainer> Build()
        {
            var container = (await _client.Containers.ListContainersAsync(new ContainersListParameters { All = true }))
                .FirstOrDefault(x => x.Names.Any(n => n.Equals("/" + Name, StringComparison.OrdinalIgnoreCase)));
            
            bool isRunning = container?.State == "running";
            if (container != null && !RemovePreviousContainer)
            {
                return new DockerContainer(_client, container.ID, isRunning);
            }

            if (container != null && RemovePreviousContainer)
            {
                var oldContainer = new DockerContainer(_client, container.ID, isRunning);
                await oldContainer.Stop();
                await oldContainer.Remove();
            }

            await _client.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = FromImage, Tag = Tag }, null, new Progress<JSONMessage>());

            var newContainer = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = $"{FromImage}:{Tag ?? "latest"}",
                Name = Name,
                Env = Env,
                ExposedPorts = new Dictionary<string, EmptyStruct> { { ExposedPort, new EmptyStruct() } },
                Cmd = Cmd,
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { ExposedPort, new List<PortBinding> { new PortBinding { HostIP = "localhost", HostPort = HostPort } } }
                    }
                }
            });

            return new DockerContainer(_client, newContainer.ID, isRunning: false);
        }
    }
}
