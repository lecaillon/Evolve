using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Evolve.Connection;
using Evolve.Metadata;
using Evolve.Migration;

namespace Evolve.Tests
{
    internal static class TestUtil
    {
        [SuppressMessage("Security", "CA2100: Review SQL queries for security vulnerabilities")]
        public static void CreateSqlServerDatabase(string dbName, string cnxStr)
        {
            var cnn = new SqlConnection(cnxStr);
            cnn.Open();

            using (var cmd = cnn.CreateCommand())
            {
                cmd.CommandText = $"CREATE DATABASE {dbName};";
                cmd.ExecuteNonQuery();
            }

            cnn.Close();
        }

        public static WrappedConnection CreateSQLiteWrappedCnx() => new WrappedConnection(new SQLiteConnection("Data Source=:memory:"));

        public static FileMigrationScript BuildFileMigrationScript(string path = null, string version = null, string description = null) =>
            new FileMigrationScript(
                path: path ?? TestContext.CrLfScriptPath,
                version: version ?? "1",
                description: description ?? "desc",
                type: MetadataType.Migration);

        public static FileMigrationScript BuildRepeatableFileMigrationScript(string path = null, string description = null, string version = null) =>
            new FileMigrationScript(
                path: path ?? TestContext.CrLfScriptPath,
                version: version ?? null,
                description: description ?? "desc",
                type: MetadataType.RepeatableMigration);

        public static EmbeddedResourceMigrationScript BuildEmbeddedResourceMigrationScript(string version = null, string description = null, string name = null, Stream content = null) =>
            new EmbeddedResourceMigrationScript(
                version: version ?? "1",
                description: description ?? "desc",
                name: name ?? "name",
                content: content ?? new MemoryStream(Encoding.UTF8.GetBytes("content")),
                type: MetadataType.Migration);

        public static EmbeddedResourceMigrationScript BuildRepeatableEmbeddedResourceMigrationScript(string name = null, string description = null, Stream content = null) =>
            new EmbeddedResourceMigrationScript(
                version: null,
                description: description ?? "desc",
                name: name ?? "name",
                content: content ?? new MemoryStream(Encoding.UTF8.GetBytes("content")),
                type: MetadataType.RepeatableMigration);

        public static MigrationMetadata BuildMigrationMetadata(string version = null, string description = null, string name = null) =>
            new MigrationMetadata(
                version: version ?? "1",
                description: description ?? "desc",
                name: name ?? "name",
                type: MetadataType.Migration);
    }
}
