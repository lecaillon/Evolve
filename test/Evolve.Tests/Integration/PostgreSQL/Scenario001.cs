using EvolveDb.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace EvolveDb.Tests.Integration.PostgreSql
{
    public class Scenario001 : Scenario<PostgreSqlContainer>
    {
        public Scenario001(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [Category(Test.PostgreSQL, Test.Sceanario)]
        public void Scenario_retry_repeatable_migrations_until_no_error()
        {
            // Assert
            Evolve.AssertInfoIsSuccessful(Cnn)
                  .AssertMigrateThrows<EvolveException>(Cnn);

            Evolve.AssertMigrateIsSuccessful(Cnn, e => e.RetryRepeatableMigrationsUntilNoError = true);
        }
    }
}
