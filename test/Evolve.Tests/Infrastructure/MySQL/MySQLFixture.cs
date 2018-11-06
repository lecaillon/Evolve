using System;

namespace Evolve.Tests.Infrastructure
{
    public class MySQLFixture : DbContainerFixture<MySQLContainer>, IDisposable
    {
    }
}
