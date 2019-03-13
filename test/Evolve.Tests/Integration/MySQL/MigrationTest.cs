using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.MySql
{
    [Collection("MySQL collection")]
    public class MigrationTest
    {
        private readonly MySQLFixture _dbContainer;
        private readonly ITestOutputHelper _output;

        public MigrationTest(MySQLFixture dbContainer, ITestOutputHelper output)
        {
            _dbContainer = dbContainer;
            _output = output;

            if (Local)
            {
                dbContainer.Run(fromScratch: true);
            }
        }

        [Fact]
        [Category(Test.MySQL)]
        public void Run_all_MySQL_migrations_work()
        {
            // Arrange
            int expectedNbMigration = Directory.GetFiles(MySQL.MigrationFolder).Length;
            var cnn = _dbContainer.CreateDbConnection();
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                EmbeddedResourceAssemblies = new[] { typeof(TestContext).Assembly },
                EmbeddedResourceFilters = new[] { MySQL.MigrationFolderFilter },
                CommandTimeout = 25
            };

            // Assert
            evolve.AssertMigrateIsSuccessful(cnn, expectedNbMigration)
                  .AssertMigrateThrows<EvolveValidationException>(cnn, e => e.EmbeddedResourceAssemblies = new List<Assembly>(), locations: MySQL.ChecksumMismatchFolder)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0)
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration, null, locations: MySQL.MigrationFolder)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 1, e => e.MustEraseOnValidationError = true, locations: MySQL.ChecksumMismatchFolder);
        }
    }
}
