using EvolveDb.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace EvolveDb.Tests.Integration.PostgreSql
{
    public record Scenario005(ITestOutputHelper Output) : Scenario<PostgreSqlContainer>(Output)
    {
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
