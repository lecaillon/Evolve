using System.IO;
using EvolveDb.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace EvolveDb.Tests.Integration.PostgreSql
{
    public record Scenario008(ITestOutputHelper Output) : Scenario<PostgreSqlContainer>(Output)
    {
        [Fact]
        [Category(Test.PostgreSQL, Test.Sceanario)]
        public void Scenario_check_validation()
        {
            // Step00: Validation succeeds when no script ever found
            Evolve.ChangeLocations(new[] { Path.Combine(ScenarioFolder, "Step00") })
                  .AsserValidationIsSuccessful();

            // Step01: Validation failed when pending scripts are found
            //         And succeeds when they are all applied even with an `evolve-repeat-always` repeatable migration 
            Evolve.ChangeLocations(new[] { Path.Combine(ScenarioFolder, "Step01") })
                  .AssertValidationThrows<EvolveValidationException>(errorCount: 3);

            Evolve.AssertMigrateIsSuccessful(Cnn, repeatAlwaysCount: 1)
                  .AsserValidationIsSuccessful();

            // Step02: Validation failed because of a missing script
            Evolve.ChangeLocations(new[] { Path.Combine(ScenarioFolder, "Step02") })
                  .AssertValidationThrows<EvolveValidationException>(errorCount: 2);

            // Step03: Validation failed because of an invalid checksum
            Evolve.ChangeLocations(new[] { Path.Combine(ScenarioFolder, "Step03") })
                  .AssertValidationThrows<EvolveValidationException>(errorCount: 1);

            // Step04: Validation failed because of new cheksum of an existing repeatable migration
            Evolve.ChangeLocations(new[] { Path.Combine(ScenarioFolder, "Step04") })
                  .AssertValidationThrows<EvolveValidationException>(errorCount: 1);

            // Step01: Validation succeeds
            Evolve.ChangeLocations(new[] { Path.Combine(ScenarioFolder, "Step01") })
                  .AsserValidationIsSuccessful();
        }
    }
}
