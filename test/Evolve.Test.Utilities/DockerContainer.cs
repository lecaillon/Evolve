using System;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Evolve.Test.Utilities
{
    public class DockerContainer : IDisposable
    {
        private readonly DockerClient _client;
        private readonly bool _rm;

        public DockerContainer(string id, bool rm = true)
        {
            Id = id;
            _rm = rm;

            _client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
        }

        public string Id { get; }

        public bool Start() => _client.Containers.StartContainerAsync(Id, null).Result;
        public bool Stop() => _client.Containers.StopContainerAsync(Id, new ContainerStopParameters()).Result;
        public void Remove() => _client.Containers.RemoveContainerAsync(Id, new ContainerRemoveParameters()).Wait();

        public void Dispose()
        {
            Stop();

            if(_rm)
            {
                Remove();
            }

            _client.Dispose();
        }
    }
}
