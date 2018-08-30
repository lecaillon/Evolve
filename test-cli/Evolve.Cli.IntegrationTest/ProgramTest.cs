using System.Collections.Generic;
using System.Diagnostics;
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
                    appPath: TestContext.IntegrationTestPostgreSqlFolder,
                    args: $"-l Resources/Sql_Scripts/Migration --schemas public unittest -s unittest --placeholders schema1:unittest");

                Assert.True(stderr == string.Empty, stderr);
            }
        }

        private string RunCliExe(string cnxStr, string driver, string command, string appPath, string args)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = TestContext.CliExe,
                    Arguments = $"{driver} {command} -c \"{cnxStr}\" -p {appPath} {args}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                }
            };

            proc.Start();
            proc.WaitForExit();
            return proc.StandardError.ReadToEnd();
        }
    }
}
