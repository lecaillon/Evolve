using EvolveDb.Tests.Infrastructure;
using Xunit.Abstractions;
using static EvolveDb.Tests.TestContext;

namespace EvolveDb.Tests.Integration.CockroachDb
{
    public record MigrationTests(ITestOutputHelper Output) : DbContainerFixture<CockroachDBContainer>
    {
        [FactSkippedOnAppVeyor]
        [Category(Test.CockroachDB)]
        public void Run_all_CockroachDB_migrations_work()
        {
            // Arrange
            var cnn = CreateDbConnection();
            var evolve = new Evolve(cnn, msg => Output.WriteLine(msg))
            {
                Schemas = new[] { "evolve", "defaultdb" }, // MetadataTableSchema = evolve | migrations = defaultdb
            };
            evolve.Erase();

            // Assert
            evolve.AssertInfoIsSuccessful(cnn)
                  .ChangeLocations(CockroachDB.MigrationFolder)
                  .AssertInfoIsSuccessful(cnn)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(CockroachDB.ChecksumMismatchFolder)
                  .AssertMigrateThrows<EvolveValidationException>(cnn)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .ChangeLocations(CockroachDB.MigrationFolder)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations()
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(CockroachDB.MigrationFolder)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);
        }
    }
}
