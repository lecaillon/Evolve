using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EvolveDb.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace EvolveDb.Tests.Cli
{
    public class CliTest
    {
        private readonly ITestOutputHelper _output;

        public CliTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [FactSkippedOnAppVeyorOrLocal]
        [Category(Test.Cli, Test.CockroachDB)]
        public async Task CockroachDB_Should_Run_All_Cli_Commands()
        {
            var container = new CockroachDBContainer();
            if (TestContext.Local || TestContext.AzureDevOps)
            {
                await container.Start();
            }

            foreach (var command in new[] { "erase", "migrate", "repair", "info" })
            {
                string stderr = RunCli(
                    db: "cockroachdb",
                    command: command,
                    cnxStr: container.CnxStr,
                    location: TestContext.CockroachDB.MigrationFolder,
                    args: "-s evolve -s defaultdb");

                Assert.True(string.IsNullOrEmpty(stderr), stderr);
            }
        }

        [FactSkippedOnAppVeyorOrLocal]
        [Category(Test.Cli, Test.Cassandra)]
        public async Task Cassandra_Should_Run_All_Cli_Commands()
        {
            var container = new CassandraContainer();
            if (TestContext.Local || TestContext.AzureDevOps)
            {
                await container.Start();
            }

            string metadataKeyspaceName = "my_keyspace_3";

            foreach (var command in new[] { "erase", "migrate", "repair", "info" })
            {
                string stderr = RunCli(
                    db: "cassandra",
                    command: command,
                    cnxStr: container.CnxStr,
                    location: TestContext.CassandraDb.MigrationFolder,
                    args: $"--scripts-suffix .cql -p keyspace:{metadataKeyspaceName} --keyspace {metadataKeyspaceName} --metadata-table-keyspace evolve_change_log");

                Assert.True(string.IsNullOrEmpty(stderr), stderr);
            }
        }

        [Fact]
        [Category(Test.Cli, Test.MySQL)]
        public async Task MySQL_With_Embedded_Resources_Should_Run_All_Cli_Commands()
        {
            var container = new MySQLContainer();
            if (TestContext.Local || TestContext.AzureDevOps)
            {
                await container.Start();
            }

            foreach (var command in new[] { "erase", "migrate", "repair", "info" })
            {
                string stderr = RunCli(
                    db: "mysql",
                    command: command,
                    cnxStr: container.CnxStr,
                    location: null,
                    args: $"-a Evolve.Tests.dll -f {TestContext.MySQL.MigrationFolderFilter}");

                Assert.True(string.IsNullOrEmpty(stderr), stderr);
            }
        }

        [Fact]
        [Category(Test.Cli, Test.PostgreSQL)]
        public async Task PostgreSql_Should_Run_All_Cli_Commands()
        {
            var container = new PostgreSqlContainer();
            if (TestContext.Local || TestContext.AzureDevOps)
            {
                await container.Start();
            }

            foreach (var command in new[] { "erase", "migrate", "repair", "info" })
            {
                string stderr = RunCli(
                    db: "postgresql",
                    command: command,
                    cnxStr: container.CnxStr.Replace(PostgreSqlContainer.DbPwd, "${pwd}"), // add secret to the connection string
                    location: TestContext.PostgreSQL.MigrationFolder,
                    args: $"-s public -s unittest --metadata-table-schema unittest --erase-disabled false -p schema1:unittest -p pwd:{PostgreSqlContainer.DbPwd}");

                Assert.True(string.IsNullOrEmpty(stderr), stderr);
            }
        }

        [Fact]
        [Category(Test.Cli, Test.SQLServer)]
        public async Task SQLServer_Should_Run_All_Cli_Commands()
        {
            var container = new SQLServerContainer();
            if (TestContext.Local || TestContext.AzureDevOps)
            {
                await container.Start();
            }

            string dbName = "my_database_3";
            TestUtil.CreateSqlServerDatabase(dbName, container.CnxStr);

            foreach (var command in new[] { "erase", "migrate", "repair", "info" })
            {
                string stderr = RunCli(
                    db: "sqlserver",
                    command: command,
                    cnxStr: container.CnxStr,
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
