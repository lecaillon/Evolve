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
#else
            IntegrationTestPostgreSqlFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.PostgreSQL/bin/Release"));
#endif
        }

        public static string ProjectFolder { get; }
        public static string DistFolder { get; } 
        public static string CliExe { get; }
        public static string IntegrationTestPostgreSqlFolder { get; }
        public static bool AppVeyor => Environment.GetEnvironmentVariable("APPVEYOR") == "True";

        [CollectionDefinition("Database collection")]
        public class DatabaseCollection : ICollectionFixture<MySQLFixture>,
                                          ICollectionFixture<PostgreSqlFixture>,
                                          ICollectionFixture<SQLServerFixture>,
                                          ICollectionFixture<CassandraFixture> { }
    }
}
