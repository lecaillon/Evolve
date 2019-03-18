using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.Sqlite
{
    public class MigrationTest
    {
        private readonly ITestOutputHelper _output;

        public MigrationTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Category(Test.SQLite)]
        public void Run_all_SQLite_migrations_work()
        {
            // Arrange
            int expectedNbMigration = Directory.GetFiles(SQLite.MigrationFolder).Length;
            var cnn = new SQLiteConnection($@"Data Source={Path.GetTempPath() + Guid.NewGuid().ToString()}.db;");
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Placeholders = new Dictionary<string, string> { ["${table4}"] = "table_4" },
            };

            // Assert
            evolve.AssertMigrateIsSuccessful(cnn, expectedNbMigration, locations: SQLite.MigrationFolder)
                  .AssertMigrateThrows<EvolveValidationException>(cnn, locations: SQLite.ChecksumMismatchFolder)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0)
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration, null, locations: SQLite.MigrationFolder)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 1, e => e.MustEraseOnValidationError = true, locations: SQLite.ChecksumMismatchFolder);
        }
    }
}
