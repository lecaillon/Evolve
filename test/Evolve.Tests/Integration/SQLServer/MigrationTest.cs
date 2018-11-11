using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Evolve.Migration;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Integration.SQLServer
{
    [Collection("Database collection")]
    public class MigrationTest
    {
        public const string DbName = "my_database_2";
        private readonly SQLServerFixture _sqlServerContainer;
        private readonly ITestOutputHelper _output;

        public MigrationTest(SQLServerFixture sqlServerContainer, ITestOutputHelper output)
        {
            _sqlServerContainer = sqlServerContainer;
            _output = output;

            if (TestContext.Local || TestContext.AzureDevOps)
            {
                sqlServerContainer.Run(fromScratch: true);
            }

            TestUtil.CreateSqlServerDatabase(DbName, _sqlServerContainer.GetCnxStr("master"));
        }

        [Fact]
        public void Run_all_SQLServer_migrations_work()
        {
            var cnn = new SqlConnection(_sqlServerContainer.GetCnxStr(DbName));
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Locations = new List<string> { TestContext.SqlServer.MigrationFolder },
                Placeholders = new Dictionary<string, string> { ["${db}"] = DbName, ["${schema2}"] = "dbo" },
                TargetVersion = new MigrationVersion("8_9"),
            };

            int nbMigration = Directory.GetFiles(TestContext.SqlServer.MigrationFolder).Length - 1; // -1 because of the script V9__do_not_run.sql

            // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            // Migrate: nothing to do. Database is already up to date.
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            // StartVersion fails because migraiton scripts had been applied.
            evolve.StartVersion = new MigrationVersion("3.0");
            Assert.Throws<EvolveConfigurationException>(() => evolve.Migrate());
            evolve.StartVersion = MigrationVersion.MinVersion;
            Assert.True(cnn.State == ConnectionState.Closed);

            // Migrate Sql_Scripts\Checksum_mismatch: validation should fail due to a checksum mismatch.
            evolve.Locations = new List<string> { TestContext.SqlServer.ChecksumMismatchFolder };
            Assert.Throws<EvolveValidationException>(() => evolve.Migrate());
            Assert.True(cnn.State == ConnectionState.Closed);

            // Repair sucessfull
            evolve.Repair();
            Assert.True(evolve.NbReparation == 1, $"There should be 1 migration repaired, not {evolve.NbReparation}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            // Migrate: nothing to do. Database is already up to date.
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration}.");
            Assert.True(cnn.State == ConnectionState.Closed);

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
            evolve.Locations = new List<string> { TestContext.SqlServer.MigrationFolder }; // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");
            evolve.Locations = new List<string> { TestContext.SqlServer.ChecksumMismatchFolder }; // Migrate Sql_Scripts\Checksum_mismatch
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
            evolve.Locations = new List<string> { TestContext.SqlServer.MigrationFolder }; // Migrate Sql_Scripts\Migration
            evolve.StartVersion = new MigrationVersion("2.0");
            evolve.Migrate();
            Assert.True(evolve.NbMigration == (nbMigration - 1), $"{nbMigration - 1} migrations should have been applied, not {evolve.NbMigration} (StartVersion tests).");
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
