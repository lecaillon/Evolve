using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Integration.PostgreSql
{
    [Collection("PostgreSql collection")]
    public class Scenario005 : Scenario<PostgreSqlFixture>
    {
        public Scenario005(PostgreSqlFixture dbContainer, ITestOutputHelper output) : base(dbContainer, output) {}

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
