using System;
using System.IO;
using System.Reflection;

namespace Evolve.Core.Test.Driver
{
    public static class TestContext
    {
        static TestContext()
        {
            ProjectFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(TestContext).GetTypeInfo().Assembly.Location), @"../../../"));
            DriverResourcesProjectFolder = Path.GetFullPath(Path.Combine(ProjectFolder, @"../Evolve.Core.Test.Resources.SupportedDrivers"));
#if DEBUG
            DriverResourcesDepsFile = Path.Combine(DriverResourcesProjectFolder, @"bin/Debug/netcoreapp1.0/Evolve.Core.Test.Resources.SupportedDrivers.deps.json");
#else
            DriverResourcesDepsFile = Path.Combine(DriverResourcesProjectFolder, @"bin/Release/netcoreapp1.0/Evolve.Core.Test.Resources.SupportedDrivers.deps.json");
#endif
        }

        public static string ProjectFolder { get; }
        public static string DriverResourcesProjectFolder { get; }
        public static string DriverResourcesDepsFile { get; }
        public static string NugetPackageFolder => $@"{EnvHome}/.nuget/packages";
        public static string PgPassword => Environment.GetEnvironmentVariable("PGPASSWORD") ?? "Password12!";
#if DEBUG
        public static string MySqlPassword => "Password12!";
#else
        public static string MySqlPassword => Environment.GetEnvironmentVariable("MYSQL_PWD") ?? ""; // "" pour Travis CI
#endif
        public static string EnvHome => Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
    }
}
