using System;
using System.IO;
using System.Reflection;

namespace Evolve.Test
{
    public static class TestContext
    {
        static TestContext()
        {
            ResourcesDirectory = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath), "Resources");
            AppConfigPath = Path.Combine(ResourcesDirectory, "App.config");
        }

        public static string ResourcesDirectory { get; }

        public static string AppConfigPath { get; }
    }
}
