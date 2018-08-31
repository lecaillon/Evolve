using System;
using System.IO;
using System.Reflection;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.Cli.IntegrationTest
{
    public static class TestContext
    {
        static TestContext()
        {
            ProjectFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(new Uri(typeof(TestContext).GetTypeInfo().Assembly.CodeBase).AbsolutePath), @"../../../"));
            DistFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../dist"));
            CliExe = Path.GetFullPath(Path.Combine(DistFolder, "Evolve.exe"));
#if DEBUG
            IntegrationTestPostgreSqlFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.PostgreSQL/bin/Debug"));
            IntegrationTestSqlServerFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.SQLServer/bin/Debug"));
            IntegrationTestMySqlFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.MySQL/bin/Debug"));
            IntegrationTestMySqlConnectorFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test-package/Evolve.MySqlConnector.ConsoleApp471.Test/bin/Debug"));
            IntegrationTestMySqlConnectorResourcesFolder = "../../../../test/Evolve.IntegrationTest.MySQL/bin/Debug/Resources/Sql_Scripts/Migration";
            IntegrationTestSQLiteFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.SQLite/bin/Debug"));
            IntegrationTestMicrosoftSQLiteFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test-package/Evolve.Microsoft.Data.SQLite.AspNet471.Test/bin"));
            IntegrationTestMicrosoftSQLiteResourcesFolder = "../../../test/Evolve.IntegrationTest.SQLite/bin/Debug/Resources/Sql_Scripts/Migration";
            IntegrationTestCassandraFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.Cassandra/bin/Debug"));
#else
            IntegrationTestPostgreSqlFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.PostgreSQL/bin/Release"));
            IntegrationTestSqlServerFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.SQLServer/bin/Release"));
            IntegrationTestMySqlFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.MySQL/bin/Release"));
            IntegrationTestMySqlConnectorFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test-package/Evolve.MySqlConnector.ConsoleApp471.Test/bin/Release"));
            IntegrationTestSQLiteFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.SQLite/bin/Release"));
            IntegrationTestMicrosoftSQLiteFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test-package/Evolve.Microsoft.Data.SQLite.AspNet471.Test/bin"));
            IntegrationTestMicrosoftSQLiteResourcesFolder = "../../../test/Evolve.IntegrationTest.SQLite/bin/Release/Resources/Sql_Scripts/Migration";
            IntegrationTestCassandraFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.Cassandra/bin/Release"));
#endif
        }

        public static string ProjectFolder { get; }
        public static string DistFolder { get; } 
        public static string CliExe { get; }
        public static string IntegrationTestPostgreSqlFolder { get; }
        public static string IntegrationTestSqlServerFolder { get; }
        public static string IntegrationTestMySqlFolder { get; }
        public static string IntegrationTestMySqlConnectorFolder { get; }
        public static string IntegrationTestMySqlConnectorResourcesFolder { get; }
        public static string IntegrationTestSQLiteFolder { get; }
        public static string IntegrationTestMicrosoftSQLiteFolder { get; }
        public static string IntegrationTestMicrosoftSQLiteResourcesFolder { get; }
        public static string IntegrationTestCassandraFolder { get; }
        public static bool AppVeyor => Environment.GetEnvironmentVariable("APPVEYOR") == "True";

        [CollectionDefinition("Database collection")]
        public class DatabaseCollection : ICollectionFixture<MySQLFixture>,
                                          ICollectionFixture<PostgreSqlFixture>,
                                          ICollectionFixture<SQLServerFixture>,
                                          ICollectionFixture<CassandraFixture> { }
    }
}
