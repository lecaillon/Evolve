using EvolveDb.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static EvolveDb.Tests.TestContext;

namespace EvolveDb.Tests.Integration.MySql
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
            var cnn = _dbContainer.CreateDbConnection();
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                EmbeddedResourceAssemblies = new[] { typeof(TestContext).Assembly },
                EmbeddedResourceFilters = new[] { MySQL.MigrationFolderFilter },
                CommandTimeout = 25
            };

            // Assert
            evolve.ChangeLocations(MySQL.MigrationFolder)
                  .AssertInfoIsSuccessful(cnn)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.UseFileMigrationLoader()
                  .ChangeLocations(MySQL.ChecksumMismatchFolder)
                  .AssertMigrateThrows<EvolveValidationException>(cnn)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .ChangeLocations(MySQL.MigrationFolder)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations()
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(MySQL.MigrationFolder)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(MySQL.ChecksumMismatchFolder)
                  .AssertMigrateIsSuccessful(cnn, e => e.MustEraseOnValidationError = true)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(MySQL.MigrationFolder)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(MySQL.RepeatableFolder)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 0)
                  .AssertMigrateIsSuccessful(cnn);
        }
    }
}
