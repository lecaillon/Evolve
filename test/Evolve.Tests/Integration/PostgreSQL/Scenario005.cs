using EvolveDb.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace EvolveDb.Tests.Integration.PostgreSql
{
    public class Scenario005 : Scenario<PostgreSqlContainer>
    {
        public Scenario005(ITestOutputHelper output) : base(output) {}

        [Fact]
        [Category(Test.PostgreSQL, Test.Sceanario)]
        public void Scenario_skip_migration_until_target_version_is_reached()
        {
            // Arrange
            Evolve.SkipNextMigrations = true;
            Evolve.TargetVersion = new("2_0");

            // Assert
            Evolve.AssertMigrateIsSuccessful(Cnn);
        }
    }
}
