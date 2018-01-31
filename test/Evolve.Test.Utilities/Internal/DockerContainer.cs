using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Evolve.Test.Utilities
{
    internal class DockerContainer
    {
        private readonly DockerClient _client;
        private readonly TimeSpan? _delayAfterStartup;

        public DockerContainer(string id, TimeSpan? delayAfterStartup)
        {
            Id = id;
            _delayAfterStartup = delayAfterStartup;

            _client = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient()
                : new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        }

        public string Id { get; }

        public bool Start()
        {
            bool hasBeenStarted = _client.Containers.StartContainerAsync(Id, null).ConfigureAwait(false).GetAwaiter().GetResult();
            if (hasBeenStarted && _delayAfterStartup.HasValue)
            {
                Task.Delay(_delayAfterStartup.Value).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            return hasBeenStarted;
        }

        public bool Stop() => _client.Containers.StopContainerAsync(Id, new ContainerStopParameters()).ConfigureAwait(false).GetAwaiter().GetResult();

        public void Remove() => _client.Containers.RemoveContainerAsync(Id, new ContainerRemoveParameters()).ConfigureAwait(false).GetAwaiter().GetResult();

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
