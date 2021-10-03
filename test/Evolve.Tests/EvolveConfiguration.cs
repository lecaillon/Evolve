using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EvolveDb.Configuration;
using EvolveDb.Migration;

namespace EvolveDb.Tests
{
    internal class EvolveConfiguration : IEvolveConfiguration
    {
        public IEnumerable<string> Schemas { get; set; } = new List<string>();
        public CommandOptions Command { get; set; } = CommandOptions.DoNothing;
        public bool IsEraseDisabled { get; set; }
        public bool MustEraseOnValidationError { get; set; }
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public IEnumerable<string> Locations { get; set; } = new List<string> { "Sql_Scripts" };
        public string MetadataTableName { get; set; } = "changelog";

        private string _metadaTableSchema;
        public string MetadataTableSchema
        {
            get => _metadaTableSchema.IsNullOrWhiteSpace() ? Schemas.First() : _metadaTableSchema!;
            set => _metadaTableSchema = value;
        }
        public string PlaceholderPrefix { get; set; } = "${";
        public string PlaceholderSuffix { get; set; } = "}";
        public Dictionary<string, string> Placeholders { get; set; } = new Dictionary<string, string>();
        public string SqlMigrationPrefix { get; set; } = "V";
        public string SqlRepeatableMigrationPrefix { get; set; } = "R";
        public string SqlMigrationSeparator { get; set; } = "__";
        public string SqlMigrationSuffix { get; set; } = ".sql";
        public MigrationVersion TargetVersion { get; set; } = MigrationVersion.MaxVersion;
        public MigrationVersion StartVersion { get; set; } = MigrationVersion.MinVersion;
        public bool EnableClusterMode { get; set; } = true;
        public bool OutOfOrder { get; set; } = false;
        public int? CommandTimeout { get; set; }
        public IEnumerable<Assembly> EmbeddedResourceAssemblies { get; set; } = new List<Assembly>();
        public IEnumerable<string> EmbeddedResourceFilters { get; set; } = new List<string>();
        public bool RetryRepeatableMigrationsUntilNoError { get; set; }
        public TransactionKind TransactionMode { get; set; } = TransactionKind.CommitEach;
        public bool SkipNextMigrations { get; set; } = false;

        private IMigrationLoader _migrationLoader;
        public IMigrationLoader MigrationLoader
        {
            get
            {
                return _migrationLoader ?? (EmbeddedResourceAssemblies.Any()
                    ? new EmbeddedResourceMigrationLoader(this)
                    : new FileMigrationLoader(this));
            }
            set { _migrationLoader = value; }
        }
    }
}
