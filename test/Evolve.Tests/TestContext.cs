using System;
using System.IO;
using System.Reflection;
using Evolve.Metadata;
using Evolve.Migration;
using Evolve.Tests.Infrastructure;
using Xunit;

namespace Evolve.Tests
{
    public static class TestContext
    {
        static TestContext()
        {
            // Used in evolve.json configuration tests
            Environment.SetEnvironmentVariable("EVOLVE_HOST", "127.0.0.1");
            Environment.SetEnvironmentVariable("EVOLVE_DB_USER", "myUsername");
            Environment.SetEnvironmentVariable("EVOLVE_DB_PWD", "myPassword");
        }

        public static string ProjectFolder => Path.GetDirectoryName(typeof(TestContext).GetTypeInfo().Assembly.Location);
        public static bool AppVeyor => Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        public static bool AzureDevOps => Environment.GetEnvironmentVariable("TF_BUILD") == "True";
        public static bool Local => !AppVeyor && !AzureDevOps;
        public static string DistFolder => Path.GetFullPath(Path.Combine(ProjectFolder, "../../../../../dist"));
        public static string CliExe => Path.Combine(DistFolder, "evolve.exe");
        public static string Cli => Path.Combine(DistFolder, "evolve");
        public static string ResourcesFolder => Path.Combine(ProjectFolder, "Resources");
        public static FileMigrationScript FileMigrationScriptV = new FileMigrationScript(Path.Combine(ResourcesFolder, "V2_3_1__Duplicate_migration_script.sql"), "2_3_1", "Duplicate migration script", MetadataType.Migration);
        public static FileMigrationScript FileMigrationScriptR = new FileMigrationScript(Path.Combine(ResourcesFolder, "R__desc_b.sql"), version: null, "desc b", MetadataType.RepeatableMigration);
        public static string CrLfScriptPath => Path.Combine(ResourcesFolder, "LF_CRLF/V2_3_1__Migration_description.sql");
        public static string LfScriptPath => Path.Combine(ResourcesFolder, "LF_CRLF/V2_3_2__Migration_description_lf.sql");
        public static string Scripts1 => Path.Combine(ResourcesFolder, "Scripts_1");
        public static string Scripts2 => Path.Combine(ResourcesFolder, "Scripts_2");
        public static string EvolveJsonPath => Path.Combine(ResourcesFolder, "Configuration/evolve.json");
        public static string Evolve2JsonPath => Path.Combine(ResourcesFolder, "Configuration/evolve2.json");
        public static string Evolve3JsonPath => Path.Combine(ResourcesFolder, "Configuration/evolve3.json");
        public static string EvolveAppConfigPath => Path.Combine(ResourcesFolder, "Configuration/App.config");
        public static string EvolveWebConfigPath => Path.Combine(ResourcesFolder, "Configuration/Web.config");

        public static class Cassandra
        {
            public static string ResourcesFolder => Path.Combine(ProjectFolder, "Integration/Cassandra/Resources");
            public static string SqlScriptsFolder => Path.Combine(ResourcesFolder, "Cql_Scripts");
            public static string MigrationFolder => Path.Combine(SqlScriptsFolder, "Migration");
            public static string ChecksumMismatchFolder => Path.Combine(SqlScriptsFolder, "Checksum_mismatch");
        }

        public static class MySQL
        {
            public static string ResourcesFolder => Path.Combine(ProjectFolder, "Integration/MySQL/Resources");
            public static string SqlScriptsFolder => Path.Combine(ResourcesFolder, "Sql_Scripts");
            public static string MigrationFolder => Path.Combine(SqlScriptsFolder, "Migration");
            public static string MigrationFolderFilter => "Evolve.Tests.Integration.MySQL.Resources.Sql_Scripts.Migration";
            public static string ChecksumMismatchFolder => Path.Combine(SqlScriptsFolder, "Checksum_mismatch");
            public static string RepeatableFolder => Path.Combine(SqlScriptsFolder, "Repeatable");
        }

        public static class PostgreSQL
        {
            public static string ResourcesFolder => Path.Combine(ProjectFolder, "Integration/PostgreSQL/Resources");
            public static string SqlScriptsFolder => Path.Combine(ResourcesFolder, "Sql_Scripts");
            public static string MigrationFolder => Path.Combine(SqlScriptsFolder, "Migration");
            public static string Migration11Folder => Path.Combine(SqlScriptsFolder, "Migration11"); // PostgreSQL 11
            public static string ChecksumMismatchFolder => Path.Combine(SqlScriptsFolder, "Checksum_mismatch");
            public static string RepeatableFolder => Path.Combine(SqlScriptsFolder, "Repeatable");
            public static string OutOfOrderFolder => Path.Combine(SqlScriptsFolder, "OutOfOrder");
        }

        public static class SQLite
        {
            public static string ResourcesFolder => Path.Combine(ProjectFolder, "Integration/SQLite/Resources");
            public static string SqlScriptsFolder => Path.Combine(ResourcesFolder, "Sql_Scripts");
            public static string MigrationFolder => Path.Combine(SqlScriptsFolder, "Migration");
            public static string ChecksumMismatchFolder => Path.Combine(SqlScriptsFolder, "Checksum_mismatch");
            public static string RepeatableFolder => Path.Combine(SqlScriptsFolder, "Repeatable");
            public static string ChinookScriptPath => Path.Combine(SqlScriptsFolder, "Chinook_Sqlite.sql");
            public static string ChinookScript => File.ReadAllText(ChinookScriptPath);
        }

        public static class SqlServer
        {
            public static string ResourcesFolder => Path.Combine(ProjectFolder, "Integration/SQLServer/Resources");
            public static string SqlScriptsFolder => Path.Combine(ResourcesFolder, "Sql_Scripts");
            public static string MigrationFolder => Path.Combine(SqlScriptsFolder, "Migration");
            public static string ChecksumMismatchFolder => Path.Combine(SqlScriptsFolder, "Checksum_mismatch");
        }
    }

    [CollectionDefinition("Cassandra collection")]
    public class CassandraCollection : IClassFixture<CassandraFixture> { }

    [CollectionDefinition("MySQL collection")]
    public class MySQLCollection : IClassFixture<MySQLFixture> { }

    [CollectionDefinition("PostgreSql collection")]
    public class PostgreSqlCollection : IClassFixture<PostgreSqlFixture> { }

    [CollectionDefinition("SQLServer collection")]
    public class SQLServerCollection : IClassFixture<SQLServerFixture> { }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : IClassFixture<MySQLFixture>,
                                      IClassFixture<PostgreSqlFixture>,
                                      IClassFixture<SQLServerFixture>,
                                      IClassFixture<CassandraFixture> { }
}
