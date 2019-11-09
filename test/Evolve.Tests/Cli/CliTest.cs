using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Evolve.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Evolve.Tests.Cli
{
    [Collection("Database collection")]
    public class CliTest
    {
        private readonly PostgreSqlFixture _pgContainer;
        private readonly MySQLFixture _mySQLContainer;
        private readonly SQLServerFixture _sqlServerContainer;
        private readonly CassandraFixture _cassandraContainer;
        private readonly CockroachDBFixture _cockroachDBContainer;
        private readonly ITestOutputHelper _output;

        public CliTest(PostgreSqlFixture pgContainer, MySQLFixture mySQLContainer, SQLServerFixture sqlServerContainer, CassandraFixture cassandraContainer, CockroachDBFixture cockroachDBContainer, ITestOutputHelper output)
        {
            _pgContainer = pgContainer;
            _mySQLContainer = mySQLContainer;
            _sqlServerContainer = sqlServerContainer;
            _cassandraContainer = cassandraContainer;
            _cockroachDBContainer = cockroachDBContainer;
            _output = output;

            if (TestContext.Local || TestContext.AzureDevOps)
            {
                pgContainer.Run();
                sqlServerContainer.Run();
                cassandraContainer.Run();
                cockroachDBContainer.Run();
                if (TestContext.Local)
                {
                    mySQLContainer.Run();
                }
            }
        }

        [FactSkippedOnAppVeyor]
        [Category(Test.Cli, Test.CockroachDB)]
        public void CockroachDB_Should_Run_All_Cli_Commands()
        {
            foreach (var command in new[] { "erase", "migrate", "repair", "info" })
            {
                string stderr = RunCli(
                    db: "cockroachdb",
                    command: command,
                    cnxStr: _cockroachDBContainer.CnxStr,
                    location: TestContext.CockroachDB.MigrationFolder,
                    args: "-s evolve -s defaultdb");

                Assert.True(string.IsNullOrEmpty(stderr), stderr);
            }
        }

        [FactSkippedOnAppVeyor]
        [Category(Test.Cli, Test.Cassandra)]
        public void Cassandra_Should_Run_All_Cli_Commands()
        {
            string metadataKeyspaceName = "my_keyspace_3";

            foreach (var command in new[] { "erase", "migrate", "repair", "info" })
            {
                string stderr = RunCli(
                    db: "cassandra",
                    command: command,
                    cnxStr: _cassandraContainer.CnxStr,
                    location: TestContext.CassandraDb.MigrationFolder,
                    args: $"--scripts-suffix .cql -p keyspace:{metadataKeyspaceName} --keyspace {metadataKeyspaceName} --metadata-table-keyspace evolve_change_log");

                Assert.True(string.IsNullOrEmpty(stderr), stderr);
            }
        }

        [Fact]
        [Category(Test.Cli, Test.MySQL)]
        public void MySQL_With_Embedded_Resources_Should_Run_All_Cli_Commands()
        {
            foreach (var command in new[] { "erase", "migrate", "repair", "info" })
            {
                string stderr = RunCli(
                    db: "mysql",
                    command: command,
                    cnxStr: _mySQLContainer.CnxStr,
                    location: null,
                    args: $"-a Evolve.Tests.dll -f {TestContext.MySQL.MigrationFolderFilter}");

                Assert.True(string.IsNullOrEmpty(stderr), stderr);
            }
        }

        [Fact]
        [Category(Test.Cli, Test.PostgreSQL)]
        public void PostgreSql_Should_Run_All_Cli_Commands()
        {
            foreach (var command in new [] { "erase", "migrate", "repair", "info" })
            {
                string stderr = RunCli(
                    db: "postgresql",
                    command: command,
                    cnxStr: _pgContainer.CnxStr,
                    location: TestContext.PostgreSQL.MigrationFolder,
                    args: "-s public -s unittest --metadata-table-schema unittest --erase-disabled false -p schema1:unittest");

                Assert.True(string.IsNullOrEmpty(stderr), stderr);
            }
        }

        [Fact]
        [Category(Test.Cli, Test.SQLServer)]
        public void SQLServer_Should_Run_All_Cli_Commands()
        {
            string dbName = "my_database_3";
            TestUtil.CreateSqlServerDatabase(dbName, _sqlServerContainer.GetCnxStr("master"));

            foreach (var command in new[] { "erase", "migrate", "repair", "info" })
            {
                string stderr = RunCli(
                    db: "sqlserver",
                    command: command,
                    cnxStr: _sqlServerContainer.GetCnxStr(dbName),
                    location: TestContext.SqlServer.MigrationFolder,
                    args: $"-p db:{dbName} -p schema2:dbo --target-version 8_9");

                Assert.True(string.IsNullOrEmpty(stderr), stderr);
            }
        }

        [Fact]
        [Category(Test.Cli, Test.SQLite)]
        public void SQLite_Should_Run_All_Cli_Commands()
        {
            string sqliteCnxStr = $"Data Source={Path.GetTempPath() + Guid.NewGuid().ToString()}.db";

            foreach (var command in new[] { "erase", "migrate", "repair", "info" })
            {
                string stderr = RunCli(
                    db: "sqlite",
                    command: command,
                    cnxStr: sqliteCnxStr,
                    location: TestContext.SQLite.MigrationFolder,
                    args: "-p table4:table_4");

                Assert.True(string.IsNullOrEmpty(stderr), stderr);
            }
        }

        private string RunCli(string db, string command, string cnxStr, string location, string args)
        {
            string commandLineArgs = location is null
                ? $"{command} {db} -c \"{cnxStr}\" {args}"
                : $"{command} {db} -c \"{cnxStr}\" -l {location} {args}";

            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? TestContext.CliExe : TestContext.Cli,
                    Arguments = commandLineArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            proc.Start();
            _output.WriteLine(proc.StandardOutput.ReadToEnd());
            proc.WaitForExit();
            return proc.StandardError.ReadToEnd();
        }
    }
}
