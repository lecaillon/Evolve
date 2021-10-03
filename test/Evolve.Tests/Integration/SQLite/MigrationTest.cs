using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using static EvolveDb.Tests.TestContext;

namespace EvolveDb.Tests.Integration.Sqlite
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
            var cnn = new SQLiteConnection($@"Data Source={Path.GetTempPath() + Guid.NewGuid().ToString()}.db;");
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Placeholders = new Dictionary<string, string> { ["${table4}"] = "table_4" },
            };

            // Assert
            evolve.AssertInfoIsSuccessful(cnn)
                  .ChangeLocations(SQLite.MigrationFolder)
                  .AssertInfoIsSuccessful(cnn)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(SQLite.ChecksumMismatchFolder)
                  .AssertMigrateThrows<EvolveValidationException>(cnn)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .ChangeLocations(SQLite.MigrationFolder)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations()
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(SQLite.MigrationFolder)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(SQLite.ChecksumMismatchFolder)
                  .AssertMigrateIsSuccessful(cnn, e => e.MustEraseOnValidationError = true)
                  .AssertInfoIsSuccessful(cnn);
        }
    }
}
