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
            evolve.AssertInfoIsSuccessfulV2(cnn)
                  .ChangeLocations(SqlServer.MigrationFolder)
                  .AssertInfoIsSuccessfulV2(cnn)
                  .AssertMigrateIsSuccessfulV2(cnn)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.AssertMigrateThrows<EvolveConfigurationException>(cnn, e => e.StartVersion = new MigrationVersion("3.0"));

            evolve.ChangeLocations(SqlServer.ChecksumMismatchFolder)
                  .AssertMigrateThrows<EvolveValidationException>(cnn, e => e.StartVersion = MigrationVersion.MinVersion)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .ChangeLocations(SqlServer.MigrationFolder)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations()
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(SqlServer.MigrationFolder)
                  .AssertMigrateIsSuccessfulV2(cnn)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(SqlServer.ChecksumMismatchFolder)
                  .AssertMigrateIsSuccessfulV2(cnn, e => e.MustEraseOnValidationError = true)
                  .AssertInfoIsSuccessfulV2(cnn);

            
            evolve.ChangeLocations(SqlServer.MigrationFolder)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessfulV2(cnn, e => e.StartVersion = new MigrationVersion("2.0"))
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(SqlServer.MigrationFolder)
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessfulV2(cnn, e => e.StartVersion = MigrationVersion.MinVersion)
                  .AssertInfoIsSuccessfulV2(cnn);

            evolve.ChangeLocations(SqlServer.RepeatableFolder)
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 0)
                  .AssertMigrateIsSuccessfulV2(cnn);
        }
    }
}
