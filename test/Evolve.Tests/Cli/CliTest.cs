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
        private readonly ITestOutputHelper _output;

        public CliTest(PostgreSqlFixture pgContainer, MySQLFixture mySQLContainer, SQLServerFixture sqlServerContainer, CassandraFixture cassandraContainer, ITestOutputHelper output)
        {
            _pgContainer = pgContainer;
            _mySQLContainer = mySQLContainer;
            _sqlServerContainer = sqlServerContainer;
            _cassandraContainer = cassandraContainer;
            _output = output;

            if (TestContext.Local || TestContext.AzureDevOps)
            {
                pgContainer.Run();
                sqlServerContainer.Run();
                cassandraContainer.Run();
                if (TestContext.Local)
                {
                    mySQLContainer.Run();
                }
            }
        }

        [FactSkippedOnAppVeyor]
        [Category(Test.Cli, Test.Cassandra)]
        public void Erase_And_Migrate_Cassandra()
        {
            string metadataKeyspaceName = "my_keyspace_3";

            foreach (var command in new[] { "erase", "migrate" })
            {
                string stderr = RunCli(
                    db: "cassandra",
                    command: command,
                    cnxStr: _cassandraContainer.CnxStr,
                    location: TestContext.Cassandra.MigrationFolder,
                    args: $"--scripts-suffix .cql -p keyspace:{metadataKeyspaceName} --keyspace {metadataKeyspaceName} --metadata-table-keyspace evolve_change_log");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [Fact]
        [Category(Test.Cli, Test.MySQL)]
        public void Erase_And_Migrate_MySQL_With_Embedded_Resources()
        {
            foreach (var command in new[] { "erase", "migrate" })
            {
                string stderr = RunCli(
                    db: "mysql",
                    command: command,
                    cnxStr: _mySQLContainer.CnxStr,
                    location: null,
                    args: "-a Evolve.Tests.dll -f Evolve.Tests.Integration.MySQL.Resources.Sql_Scripts.Migration");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [Fact]
        [Category(Test.Cli, Test.PostgreSQL)]
        public void Erase_And_Migrate_PostgreSql()
        {
            foreach (var command in new [] { "erase", "migrate" })
            {
                string stderr = RunCli(
                    db: "postgresql",
                    command: command,
                    cnxStr: _pgContainer.CnxStr,
                    location: TestContext.PostgreSQL.MigrationFolder,
                    args: "-s public -s unittest -p schema1:unittest");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [Fact]
        [Category(Test.Cli, Test.SQLServer)]
        public void Erase_And_Migrate_SQLServer()
        {
            string dbName = "my_database_3";
            TestUtil.CreateSqlServerDatabase(dbName, _sqlServerContainer.GetCnxStr("master"));

            foreach (var command in new[] { "erase", "migrate" })
            {
                string stderr = RunCli(
                    db: "sqlserver",
                    command: command,
                    cnxStr: _sqlServerContainer.GetCnxStr(dbName),
                    location: TestContext.SqlServer.MigrationFolder,
                    args: $"-p db:{dbName} -p schema2:dbo --target-version 8_9");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        [Fact]
        [Category(Test.Cli, Test.SQLite)]
        public void Erase_And_Migrate_SQLite()
        {
            string sqliteCnxStr = $"Data Source={Path.GetTempPath() + Guid.NewGuid().ToString()}.db";

            foreach (var command in new[] { "erase", "migrate" })
            {
                string stderr = RunCli(
                    db: "sqlite",
                    command: command,
                    cnxStr: sqliteCnxStr,
                    location: TestContext.SQLite.MigrationFolder,
                    args: "-p table4:table_4");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        private string RunCli(string db, string command, string cnxStr, string location, string args)
        {
            string commandLineArgs = location is null
                ? $"{command} {db} -c \"{cnxStr}\" {args}"
                : $"{command} {db} -c \"{cnxStr}\" -l {location} {args}";

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? TestContext.CliExe : TestContext.Cli,
                    Arguments = $"{command} {db} -c \"{cnxStr}\" -l {location} {args}",
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
