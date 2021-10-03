using System;

namespace EvolveDb.Tests.Infrastructure
{
    public class CassandraFixture : DbContainerFixture<CassandraContainer>, IDisposable
    {
    }
}
