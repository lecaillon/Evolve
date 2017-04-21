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
            NugetPackageFolder = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%/.nuget/packages");
#if DEBUG
            DriverResourcesDepsFile = Path.Combine(DriverResourcesProjectFolder, @"bin/Debug/netcoreapp1.1/Evolve.Core.Test.Resources.SupportedDrivers.deps.json");
#else
            DriverResourcesDepsFile = Path.Combine(DriverResourcesProjectFolder, @"bin/Release/netcoreapp1.1/Evolve.Core.Test.Resources.SupportedDrivers.deps.json");
#endif
        }

        public static string ProjectFolder { get; }
        public static string DriverResourcesProjectFolder { get; }
        public static string DriverResourcesDepsFile { get; }
        public static string NugetPackageFolder { get; }
        public static string PgPassword => Environment.GetEnvironmentVariable("PGPASSWORD") ?? "Password12!";
    }
}
