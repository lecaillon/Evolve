﻿using System;
using System.IO;
using System.Reflection;

namespace Evolve.Test
{
    public static class TestContext
    {
        static TestContext()
        {
            ProjectFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(new Uri(typeof(TestContext).GetTypeInfo().Assembly.CodeBase).AbsolutePath), @"../../"));
            DriverResourcesProjectFolder = Path.GetFullPath(Path.Combine(ProjectFolder, @"../Evolve.Core.Test.Resources.SupportedDrivers"));
#if DEBUG
            DriverResourcesDepsFile = Path.Combine(DriverResourcesProjectFolder, @"bin/Debug/netcoreapp1.1/Evolve.Core.Test.Resources.SupportedDrivers.deps.json");
#else
            DriverResourcesDepsFile = Path.Combine(DriverResourcesProjectFolder, @"bin/Release/netcoreapp1.1/Evolve.Core.Test.Resources.SupportedDrivers.deps.json");
#endif
            ResourcesFolder = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath), "Resources");
            AppConfigPath = Path.Combine(ResourcesFolder, "App.config");
            WebConfigPath = Path.Combine(ResourcesFolder, "Web.config");
        }

        public static string ProjectFolder { get; }
        public static string DriverResourcesProjectFolder { get; }
        public static string DriverResourcesDepsFile { get; }
        public static string NugetPackageFolder => $@"{EnvHome}/.nuget/packages";
        public static string PgPassword => Environment.GetEnvironmentVariable("PGPASSWORD") ?? "Password12!";
        public static string EnvHome => Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
        public static string ResourcesFolder { get; }
        public static string AppConfigPath { get; }
        public static string WebConfigPath { get; }
    }
}
