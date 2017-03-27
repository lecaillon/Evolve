using System;
using System.IO;
using System.Reflection;

namespace Evolve.Test
{
    public static class TestContext
    {
        static TestContext()
        {
            ResourcesFolder = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath), "Resources");
            AppConfigPath = Path.Combine(ResourcesFolder, "App.config");
            WebConfigPath = Path.Combine(ResourcesFolder, "Web.config");
        }

        public static string ResourcesFolder { get; }
        public static string AppConfigPath { get; }
        public static string WebConfigPath { get; }
    }
}
