using System;
using Docker.DotNet;
using Docker.DotNet.Models;
using EvolveDb.Utilities;

namespace EvolveDb.Tests.Infrastructure
{
    internal class DockerContainer : IDisposable
    {
        private readonly DockerClient _client;
        private bool _disposedValue = false;

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
