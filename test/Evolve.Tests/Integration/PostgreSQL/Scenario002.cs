using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Integration.PostgreSql
{
    [Collection("PostgreSql collection")]
    public class Scenario002 : ScenarioBase
    {
        public Scenario002(PostgreSqlFixture dbContainer, ITestOutputHelper output)
            : base(dbContainer, output)
        {
        }

        [Fact]
        [Category(Test.PostgreSQL, Test.Sceanario)]
        public void Scenario_retry_repeatable_migrations_until_no_progression()
        {
            // Arrange
            Evolve.RetryRepeatableMigrationsUntilNoError = true;

            // Assert
            Evolve.AssertInfoIsSuccessful(Cnn)
                  .AssertMigrateThrows<EvolveException>(Cnn);
        }
    }
}
