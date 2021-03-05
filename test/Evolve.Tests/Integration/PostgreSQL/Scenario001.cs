using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Integration.PostgreSql
{
    [Collection("PostgreSql collection")]
    public class Scenario001 : Scenario<PostgreSqlFixture>
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
            Evolve.AssertInfoIsSuccessful(Cnn)
                  .AssertMigrateThrows<EvolveException>(Cnn);

            Evolve.AssertMigrateIsSuccessful(Cnn, e => e.RetryRepeatableMigrationsUntilNoError = true);
        }
    }
}
