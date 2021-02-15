using System.Collections.Generic;
using System.Data.SqlClient;
using Evolve.Migration;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static Evolve.Tests.TestContext;

namespace Evolve.Tests.Integration.SQLServer
{
    [Collection("SQLServer collection")]
    public class MigrationTest
    {
        public const string DbName = "my_database_2";
        private readonly SQLServerFixture _dbContainer;
        private readonly ITestOutputHelper _output;

        public MigrationTest(SQLServerFixture dbContainer, ITestOutputHelper output)
        {
            _dbContainer = dbContainer;
            _output = output;

            if (Local)
            {
                dbContainer.Run(fromScratch: true);
            }

            TestUtil.CreateSqlServerDatabase(DbName, _dbContainer.GetCnxStr("master"));
        }

        [Fact]
        [Category(Test.SQLServer)]
        public void Run_all_SQLServer_migrations_work()
        {
            // Arrange
            var cnn = new SqlConnection(_dbContainer.GetCnxStr(DbName));
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Placeholders = new Dictionary<string, string> { ["${db}"] = DbName, ["${schema2}"] = "dbo" },
                TargetVersion = new MigrationVersion("8_9"),
            };

            // Assert
            evolve.AssertInfoIsSuccessful(cnn)
                  .ChangeLocations(SqlServer.MigrationFolder)
                  .AssertInfoIsSuccessful(cnn)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.AssertMigrateThrows<EvolveConfigurationException>(cnn, e => e.StartVersion = new MigrationVersion("3.0"));

            evolve.ChangeLocations(SqlServer.ChecksumMismatchFolder)
                  .AssertMigrateThrows<EvolveValidationException>(cnn, e => e.StartVersion = MigrationVersion.MinVersion)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .ChangeLocations(SqlServer.MigrationFolder)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations()
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(SqlServer.MigrationFolder)
                  .AssertMigrateIsSuccessful(cnn)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(SqlServer.ChecksumMismatchFolder)
                  .AssertMigrateIsSuccessful(cnn, e => e.MustEraseOnValidationError = true)
                  .AssertInfoIsSuccessful(cnn);

            
            evolve.ChangeLocations(SqlServer.MigrationFolder)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, e => e.StartVersion = new MigrationVersion("2.0"))
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(SqlServer.MigrationFolder)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, e => e.StartVersion = MigrationVersion.MinVersion)
                  .AssertInfoIsSuccessful(cnn);

            evolve.ChangeLocations(SqlServer.RepeatableFolder)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 0)
                  .AssertMigrateIsSuccessful(cnn);
        }
    }
}
