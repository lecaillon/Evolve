using System.IO;
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
            string[] locations = new[] { CockroachDB.MigrationFolder };
            int expectedNbMigration = Directory.GetFiles(CockroachDB.MigrationFolder).Length;
            var cnn = _dbContainer.CreateDbConnection();
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Schemas = new[] { "evolve", "defaultdb" }, // MetadataTableSchema = evolve | migrations = defaultdb
            };

            // Assert
            evolve.AssertMigrateIsSuccessful(cnn, expectedNbMigration, null, locations)
                  .AssertMigrateThrows<EvolveValidationException>(cnn, locations: CockroachDB.ChecksumMismatchFolder)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0)
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration, locations: CockroachDB.MigrationFolder)
                  .AssertInfoIsSuccessful(cnn, expectedNbRows: expectedNbMigration + 2);
        }
    }
}
