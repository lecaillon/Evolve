using System.Collections.Generic;
using System.IO;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.PostgregSql
{
    [Collection("PostgreSql collection")]
    public class MigrationTests
    {
        private readonly PostgreSqlFixture _dbContainer;
        private readonly ITestOutputHelper _output;

        public MigrationTests(PostgreSqlFixture dbContainer, ITestOutputHelper output)
        {
            _dbContainer = dbContainer;
            _output = output;

            if (Local)
            {
                dbContainer.Run(fromScratch: true);
            }
        }

        [Fact]
        [Category(Test.PostgreSQL)]
        public void Run_all_PostgreSQL_migrations_work()
        {
            // Arrange
            string[] locations = AppVeyor ? new[] { PostgreSQL.MigrationFolder } : new[] { PostgreSQL.MigrationFolder, PostgreSQL.Migration11Folder }; // Add specific PostgreSQL 11 scripts
            int expectedNbMigration = AppVeyor ? Directory.GetFiles(PostgreSQL.MigrationFolder).Length : Directory.GetFiles(PostgreSQL.MigrationFolder).Length + 1;
            var cnn = _dbContainer.CreateDbConnection();
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Schemas = new[] { "public", "unittest" },
                MetadataTableSchema = "unittest",
                Placeholders = new Dictionary<string, string> { ["${schema1}"] = "unittest" }
            };

            // Assert
            evolve.AssertInfoIsSuccessful(cnn, expectedNbRows: 0)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration, null, locations)
                  .AssertMigrateThrows<EvolveValidationException>(cnn, locations: PostgreSQL.ChecksumMismatchFolder)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0)
                  .AssertMigrateThrows<EvolveValidationException>(cnn, locations: PostgreSQL.OutOfOrderFolder)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 1, e => e.OutOfOrder = true)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0, e => e.OutOfOrder = true)
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration, null, locations)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 1, e => e.MustEraseOnValidationError = true, locations: PostgreSQL.ChecksumMismatchFolder)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration, null, locations)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 0, locations: PostgreSQL.RepeatableFolder)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 1)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0)
                  .AssertInfoIsSuccessful(cnn, expectedNbRows: expectedNbMigration + 3);
        }
    }
}
