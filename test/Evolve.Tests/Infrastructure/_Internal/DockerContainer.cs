using Docker.DotNet;
using Docker.DotNet.Models;
using EvolveDb.Utilities;
using System.Threading.Tasks;

namespace EvolveDb.Tests.Infrastructure
{
    internal class DockerContainer
    {
        private readonly DockerClient _client;

        public DockerContainer(DockerClient client, string id, bool isRunning)
        {
            _client = Check.NotNull(client, nameof(client));
            Id = id;
            IsRunning = isRunning;
        }

        public string Id { get; }
        public bool IsRunning { get; private set; }

        /// <summary>
        ///     Returns false, if it was already running. Otherwise true.
        /// </summary>
        public async Task<bool> Start()
        {
            if (IsRunning)
            {
                return false;
            }

            IsRunning = await _client.Containers.StartContainerAsync(Id, null);
            return IsRunning;
        }

        public async Task<bool> Stop() => await _client.Containers.StopContainerAsync(Id, new ContainerStopParameters());

        public async Task Remove() => await _client.Containers.RemoveContainerAsync(Id, new ContainerRemoveParameters());
    }
}
