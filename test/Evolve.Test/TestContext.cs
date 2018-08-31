using System;
using System.IO;
using System.Reflection;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.Test
{
    public static class TestContext
    {
        static TestContext()
        {
            ProjectFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(new Uri(typeof(TestContext).GetTypeInfo().Assembly.CodeBase).AbsolutePath), @"../../"));
            NetCore11DriverResourcesProjectFolder = Path.GetFullPath(Path.Combine(ProjectFolder, @"../Evolve.Core.Test.Resources.SupportedDrivers"));
            NetCore20DriverResourcesProjectFolder = Path.GetFullPath(Path.Combine(ProjectFolder, @"../Evolve.Core2.Test.Resources.SupportedDrivers"));
            NetCore21DriverResourcesProjectFolder = Path.GetFullPath(Path.Combine(ProjectFolder, @"../Evolve.Core21.Test.Resources.SupportedDrivers"));
#if DEBUG
            NetCore11DepsFile = Path.Combine(NetCore11DriverResourcesProjectFolder, @"bin/Debug/netcoreapp1.1/Evolve.Core.Test.Resources.SupportedDrivers.deps.json");
            NetCore20DepsFile = Path.Combine(NetCore20DriverResourcesProjectFolder, @"bin/Debug/netcoreapp2.0/Evolve.Core2.Test.Resources.SupportedDrivers.deps.json");
            NetCore21DepsFile = Path.Combine(NetCore21DriverResourcesProjectFolder, @"bin/Debug/netcoreapp2.1/Evolve.Core21.Test.Resources.SupportedDrivers.deps.json");
            IntegrationTestPostgreSqlFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.PostgreSQL/bin/Debug"));
            IntegrationTestMySqlFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.MySQL/bin/Debug"));
            IntegrationTestMySqlConnectorFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test-package/Evolve.MySqlConnector.ConsoleApp471.Test/bin/Debug"));
            IntegrationTestCassandraFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.Cassandra/bin/Debug"));
#else
            NetCore11DepsFile = Path.Combine(NetCore11DriverResourcesProjectFolder, @"bin/Release/netcoreapp1.1/Evolve.Core.Test.Resources.SupportedDrivers.deps.json");
            NetCore20DepsFile = Path.Combine(NetCore20DriverResourcesProjectFolder, @"bin/Release/netcoreapp2.0/Evolve.Core2.Test.Resources.SupportedDrivers.deps.json");
            NetCore21DepsFile = Path.Combine(NetCore21DriverResourcesProjectFolder, @"bin/Release/netcoreapp2.1/Evolve.Core21.Test.Resources.SupportedDrivers.deps.json");
            IntegrationTestPostgreSqlFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.PostgreSQL/bin/Release"));
            IntegrationTestMySqlFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.MySQL/bin/Release"));
            IntegrationTestMySqlConnectorFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test-package/Evolve.MySqlConnector.ConsoleApp471.Test/bin/Release"));
            IntegrationTestCassandraFolder = Path.GetFullPath(Path.Combine(ProjectFolder, "../../test/Evolve.IntegrationTest.Cassandra/bin/Release"));
#endif
            ResourcesFolder = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath), "Resources");
            AppConfigPath = Path.Combine(ResourcesFolder, "App.config");
            WebConfigPath = Path.Combine(ResourcesFolder, "Web.config");
            JsonConfigPath = Path.Combine(ResourcesFolder, "evolve.json");
            Json2ConfigPath = Path.Combine(ResourcesFolder, "evolve2.json");

            Environment.SetEnvironmentVariable("EVOLVE_HOST", "127.0.0.1");
            Environment.SetEnvironmentVariable("EVOLVE_DB_USER", "myUsername");
            Environment.SetEnvironmentVariable("EVOLVE_DB_PWD", "myPassword");
        }

        public static string ProjectFolder { get; }
        public static string NetCore11DriverResourcesProjectFolder { get; }
        public static string NetCore11DepsFile { get; }
        public static string NetCore20DriverResourcesProjectFolder { get; }
        public static string NetCore20DepsFile { get; }
        public static string NetCore21DriverResourcesProjectFolder { get; }
        public static string NetCore21DepsFile { get; }
        public static string NugetPackageFolder => $@"{EnvHome}/.nuget/packages";
        public static string EnvHome => Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
        public static string ResourcesFolder { get; }
        public static string AppConfigPath { get; }
        public static string WebConfigPath { get; }
        public static string JsonConfigPath { get; }
        public static string Json2ConfigPath { get; }
        public static string IntegrationTestPostgreSqlFolder { get; }
        public static string IntegrationTestMySqlFolder { get; }
        public static string IntegrationTestMySqlConnectorFolder { get; }
        public static string IntegrationTestCassandraFolder { get; }
        public static bool AppVeyor => Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        public static bool Travis => Environment.GetEnvironmentVariable("TRAVIS") == "True";

        [CollectionDefinition("Database collection")]
        public class DatabaseCollection : ICollectionFixture<MySQLFixture>,
                                          ICollectionFixture<PostgreSqlFixture>,
                                          ICollectionFixture<SQLServerFixture>,
                                          ICollectionFixture<CassandraFixture> { }
    }
}
