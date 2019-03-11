﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
            int expectedNbMigration = Directory.GetFiles(SqlServer.MigrationFolder).Length - 1; // Total -1 because of the script V9__do_not_run.sql
            var cnn = new SqlConnection(_dbContainer.GetCnxStr(DbName));
            var evolve = new Evolve(cnn, msg => _output.WriteLine(msg))
            {
                Placeholders = new Dictionary<string, string> { ["${db}"] = DbName, ["${schema2}"] = "dbo" },
                TargetVersion = new MigrationVersion("8_9"),
            };

            // Assert
            evolve.AssertMigrateIsSuccessful(cnn, expectedNbMigration, locations: SqlServer.MigrationFolder)
                  .AssertMigrateThrows<EvolveConfigurationException>(cnn, e => e.StartVersion = new MigrationVersion("3.0")) // Migrate should have failed because a least one migration has already been applied
                  .AssertMigrateThrows<EvolveValidationException>(cnn, e => e.StartVersion = MigrationVersion.MinVersion, SqlServer.ChecksumMismatchFolder) // Validation should fail because checksum mismatches
                  .AssertRepairIsSuccessful(cnn, expectedNbReparation: 1)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0) // No migration needed. Database is already up to date.
                  .AssertEraseThrows<EvolveConfigurationException>(cnn, e => e.IsEraseDisabled = true) // Erase throws because option is disabled.
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration, locations: SqlServer.MigrationFolder)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 1, e => e.MustEraseOnValidationError = true, locations: SqlServer.ChecksumMismatchFolder) // Migrate fails then erases and migrates successfully.
                  .AssertEraseIsSuccessful(cnn, e => e.IsEraseDisabled = false)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration - 1, e => e.StartVersion = new MigrationVersion("2.0"), locations: SqlServer.MigrationFolder)
                  .AssertMigrateIsSuccessful(cnn, expectedNbMigration: 0, e => e.StartVersion = MigrationVersion.MinVersion); // No migration needed. Database is already up to date.
        }
    }
}
