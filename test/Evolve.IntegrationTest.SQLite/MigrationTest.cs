using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Evolve.IntegrationTest.SQLite
{
    public class MigrationTest
    {
        [Fact(DisplayName = "Run_all_SQLite_migrations_work")]
        public void Run_all_SQLite_migrations_work()
        {
            var cnn = new SQLiteConnection($@"Data Source={Path.GetTempPath() + Guid.NewGuid().ToString()}.db;");
            var evolve = new Evolve(cnn, msg => Debug.WriteLine(msg))
            {
                Locations = new List<string> { TestContext.MigrationFolder },
                Placeholders = new Dictionary<string, string> { ["${table4}"] = "table_4" },
            };

            int nbMigration = Directory.GetFiles(TestContext.MigrationFolder).Length;

            // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");

            // Migrate: nothing to do. Database is already up to date.
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration}.");

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
        }
    }
}
