using System;

namespace Evolve.Test.Utilities
{
    public interface IDockerContainer : IDisposable
    {
        string Id { get; }

        void Remove();
        bool Start();
        bool Stop();
    }
}