using System.IO;
using System.Reflection;

namespace Evolve.Test
{
    public static class TestContext
    {
        static TestContext()
        {
            ResourcesDirectory = Path.Combine(Path.GetDirectoryName(typeof(TestContext).GetTypeInfo().Assembly.Location), "Resources");
            ValidMigrationScriptPath = Path.Combine(ResourcesDirectory, "V1_3_1__Migration_description.sql");
            ChinookScriptPath = Path.Combine(ResourcesDirectory, "Chinook_Sqlite.sql");
        }

        public static string ResourcesDirectory { get; }

        public static string ValidMigrationScriptPath { get; }

        public static string ChinookScriptPath { get; }

        public static string SqlMigrationPrefix => "V";

        public static string SqlMigrationSeparator => "__";

        public static string SqlMigrationSuffix => ".sql";

        public static string SQLiteInMemoryConnectionString => "Data Source=:memory:";

        public static string DefaultMetadataTableName => "changelog";
    }
}
