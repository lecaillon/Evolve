using System;

namespace Evolve.Test.Utilities
{
    public class CassandraFixture : IDisposable
    {
        public CassandraFixture()
        {
            Cassandra = new CassandraDockerContainer();
        }

        public void Start(bool fromScratch = false)
        {
            Cassandra.Start(fromScratch);
        }

        public CassandraDockerContainer Cassandra { get; }

        public void Dispose() => Cassandra.Dispose();
    }
}
