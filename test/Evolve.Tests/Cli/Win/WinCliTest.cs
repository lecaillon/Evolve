using System;
using System.Diagnostics;
using System.IO;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Cli.Win
{
    [Collection("Database collection")]
    public class WinCliTest
    {
        private readonly PostgreSqlFixture _pgContainer;
        private readonly MySQLFixture _mySQLContainer;
        private readonly SQLServerFixture _sqlServerContainer;
        private readonly CassandraFixture _cassandraContainer;
        private readonly ITestOutputHelper _output;

        public WinCliTest(PostgreSqlFixture pgContainer, MySQLFixture mySQLContainer, SQLServerFixture sqlServerContainer, CassandraFixture cassandraContainer, ITestOutputHelper output)
        {
            _pgContainer = pgContainer;
            _mySQLContainer = mySQLContainer;
            _sqlServerContainer = sqlServerContainer;
            _cassandraContainer = cassandraContainer;
            _output = output;

            if (TestContext.Local || TestContext.AzureDevOps)
            {
                pgContainer.Run();
                mySQLContainer.Run();
                sqlServerContainer.Run();
                cassandraContainer.Run();
            }
        }

        [Fact]
        [Trait("Category", "Cli")]
        public void Erase_And_Migrate_Cassandra()
        {
            foreach (var command in new[] { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    db: "cassandra",
                    command: command,
                    cnxStr: _cassandraContainer.CnxStr,
                    location: TestContext.Cassandra.MigrationFolder,
                    args: "--scripts-suffix .cql --keyspace my_keyspace --metadata-table-keyspace evolve_change_log");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [Fact]
        [Trait("Category", "Cli")]
        public void Erase_And_Migrate_MySQL()
        {
            foreach (var command in new[] { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    db: "mysql",
                    command: command,
                    cnxStr: _mySQLContainer.CnxStr,
                    location: TestContext.MySQL.MigrationFolder,
                    args: "--command-timeout 25");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [Fact]
        [Trait("Category", "Cli")]
        public void Erase_And_Migrate_PostgreSql()
        {
            foreach (var command in new [] { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    db: "postgresql",
                    command: command,
                    cnxStr: _pgContainer.CnxStr,
                    location: TestContext.PostgreSQL.MigrationFolder,
                    args: "-s public -s unittest -p schema1:unittest");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [Fact]
        [Trait("Category", "Cli")]
        public void Erase_And_Migrate_SQLServer()
        {
            foreach (var command in new[] { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    db: "sqlserver",
                    command: command,
                    cnxStr: _sqlServerContainer.GetCnxStr("my_database_2"),
                    location: TestContext.SqlServer.MigrationFolder,
                    args: "-p db:my_database_2 -p schema2:dbo --target-version 8_9");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [Fact]
        [Trait("Category", "Cli")]
        public void Erase_And_Migrate_SQLite()
        {
            string sqliteCnxStr = $"Data Source={Path.GetTempPath() + Guid.NewGuid().ToString()}.db";

            foreach (var command in new[] { "erase", "migrate" })
            {
                string stderr = RunCliExe(
                    db: "sqlite",
                    command: command,
                    cnxStr: sqliteCnxStr,
                    location: TestContext.SQLite.MigrationFolder,
                    args: "-p table4:table_4");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        private string RunCliExe(string db, string command, string cnxStr, string location, string args)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = TestContext.CliExe,
                    Arguments = $"{db} {command} -c \"{cnxStr}\" -l {location} {args}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            proc.Start();
            proc.WaitForExit();
            _output.WriteLine(proc.StandardOutput.ReadToEnd());

            return proc.StandardError.ReadToEnd();
        }
    }
}
