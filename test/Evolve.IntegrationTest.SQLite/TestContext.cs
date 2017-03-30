using System;
using System.IO;
using System.Reflection;

namespace Evolve.IntegrationTest.SQLite
{
    public static class TestContext
    {
        static TestContext()
        {
            ResourcesFolder = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath), "Resources");
            SqlScriptsFolder = Path.Combine(ResourcesFolder, "Sql_Scripts");
            MigrationFolder = Path.Combine(SqlScriptsFolder, "Migration");
            ChecksumMismatchFolder = Path.Combine(SqlScriptsFolder, "Checksum_mismatch");
        }

        public static string ResourcesFolder { get; }
        public static string SqlScriptsFolder { get; }
        public static string MigrationFolder { get; }
        public static string ChecksumMismatchFolder { get; }
    }
}
