using System.IO;
using System.Reflection;

namespace Evolve.Test
{
    public static class TestContext
    {
        static TestContext()
        {
            ResourcesDirectory = Path.Combine(Path.GetDirectoryName(typeof(TestContext).GetTypeInfo().Assembly.Location), "Resources");
            ValidMigrationScriptPath = Path.Combine(ResourcesDirectory, "V1_3_1__Migration-description.sql");
        }

        public static string ResourcesDirectory { get; private set; }

        public static string ValidMigrationScriptPath { get; private set; }
    }
}
