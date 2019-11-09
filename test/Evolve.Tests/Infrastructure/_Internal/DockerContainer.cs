using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Evolve.Tests.Infrastructure
{
    internal class DockerContainer : IDisposable
    {
        private readonly DockerClient _client;
        private bool _disposedValue = false;

        [SuppressMessage("Qualité du code", "IDE0067: Supprimer les objets avant la mise hors de portée")]
        public DockerContainer(string id)
        {
            Id = id;

            _client = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient()
                : new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        }

        public string Id { get; }

        public bool Start() => _client.Containers.StartContainerAsync(Id, null).ConfigureAwait(false).GetAwaiter().GetResult();

        public bool Stop() => _client.Containers.StopContainerAsync(Id, new ContainerStopParameters()).ConfigureAwait(false).GetAwaiter().GetResult();

        public void Remove() => _client.Containers.RemoveContainerAsync(Id, new ContainerRemoveParameters()).ConfigureAwait(false).GetAwaiter().GetResult();

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
