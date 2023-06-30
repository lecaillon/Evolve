using EvolveDb.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace EvolveDb.Tests.Integration.PostgreSql
{
    public class Scenario002 : Scenario<PostgreSqlContainer>
    {
        public Scenario002(ITestOutputHelper output) : base(output)
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
