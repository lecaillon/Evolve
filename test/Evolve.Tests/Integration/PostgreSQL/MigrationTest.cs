using System.Collections.Generic;
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
            var cnn = _dbContainer.CreateDbConnection();
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Schemas = new[] { "public", "unittest" },
                MetadataTableSchema = "unittest",
                Placeholders = new Dictionary<string, string> { ["${schema1}"] = "unittest" }
            };

            // Assert
            evolve.AssertInfoIsSuccessfulV2(cnn)
                  .ChangeLocations(locations)
                  .AssertInfoIsSuccessfulV2(cnn)
                  .AssertMigrateIsSuccessfulV2(cnn)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(PostgreSQL.ChecksumMismatchFolder)
                  .AssertMigrateThrows<EvolveValidationException>(cnn)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .ChangeLocations(locations)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(PostgreSQL.OutOfOrderFolder)
                  .AssertMigrateIsSuccessfulV2(cnn, e => e.OutOfOrder = true);

            evolve.ChangeLocations()
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(locations)
                  .AssertMigrateIsSuccessfulV2(cnn)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(PostgreSQL.ChecksumMismatchFolder)
                  .AssertMigrateIsSuccessfulV2(cnn, e => e.MustEraseOnValidationError = true)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(locations)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessfulV2(cnn)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(PostgreSQL.RepeatableFolder)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 0)
                  .AssertMigrateIsSuccessfulV2(cnn);
        }
    }
}
