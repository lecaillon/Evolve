using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Evolve.Migration;
using Xunit;

namespace Evolve.IntegrationTest.SQLServer
{
    public class MigrationTest : IDisposable
    {
        public const string DbName = "my_database_2";

        [Fact(DisplayName = "Run_all_SQLServer_migrations_work")]
        public void Run_all_SQLServer_migrations_work()
        {
            var cnn = new SqlConnection($"Server=127.0.0.1;Database={DbName};User Id={TestContext.DbUser};Password={TestContext.DbPwd};");

            var evolve = new Evolve(cnn, msg => Debug.WriteLine(msg))
            {
                Locations = new List<string> { TestContext.MigrationFolder },
                Placeholders = new Dictionary<string, string> { ["${db}"] = DbName, ["${schema2}"] = "dbo" },
                TargetVersion = new MigrationVersion("8_9"),
            };

            int nbMigration = Directory.GetFiles(TestContext.MigrationFolder).Length - 1; // -1 because of the script V9__do_not_run.sql

            // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");

            // Migrate: nothing to do. Database is already up to date.
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration}.");

            // StartVersion fails because migraiton scripts had been applied.
            evolve.StartVersion = new MigrationVersion("3.0");
            Assert.Throws<EvolveConfigurationException>(() => evolve.Migrate());
            evolve.StartVersion = MigrationVersion.MinVersion;

            // Migrate Sql_Scripts\Checksum_mismatch: validation should fail due to a checksum mismatch.
            evolve.Locations = new List<string> { TestContext.ChecksumMismatchFolder };
            Assert.Throws<EvolveValidationException>(() => evolve.Migrate());

            // Repair sucessfull
            evolve.Repair();
            Assert.True(evolve.NbReparation == 1, $"There should be 1 migration repaired, not {evolve.NbReparation}.");

            // Migrate: nothing to do. Database is already up to date.
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration}.");

            // Erase cancelled (EraseDisabled = true)
            evolve.IsEraseDisabled = true;
            Assert.Throws<EvolveConfigurationException>(() => evolve.Erase());

            // Erase sucessfull
            evolve.IsEraseDisabled = false;
            evolve.Erase();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");

            // Migrate sucessfull after a validation error (MustEraseOnValidationError = true)
            evolve.Locations = new List<string> { TestContext.MigrationFolder }; // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");
            evolve.Locations = new List<string> { TestContext.ChecksumMismatchFolder }; // Migrate Sql_Scripts\Checksum_mismatch
            evolve.MustEraseOnValidationError = true;
            evolve.Migrate();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");
            Assert.True(evolve.NbMigration == 1, $"1 migration should have been applied, not {evolve.NbMigration}.");

            // Erase sucessfull
            evolve.IsEraseDisabled = false;
            evolve.Erase();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");

            // StartVersion = 2.0
            evolve.Locations = new List<string> { TestContext.MigrationFolder }; // Migrate Sql_Scripts\Migration
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
        }

        /// <summary>
        ///     Start SQL Server.
        ///     Create test database.
        /// </summary>
        public MigrationTest()
        {
            TestUtil.RunContainer();
            TestUtil.CreateTestDatabase(DbName);
        }

        /// <summary>
        ///     Stop SQL Server and remove container.
        /// </summary>
        public void Dispose()
        {
            TestUtil.RemoveContainer();
        }
    }
}
