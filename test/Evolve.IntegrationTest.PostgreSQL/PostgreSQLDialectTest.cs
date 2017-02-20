using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Threading;
using Evolve.Connection;
using Evolve.Dialect;
using Evolve.Dialect.PostgreSQL;
using Evolve.Migration;
using Npgsql;
using Xunit;

namespace Evolve.IntegrationTest.PostgreSQL
{
    public class PostgreSQLDialectTest : IDisposable
    {
        #region Test config

        public const string ImageName = "postgres:latest";
        public const string ContainerName = "postgres-evolve";
        public const string ContainerPort = "5432";
        public const string DbName = "my_database";
        public const string DbPwd = "postgres";
        public const string DbUser = "postgres";

        public static string ResourcesDirectory = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath), "Resources");
        public static string MigrationScriptPath = Path.Combine(ResourcesDirectory, "V1_3_2__Migration_description.sql");

        #endregion

        [Fact(DisplayName = "Run_all_PostgreSQL_integration_tests_work")]
        public void Run_all_PostgreSQL_integration_tests_work()
        {
            // Open a connection to the PostgreSQL database
            var cnn = new NpgsqlConnection($"Server=127.0.0.1;Port={ContainerPort};Database={DbName};User Id={DbUser};Password={DbPwd};");
            cnn.Open();
            Assert.True(cnn.State == ConnectionState.Open, "Cannot open a connection to the database.");

            // Init the DatabaseHelper
            var wcnn = new WrappedConnection(cnn);
            DatabaseHelper db = new PostgreSQLDatabase(wcnn);
            Assert.True(db.GetCurrentSchemaName() == "public", "The default PostgreSQL schema should be 'public'.");

            // Create schema
            string metadataSchemaName = "My metadata schema";
            Schema metadataSchema = new PostgreSQLSchema(metadataSchemaName, wcnn);
            Assert.False(metadataSchema.IsExists(), $"The schema [{metadataSchemaName}] should not already exist.");
            Assert.True(metadataSchema.Create(), $"Creation of the schema [{metadataSchemaName}] failed.");
            Assert.True(metadataSchema.IsExists(), $"The schema [{metadataSchemaName}] should be created.");
            Assert.True(metadataSchema.IsEmpty(), $"The schema [{metadataSchemaName}] should be empty.");

            // Change current schema
            db.ChangeSchema(metadataSchemaName);
            Assert.True(db.GetCurrentSchemaName() == metadataSchemaName, $"[{metadataSchemaName}] should be the current schema.");

            // Get MetadataTable
            string metadataTableName = "changelog";
            var metadata = db.GetMetadataTable(metadataSchemaName, metadataTableName);

            // Create MetadataTable
            Assert.True(metadata.CreateIfNotExists(), "MetadataTable creation failed.");
            Assert.False(metadata.CreateIfNotExists(), "MetadataTable already exists. Creation should return false.");
            Assert.True(metadata.GetAllMigrationMetadata().Count() == 0, "No migration metadata should be found.");

            // Lock MetadataTable
            metadata.Lock();

            // Add metadata migration
            var migrationScript = new MigrationScript(MigrationScriptPath, "1.3.2", "Migration_description");
            metadata.SaveMigration(migrationScript, true);
            Assert.True(metadata.GetAllMigrationMetadata().Count() == 1, "One migration metadata should be found.");

            // compléter le test : comparer en détail migrationScript et metadata.GetAllMigrationMetadata()
            // ajouter la méthode MetadataTable.AddSchemaMarker()

            // Assert metadata schema is not empty
            Assert.False(metadataSchema.IsEmpty(), $"[{metadataSchemaName}] should not be empty.");

            // Clean schema
            metadataSchema.Clean();
            Assert.True(metadataSchema.IsEmpty(), $"The schema [{metadataSchemaName}] should be empty.");
            Assert.True(metadataSchema.IsExists(), $"The schema [{metadataSchemaName}] should exist.");

            // Drop schema
            metadataSchema.Drop();
            Assert.False(metadataSchema.IsExists(), $"The schema [{metadataSchemaName}] should not exist.");
        }

        /// <summary>
        ///     Start PostgreSQL server.
        /// </summary>
        public PostgreSQLDialectTest()
        {
            using (var ps = PowerShell.Create())
            {
                ps.Commands.AddScript($"docker run --name {ContainerName} --publish={ContainerPort}:{ContainerPort} -e POSTGRES_PASSWORD={DbPwd} -e POSTGRES_DB={DbName} -d {ImageName}");
                ps.Invoke();
            }

            Thread.Sleep(5000);
        }

        /// <summary>
        ///     Stop PostgreSQL server and remove container.
        /// </summary>
        public void Dispose()
        {
            using (var ps = PowerShell.Create())
            {
                ps.Commands.AddScript($"docker stop '{ContainerName}'");
                ps.Commands.AddScript($"docker rm '{ContainerName}'");
                ps.Invoke();
            }
        }
    }
}
