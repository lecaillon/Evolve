using System;
using System.IO;
using System.Reflection;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.Core21.Test.Driver
{
    public static class TestContext
    {
        static TestContext()
        {
            ProjectFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(TestContext).GetTypeInfo().Assembly.Location), @"../../../"));
            NetCore21DriverResourcesProjectFolder = Path.GetFullPath(Path.Combine(ProjectFolder, @"../Evolve.Core21.Test.Resources.SupportedDrivers"));
#if DEBUG
            NetCore21DepsFile = Path.Combine(NetCore21DriverResourcesProjectFolder, @"bin/Debug/netcoreapp2.1/Evolve.Core21.Test.Resources.SupportedDrivers.deps.json");
#else
            NetCore21DepsFile = Path.Combine(NetCore21DriverResourcesProjectFolder, @"bin/Release/netcoreapp2.1/Evolve.Core21.Test.Resources.SupportedDrivers.deps.json");
#endif
        }

        public static string ProjectFolder { get; }
        public static string NetCore21DriverResourcesProjectFolder { get; }
        public static string NetCore21DepsFile { get; }
        public static string NugetPackageFolder => $@"{EnvHome}/.nuget/packages";
        public static string EnvHome => Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
        public static bool AppVeyor => Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        public static bool Travis => Environment.GetEnvironmentVariable("TRAVIS") == "True";
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<MySQLFixture>,
                                      ICollectionFixture<PostgreSqlFixture>,
                                      ICollectionFixture<SQLServerFixture>,
                                      ICollectionFixture<CassandraFixture> { }
}
