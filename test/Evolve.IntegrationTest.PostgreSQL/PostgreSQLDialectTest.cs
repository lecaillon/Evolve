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
using Evolve.Metadata;
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

            // Initiate a connection to the database
            var wcnn = new WrappedConnection(cnn);

            // Validate DBMS.PostgreSQL
            Assert.Equal(DBMS.PostgreSQL, wcnn.GetDatabaseServerType());

            // Init the DatabaseHelper
            DatabaseHelper db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.PostgreSQL, wcnn);
            Assert.True(db.GetCurrentSchemaName() == "public", "The default PostgreSQL schema should be 'public'.");

            // Create schema
            string metadataSchemaName = "My metadata schema";
            Schema metadataSchema = new PostgreSQLSchema(metadataSchemaName, wcnn);
            Assert.False(metadataSchema.IsExists(), $"The schema [{metadataSchemaName}] should not already exist.");
            Assert.True(metadataSchema.Create(), $"Creation of the schema [{metadataSchemaName}] failed.");
            Assert.True(metadataSchema.IsExists(), $"The schema [{metadataSchemaName}] should be created.");
            Assert.True(metadataSchema.IsEmpty(), $"The schema [{metadataSchemaName}] should be empty.");

            // Get MetadataTable
            string metadataTableName = "changelog";
            var metadata = db.GetMetadataTable(metadataSchemaName, metadataTableName);

            // Create MetadataTable
            Assert.False(metadata.IsExists(), "MetadataTable sould not already exist.");
            Assert.True(metadata.CreateIfNotExists(), "MetadataTable creation failed.");
            Assert.True(metadata.IsExists(), "MetadataTable sould exist.");
            Assert.False(metadata.CreateIfNotExists(), "MetadataTable already exists. Creation should return false.");
            Assert.True(metadata.GetAllMigrationMetadata().Count() == 0, "No migration metadata should be found.");

            // Lock MetadataTable
            metadata.Lock();

            // Save NewSchema metadata
            metadata.Save(MetadataType.NewSchema, "0", "New schema created.", metadataSchemaName);
            Assert.True(metadata.CanDropSchema(metadataSchemaName), $"[{metadataSchemaName}] should be droppable.");
            Assert.False(metadata.CanEraseSchema(metadataSchemaName), $"[{metadataSchemaName}] should not be erasable.");

            // Add metadata migration
            var migrationScript = new MigrationScript(MigrationScriptPath, "1_3_2", "Migration_description");
            metadata.SaveMigration(migrationScript, true);
            var migrationMetadata = metadata.GetAllMigrationMetadata().FirstOrDefault();
            Assert.True(migrationMetadata != null, "One migration metadata should be found.");
            Assert.True(migrationMetadata.Version == migrationScript.Version, "Metadata version is not the same.");
            Assert.True(migrationMetadata.Checksum == migrationScript.CalculateChecksum(), "Metadata checksum is not the same.");
            Assert.True(migrationMetadata.Description == migrationScript.Description, "Metadata descritpion is not the same.");
            Assert.True(migrationMetadata.Name == migrationScript.Name, "Metadata name is not the same.");
            Assert.True(migrationMetadata.Success == true, "Metadata success is not true.");
            Assert.True(migrationMetadata.Id > 0, "Metadata id is not set.");
            Assert.True(migrationMetadata.InstalledOn.Date == DateTime.Now.Date, "Installed date is not set.");

            // Assert metadata schema is not empty
            Assert.False(metadataSchema.IsEmpty(), $"[{metadataSchemaName}] should not be empty.");

            // Erase schema
            metadataSchema.Erase();
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
