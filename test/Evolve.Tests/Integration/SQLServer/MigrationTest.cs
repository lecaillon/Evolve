using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Evolve.Migration;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.SQLServer
{
    [Collection("SQLServer collection")]
    public class MigrationTest
    {
        public const string DbName = "my_database_2";
        private readonly SQLServerFixture _dbContainer;
        private readonly ITestOutputHelper _output;

        public MigrationTest(SQLServerFixture dbContainer, ITestOutputHelper output)
        {
            _dbContainer = dbContainer;
            _output = output;

            if (Local)
            {
                dbContainer.Run(fromScratch: true);
            }

            TestUtil.CreateSqlServerDatabase(DbName, _dbContainer.GetCnxStr("master"));
        }

        [Fact]
        [Category(Test.SQLServer)]
        public void Run_all_SQLServer_migrations_work()
        {
            // Arrange
            int expectedNbMigration = Directory.GetFiles(SqlServer.MigrationFolder).Length - 1; // Total -1 because of the script V9__do_not_run.sql
            var cnn = new SqlConnection(_dbContainer.GetCnxStr(DbName));
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Placeholders = new Dictionary<string, string> { ["${db}"] = DbName, ["${schema2}"] = "dbo" },
                TargetVersion = new MigrationVersion("8_9"),
            };

            // Assert
            evolve.AssertMigrateIsSuccessful(cnn, expectedNbMigration, SqlServer.MigrationFolder)
                  .AssertMigrateThrows<EvolveConfigurationException>(cnn, e => e.StartVersion = new MigrationVersion("3.0")) // Migrate should have failed because a least one migration has already been applied
                  .AssertMigrateThrows<EvolveValidationException>(cnn, e => e.StartVersion = MigrationVersion.MinVersion, SqlServer.ChecksumMismatchFolder) // Validation should fail because checksum mismatches
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0); // No migration needed. Database is already up to date.



            // Erase cancelled (EraseDisabled = true)
            evolve.IsEraseDisabled = true;
            Assert.Throws<EvolveConfigurationException>(() => evolve.Erase());
            Assert.True(cnn.State == ConnectionState.Closed);

            // Erase sucessfull
            evolve.IsEraseDisabled = false;
            evolve.Erase();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            // Migrate sucessfull after a validation error (MustEraseOnValidationError = true)
            evolve.Locations = new[] { SqlServer.MigrationFolder }; // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == expectedNbMigration, $"{expectedNbMigration} migrations should have been applied, not {evolve.NbMigration}.");
            evolve.Locations = new[] { SqlServer.ChecksumMismatchFolder }; // Migrate Sql_Scripts\Checksum_mismatch
            evolve.MustEraseOnValidationError = true;
            evolve.Migrate();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");
            Assert.True(evolve.NbMigration == 1, $"1 migration should have been applied, not {evolve.NbMigration}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            // Erase sucessfull
            evolve.IsEraseDisabled = false;
            evolve.Erase();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            // StartVersion = 2.0
            evolve.Locations = new[] { SqlServer.MigrationFolder }; // Migrate Sql_Scripts\Migration
            evolve.StartVersion = new MigrationVersion("2.0");
            evolve.Migrate();
            Assert.True(evolve.NbMigration == (expectedNbMigration - 1), $"{expectedNbMigration - 1} migrations should have been applied, not {evolve.NbMigration} (StartVersion tests).");
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration} (StartVersion tests).");
            evolve.StartVersion = MigrationVersion.MinVersion;
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration} (StartVersion tests).");
            evolve.StartVersion = new MigrationVersion("3.0");
            Assert.Throws<EvolveConfigurationException>(() => evolve.Migrate());
            Assert.True(cnn.State == ConnectionState.Closed);
        }
    }
}
