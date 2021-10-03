using System;

namespace EvolveDb.Tests.Infrastructure
{
    public class MySQLFixture : DbContainerFixture<MySQLContainer>, IDisposable
    {
    }
}
