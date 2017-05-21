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
        [Fact(DisplayName = "Run_all_SQLServer_migrations_work")]
        public void Run_all_SQLServer_migrations_work()
        {
            var cnn = new SqlConnection($"Server=127.0.0.1;Database={TestContext.DbName};User Id={TestContext.DbUser};Password={TestContext.DbPwd};");
            var evolve = new Evolve(cnn, msg => Debug.WriteLine(msg))
            {
                Locations = new List<string> { TestContext.MigrationFolder },
                Placeholders = new Dictionary<string, string> { ["${db}"] = TestContext.DbName, ["${schema2}"] = "dbo" },
                TargetVersion = new MigrationVersion("8_9"),
            };

            int nbMigration = Directory.GetFiles(TestContext.MigrationFolder).Length - 1; // -1 because of the script V9__do_not_run.sql

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

            // Can test the erase schema on the master one. I should think to 
        }

        /// <summary>
        ///     Start SQL Server.
        /// </summary>
        public MigrationTest()
        {
            TestUtil.RunContainer();
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
