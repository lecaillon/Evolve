using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Evolve.Test.Utilities;
using Npgsql;
using Xunit;

namespace Evolve.IntegrationTest.PostgreSQL
{
    [Collection("Database collection")]
    public class MigrationTest
    {
        private readonly PostgreSqlFixture _pgFixture;

        public MigrationTest(PostgreSqlFixture pgFixture)
        {
            _pgFixture = pgFixture;
            if (!TestContext.Travis && !TestContext.AppVeyor)
            { // AppVeyor and Windows 2016 does not support linux docker images
                pgFixture.Start(fromScratch: true);
            }
        }

        [Fact(DisplayName = "Run_all_PostgreSQL_migrations_work")]
        public void Run_all_PostgreSQL_migrations_work()
        {
            var cnn = new NpgsqlConnection($"Server=127.0.0.1;Port={_pgFixture.HostPort};Database={_pgFixture.DbName};User Id={_pgFixture.DbUser};Password={_pgFixture.DbPwd};");
            var evolve = new Evolve(cnn, msg => Debug.WriteLine(msg))
            {
                Locations = new List<string> { TestContext.MigrationFolder },
                Schemas = new List<string> { "public", "unittest" },
                MetadataTableSchema = "unittest",
                Placeholders = new Dictionary<string, string> { ["${schema1}"] = "unittest" }
            };

            int nbMigration = Directory.GetFiles(TestContext.MigrationFolder).Length;

            // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            // Migrate: nothing to do. Database is already up to date.
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            // Migrate Sql_Scripts\Checksum_mismatch: validation should fail due to a checksum mismatch.
            evolve.Locations = new List<string> { TestContext.ChecksumMismatchFolder };
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

            // Migrate Sql_Scripts\OutOfOrder: validation should fail due to an unordered migration (OutOfOrder = false).
            evolve.Locations = new List<string> { TestContext.OutOfOrderFolder };
            Assert.Throws<EvolveValidationException>(() => evolve.Migrate());
            Assert.True(cnn.State == ConnectionState.Closed);

            // Migrate sucessfull: (OutOfOrder = true).
            evolve.OutOfOrder = true;
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 1, $"1 migration should have been applied, not {evolve.NbMigration}.");
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
            evolve.Locations = new List<string> { TestContext.MigrationFolder }; // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");
            evolve.Locations = new List<string> { TestContext.ChecksumMismatchFolder }; // Migrate Sql_Scripts\Checksum_mismatch
            evolve.MustEraseOnValidationError = true;
            evolve.Migrate();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");
            Assert.True(evolve.NbMigration == 1, $"1 migration should have been applied, not {evolve.NbMigration}.");
            Assert.True(cnn.State == ConnectionState.Closed);
        }
    }
}
