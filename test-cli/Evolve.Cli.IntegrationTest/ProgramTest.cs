using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.Cli.IntegrationTest
{
    [Collection("Database collection")]
    public class ProgramTest
    {
        private readonly MySQLFixture _mySqlfixture;
        private readonly PostgreSqlFixture _pgFixture;
        private readonly SQLServerFixture _sqlServerFixture;
        private readonly CassandraFixture _cassandraFixture;

        public ProgramTest(MySQLFixture mySqlfixture, PostgreSqlFixture pgFixture, SQLServerFixture sqlServerFixture, CassandraFixture cassandraFixture)
        {
            _mySqlfixture = mySqlfixture;
            _pgFixture = pgFixture;
            _sqlServerFixture = sqlServerFixture;
            _cassandraFixture = cassandraFixture;

            if (!TestContext.AppVeyor)
            { // AppVeyor and Windows 2016 does not support linux docker images
                _mySqlfixture.Start();
                _pgFixture.Start();
                _sqlServerFixture.Start();
                _cassandraFixture.Start();
            }
        }

        [SkipOnLinuxFact]
        public void Cli_Exe_Erase_And_Migrate_PostgreSql()
        {
            foreach (var command in new List<string> { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    cnxStr: _pgFixture.CnxStr,
                    driver: "postgresql",
                    command: command,
                    driverAssemblyPath: TestContext.IntegrationTestPostgreSqlFolder,
                    args: $"-l Resources/Sql_Scripts/Migration --schemas public unittest -s unittest --placeholders schema1:unittest");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [SkipOnLinuxFact]
        public void Cli_Exe_Erase_And_Migrate_SqlServer()
        {
            foreach (var command in new List<string> { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    cnxStr: _sqlServerFixture.CnxStr.Replace("master", "my_database_2"),
                    driver: "sqlserver",
                    command: command,
                    driverAssemblyPath: TestContext.IntegrationTestSqlServerFolder,
                    args: $"-l Resources/Sql_Scripts/Migration --placeholders db:my_database_2 schema2:dbo --v 8_9");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [SkipOnLinuxFact]
        public void Cli_Exe_Erase_And_Migrate_MySql()
        {
            foreach (var command in new List<string> { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    cnxStr: _mySqlfixture.CnxStr,
                    driver: "mysql",
                    command: command,
                    driverAssemblyPath: TestContext.IntegrationTestMySqlFolder,
                    args: $"-l Resources/Sql_Scripts/Migration --command-timeout 25");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [SkipOnLinuxFact]
        public void Cli_Exe_Erase_And_Migrate_MySqlConnector()
        {
            foreach (var command in new List<string> { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    cnxStr: _mySqlfixture.CnxStr,
                    driver: "mysqlconnector",
                    command: command,
                    driverAssemblyPath: TestContext.IntegrationTestMySqlConnectorFolder,
                    args: $"-l {TestContext.IntegrationTestMySqlConnectorResourcesFolder} --command-timeout 25");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [SkipOnLinuxFact]
        public void Cli_Exe_Erase_And_Migrate_Sqlite()
        {
            string cnxStr = $"Data Source={Path.GetTempPath() + Guid.NewGuid().ToString()}.db";

            foreach (var command in new List<string> { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    cnxStr: cnxStr,
                    driver: "sqlite",
                    command: command,
                    driverAssemblyPath: TestContext.IntegrationTestSQLiteFolder,
                    args: $"-l Resources/Sql_Scripts/Migration --placeholders table4:table_4");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [SkipOnLinuxFact]
        public void Cli_Exe_Erase_And_Migrate_MicrosoftSqlite()
        {
            string cnxStr = $"Data Source={Path.GetTempPath() + Guid.NewGuid().ToString()}.db";

            foreach (var command in new List<string> { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    cnxStr: cnxStr,
                    driver: "microsoftsqlite",
                    command: command,
                    driverAssemblyPath: TestContext.IntegrationTestMicrosoftSQLiteFolder,
                    args: $"-l {TestContext.IntegrationTestMicrosoftSQLiteResourcesFolder} --placeholders table4:table_4");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [SkipOnLinuxFact(Skip = "Not enough replica...")]
        public void Cli_Exe_Erase_And_Migrate_Cassandra()
        {
            foreach (var command in new List<string> { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    cnxStr: _cassandraFixture.CnxStr,
                    driver: "cassandra",
                    command: command,
                    driverAssemblyPath: TestContext.IntegrationTestCassandraFolder,
                    args: $"-l Resources/Sql_Scripts/Migration -k my_keyspace -t evolve_change_log --scripts-suffix .cql --command-timeout 25 ");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        private string RunCliExe(string cnxStr, string driver, string command, string driverAssemblyPath, string args)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = TestContext.CliExe,
                    Arguments = $"{driver} {command} -c \"{cnxStr}\" -p {driverAssemblyPath} {args}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };

            proc.Start();
            proc.WaitForExit();
            string stdout = proc.StandardOutput.ReadToEnd();

            return proc.StandardError.ReadToEnd();
        }
    }
}
