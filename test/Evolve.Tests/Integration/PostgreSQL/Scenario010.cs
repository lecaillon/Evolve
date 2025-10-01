using EvolveDb.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace EvolveDb.Tests.Integration.PostgreSql
{
    [Collection("PostgreSql collection")]
    public class Scenario010 : Scenario<PostgreSqlFixture>
    {
        public Scenario010(PostgreSqlFixture dbContainer, ITestOutputHelper output) : base(dbContainer, output) {}

        [Fact]
        [Category(Test.PostgreSQL, Test.Sceanario)]
        public void Scenario_lockIdSpecific()
        {
            Evolve.ApplicationLockId = 27893423;
            Evolve.Migrate();
        }
    }
}
