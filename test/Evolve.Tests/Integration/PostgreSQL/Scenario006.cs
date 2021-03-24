using System.Linq;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Integration.PostgreSql
{
    [Collection("PostgreSql collection")]
    public class Scenario006 : Scenario<PostgreSqlFixture>
    {
        public Scenario006(PostgreSqlFixture dbContainer, ITestOutputHelper output) : base(dbContainer, output) {}

        [Fact]
        [Category(Test.PostgreSQL, Test.Sceanario)]
        public void Scenario_repeatable_migration_executed_everytime()
        {
            for (int i = 1; i <= 3; i++)
            {
                Evolve.Migrate();
                Assert.True(Evolve.AppliedMigrations.Count == 1, $"This repeat always migration should be executed each time.");
                Assert.True(MetadataTable.GetAllAppliedRepeatableMigration().Count() == i);
                Assert.False(MetadataTable.GetAllAppliedMigration().Any());
            }
        }
    }
}
