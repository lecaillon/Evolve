using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Integration.PostgreSql
{
    [Collection("PostgreSql collection")]
    public class Scenario001 : ScenarioBase
    {
        public Scenario001(PostgreSqlFixture dbContainer, ITestOutputHelper output)
            : base(dbContainer, output)
        {
        }

        [Fact]
        [Category(Test.PostgreSQL, Test.Sceanario)]
        public void Scenario_retry_repeatable_migrations_until_no_error()
        {
            // Assert
            Evolve.AssertInfoIsSuccessfulV2(Cnn)
                  .AssertMigrateThrows<EvolveException>(Cnn);

            Evolve.AssertMigrateIsSuccessfulV2(Cnn, e => e.RetryRepeatableMigrationsUntilNoError = true);
        }
    }
}
