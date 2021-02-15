using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.CockroachDb
{
    [Collection("CockroachDB collection")]
    public class MigrationTests
    {
        private readonly CockroachDBFixture _dbContainer;
        private readonly ITestOutputHelper _output;

        public MigrationTests(CockroachDBFixture dbContainer, ITestOutputHelper output)
        {
            _dbContainer = dbContainer;
            _output = output;

            if (Local)
            {
                dbContainer.Run(fromScratch: true);
            }
        }

        [FactSkippedOnAppVeyor]
        [Category(Test.CockroachDB)]
        public void Run_all_CockroachDB_migrations_work()
        {
            // Arrange
            var cnn = _dbContainer.CreateDbConnection();
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Schemas = new[] { "evolve", "defaultdb" }, // MetadataTableSchema = evolve | migrations = defaultdb
            };

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
