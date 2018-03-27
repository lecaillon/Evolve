using Evolve.Test.Utilities;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace Evolve.IntegrationTest.Cassandra
{
    public static class TestContext
    {
        static TestContext()
        {
            ResourcesFolder = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath), "Resources");
            CqlScriptsFolder = Path.Combine(ResourcesFolder, "Cql_Scripts");
            MigrationFolder = Path.Combine(CqlScriptsFolder, "Migration");
            ChecksumMismatchFolder = Path.Combine(CqlScriptsFolder, "Checksum_mismatch");
            EmptyMigrationScriptPath = Path.Combine(ResourcesFolder, "V1_3_2__Migration_description.cql");
        }

        public static string ResourcesFolder { get; }
        public static string CqlScriptsFolder { get; }
        public static string MigrationFolder { get; }
        public static string ChecksumMismatchFolder { get; }
        public static string EmptyMigrationScriptPath { get; }
        public static bool AppVeyor => Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        public static bool Travis => Environment.GetEnvironmentVariable("TRAVIS") == "True";

        [CollectionDefinition("Database collection")]
        public class DatabaseCollection : ICollectionFixture<CassandraFixture> { }
    }
}
