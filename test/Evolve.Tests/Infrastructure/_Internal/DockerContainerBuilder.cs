using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Evolve.Tests.Infrastructure
{
    internal class DockerContainerBuilder : IDisposable
    {
        private readonly DockerClient _client;
        private bool _disposedValue = false;

        [SuppressMessage("Qualité du code", "IDE0067: Supprimer les objets avant la mise hors de portée")]
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

        public DockerContainer Build()
        {
            var container = _client.Containers.ListContainersAsync(new ContainersListParameters { All = true }).ConfigureAwait(false).GetAwaiter().GetResult()
                                              .FirstOrDefault(x => x.Names.Any(n => n.Equals("/" + Name, StringComparison.OrdinalIgnoreCase)));
            
            bool isRunning = container?.State == "running";
            if (container != null && !RemovePreviousContainer)
            {
                return new DockerContainer(_client, container.ID, isRunning);
            }

            if (container != null && RemovePreviousContainer)
            {
                using var oldContainer = new DockerContainer(_client, container.ID, isRunning);
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
                Cmd = Cmd,
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { ExposedPort, new List<PortBinding> { new PortBinding { HostIP = "localhost", HostPort = HostPort } } }
                    }
                }
            }).ConfigureAwait(false).GetAwaiter().GetResult();

            return new DockerContainer(_client, newContainer.ID, isRunning: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _client.Dispose();
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
