using System.IO;
using System.Reflection;

namespace Evolve.Core.Test
{
    public static class TestContext
    {
        static TestContext()
        {
            ResourcesFolder = Path.Combine(Path.GetDirectoryName(typeof(TestContext).GetTypeInfo().Assembly.Location), "Resources");
            ScriptsSQL1 = Path.Combine(ResourcesFolder, "Scripts_SQL_1");
            ScriptsSQL2 = Path.Combine(ResourcesFolder, "Scripts_SQL_2");
            ValidMigrationScriptPath = Path.Combine(ResourcesFolder, "V1_3_1__Migration_description.sql");
            ChinookScriptPath = Path.Combine(ResourcesFolder, "Chinook_Sqlite.sql");
        }

        public static string ResourcesFolder { get; }

        public static string ScriptsSQL1 { get; }

        public static string ScriptsSQL2 { get; }

        public static string ValidMigrationScriptPath { get; }

        public static string ChinookScriptPath { get; }

        public static string SqlMigrationPrefix => "V";

        public static string SqlMigrationSeparator => "__";

        public static string SqlMigrationSuffix => ".sql";

        public static string SQLiteInMemoryConnectionString => "Data Source=:memory:";

        public static string DefaultMetadataTableName => "changelog";
    }
}
