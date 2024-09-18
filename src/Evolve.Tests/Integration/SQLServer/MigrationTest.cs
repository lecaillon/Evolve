using EvolveDb.Migration;
using EvolveDb.Tests.Infrastructure;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;
using static EvolveDb.Tests.TestContext;

namespace EvolveDb.Tests.Integration.SQLServer
{
    public record MigrationTest(ITestOutputHelper Output) : DbContainerFixture<SQLServerContainer>
    {
        public const string DbName = "my_database_2";

        [Fact(Skip = "Not working")]
        [Category(Test.SQLServer)]
        public void Run_all_SQLServer_migrations_work()
        {
            // Arrange
            TestUtil.CreateSqlServerDatabase(DbName, CnxStr);
            var cnn = new SqlConnection(CnxStr.Replace("master", DbName));
            var evolve = new Evolve(cnn, msg => Output.WriteLine(msg))
            {
                Placeholders = new Dictionary<string, string> { ["${db}"] = DbName, ["${schema2}"] = "dbo" },
                TargetVersion = new MigrationVersion("8_9"),
            };
            evolve.Erase();

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
