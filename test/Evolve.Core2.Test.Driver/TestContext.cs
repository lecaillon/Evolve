using System;
using System.IO;
using System.Reflection;
using Evolve.Test.Utilities;
using Xunit;

namespace Evolve.Core2.Test.Driver
{
    public static class TestContext
    {
        static TestContext()
        {
            ProjectFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(TestContext).GetTypeInfo().Assembly.Location), @"../../../"));
            NetCore20DriverResourcesProjectFolder = Path.GetFullPath(Path.Combine(ProjectFolder, @"../Evolve.Core2.Test.Resources.SupportedDrivers"));
#if DEBUG
            NetCore20DepsFile = Path.Combine(NetCore20DriverResourcesProjectFolder, @"bin/Debug/netcoreapp2.0/Evolve.Core2.Test.Resources.SupportedDrivers.deps.json");
#else
            NetCore20DepsFile = Path.Combine(NetCore20DriverResourcesProjectFolder, @"bin/Release/netcoreapp2.0/Evolve.Core2.Test.Resources.SupportedDrivers.deps.json");
#endif
        }

        public static string ProjectFolder { get; }
        public static string NetCore20DriverResourcesProjectFolder { get; }
        public static string NetCore20DepsFile { get; }
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
