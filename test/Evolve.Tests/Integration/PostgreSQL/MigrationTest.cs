using EvolveDb.Dialect;
using EvolveDb.Tests.Infrastructure;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using static EvolveDb.Tests.TestContext;

namespace EvolveDb.Tests.Integration.PostgregSql
{
    public class MigrationTests : DbContainerFixture<PostgreSqlContainer>
    {
        private readonly ITestOutputHelper _output;

        public MigrationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public override bool MustRunContainer => Local;

        [Fact]
        [Category(Test.PostgreSQL)]
        public void Run_all_PostgreSQL_migrations_work()
        {
            // Arrange
            string[] locations = AppVeyor ? new[] { PostgreSQL.MigrationFolder } : new[] { PostgreSQL.MigrationFolder, PostgreSQL.Migration11Folder }; // Add specific PostgreSQL 11 scripts
            var cnn = CreateDbConnection();
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg), DBMS.PostgreSQL)
            {
                Schemas = new[] { "public", "unittest" },
                MetadataTableSchema = "unittest",
                Placeholders = new Dictionary<string, string> { ["${schema1}"] = "unittest" }
            };

            // Assert
            evolve.AssertInfoIsSuccessful(cnn)
                  .ChangeLocations(locations)
                  .AssertInfoIsSuccessful(cnn)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(PostgreSQL.ChecksumMismatchFolder)
                  .AssertMigrateThrows<EvolveValidationException>(cnn)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .ChangeLocations(locations)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(PostgreSQL.OutOfOrderFolder)
                  .AssertMigrateIsSuccessful(cnn, e => e.OutOfOrder = true);

            evolve.ChangeLocations()
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(locations)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(PostgreSQL.ChecksumMismatchFolder)
                  .AssertMigrateIsSuccessful(cnn, e => e.MustEraseOnValidationError = true)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(locations)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(PostgreSQL.RepeatableFolder)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 0)
                  .AssertMigrateIsSuccessful(cnn);

            evolve.AssertEraseIsSuccessful(cnn);
        }
    }
}
