using EvolveDb.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace EvolveDb.Tests.Integration.PostgreSql
{
    [Collection("PostgreSql collection")]
    public class Scenario009 : Scenario<PostgreSqlFixture>
    {
        public Scenario009(PostgreSqlFixture dbContainer, ITestOutputHelper output) : base(dbContainer, output) {}

        [Fact]
        [Category(Test.PostgreSQL, Test.Sceanario)]
        public void Scenario_erase_extension_should_work()
        {
            Evolve.Migrate();
            Evolve.Erase();
        }
    }
}
