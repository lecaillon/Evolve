using System;
using System.Diagnostics.CodeAnalysis;
using Docker.DotNet;
using Docker.DotNet.Models;
using Evolve.Utilities;

namespace Evolve.Tests.Infrastructure
{
    internal class DockerContainer : IDisposable
    {
        private readonly DockerClient _client;
        private bool _disposedValue = false;

        [SuppressMessage("Qualité du code", "IDE0067: Supprimer les objets avant la mise hors de portée")]
        public DockerContainer(DockerClient client, string id, bool isRunning)
        {
            _client = Check.NotNull(client, nameof(client));
            Id = id;
            IsRunning = isRunning;
        }

        public string Id { get; }
        public bool IsRunning { get; private set; }

        public bool Start()
        {
            if (!IsRunning)
            {
                IsRunning = _client.Containers.StartContainerAsync(Id, null).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            return IsRunning;
        }

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
