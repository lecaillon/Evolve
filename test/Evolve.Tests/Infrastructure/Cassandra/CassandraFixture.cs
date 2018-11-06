using System;

namespace Evolve.Tests.Infrastructure
{
    public class CassandraFixture : DbContainerFixture<CassandraContainer>, IDisposable
    {
    }
}
