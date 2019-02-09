using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Integration.MySQL
{
    [Collection("MySQL collection")]
    public class MigrationTest
    {
        private readonly MySQLFixture _mySQLFixture;
        private readonly ITestOutputHelper _output;

        public MigrationTest(MySQLFixture mySQLFixture, ITestOutputHelper output)
        {
            _mySQLFixture = mySQLFixture;
            _output = output;

            if (TestContext.Local)
            {
                mySQLFixture.Run(fromScratch: true);
            }
        }

        [Fact]
        [Category(Test.MySQL)]
        public void Run_all_MySQL_migrations_work()
        {
            var cnn = _mySQLFixture.CreateDbConnection();
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Locations = new List<string> { TestContext.MySQL.MigrationFolder },
                CommandTimeout = 25
            };

            int nbMigration = Directory.GetFiles(TestContext.MySQL.MigrationFolder).Length;

            // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");

            // Migrate: nothing to do. Database is already up to date.
            evolve.Migrate();
            Assert.True(evolve.NbMigration == 0, $"There should be no more migration after a successful one, not {evolve.NbMigration}.");
            Assert.True(cnn.State == ConnectionState.Closed);

            // Migrate Sql_Scripts\Checksum_mismatch: validation should fail due to a checksum mismatch.
            evolve.Locations = new List<string> { TestContext.MySQL.ChecksumMismatchFolder };
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
            evolve.Locations = new List<string> { TestContext.MySQL.MigrationFolder }; // Migrate Sql_Scripts\Migration
            evolve.Migrate();
            Assert.True(evolve.NbMigration == nbMigration, $"{nbMigration} migrations should have been applied, not {evolve.NbMigration}.");
            evolve.Locations = new List<string> { TestContext.MySQL.ChecksumMismatchFolder }; // Migrate Sql_Scripts\Checksum_mismatch
            evolve.MustEraseOnValidationError = true;
            evolve.Migrate();
            Assert.True(evolve.NbSchemaErased == evolve.Schemas.Count(), $"{evolve.Schemas.Count()} schemas should have been erased, not {evolve.NbSchemaErased}.");
            Assert.True(evolve.NbMigration == 1, $"1 migration should have been applied, not {evolve.NbMigration}.");
            Assert.True(cnn.State == ConnectionState.Closed);
        }
    }
}
