namespace Evolve.Cli
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Configuration;
    using Dialect;
    using McMaster.Extensions.CommandLineUtils;

    [Command(ResponseFileHandling = ResponseFileHandling.ParseArgsAsSpaceSeparated)]
    class Program
    {
        private static readonly Evolve Default = new Evolve(new System.Data.SQLite.SQLiteConnection("Data Source=:memory:"));

        static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [SuppressMessage("Design", "CA1031: Do not catch general exception types")]
        [SuppressMessage("Qualité du code", "IDE0051: Supprimer les membres privés non utilisés")]
        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            try
            {
                var evolve = EvolveFactory.Build(this, msg => console.WriteLine(msg));
                evolve.ExecuteCommand();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        [Argument(0, Description = "migrate | erase | repair | info")]
        [Required]
        [AllowedValues("migrate", "erase", "repair", "info", IgnoreCase = true)]
        public CommandOptions Command { get; }

        [Argument(1, Description = "postgresql | sqlite | sqlserver | mysql | mariadb | cassandra | cockroachdb")]
        [Required]
        [AllowedValues("postgresql", "sqlite", "sqlserver", "mysql", "mariadb", "cassandra", "cockroachdb", IgnoreCase = true)]
        public DBMS Database { get; }

        [Option("-c|--connection-string", "The connection string to the target database engine. Must have the necessary privileges to execute ddl.", CommandOptionType.SingleValue)]
        [Required]
        public string ConnectionString { get; }

        [Option("-l|--location", "Paths to scan recursively for migration scripts. Default: Sql_Scripts", CommandOptionType.MultipleValue)]
        public string[] Locations { get; } = Default.Locations?.ToArray();

        [Option("-s|--schema", "A list of schemas managed by Evolve. If empty, the default schema for the datasource connection is used.", CommandOptionType.MultipleValue)]
        public string[] Schemas { get; } = Default.Schemas?.ToArray();

        [Option("--metadata-table-schema", "The schema in which the metadata table is/should be. If empty, it is the first schema defined or the one of the datasource connection.", CommandOptionType.SingleValue)]
        public string MetadataTableSchema { get; }

        [Option("--metadata-table", "The name of the metadata table. Default: changelog", CommandOptionType.SingleValue)]
        public string MetadataTableName { get; } = Default.MetadataTableName;

        [Option("-p|--placeholder", "Placeholders are strings to replace in migration scripts. Format for commandline is \"key:value\".", CommandOptionType.MultipleValue)]
        public string[] Placeholders { get; }

        [Option("--placeholder-prefix", "The prefix of the placeholders. Default: ${", CommandOptionType.SingleValue)]
        public string PlaceholderPrefix { get; } = Default.PlaceholderPrefix;

        [Option("--placeholder-suffix", "The suffix of the placeholders. Default: }", CommandOptionType.SingleValue)]
        public string PlaceholderSuffix { get; } = Default.PlaceholderSuffix;

        [Option("--target-version", "Target version to reach. If empty it evolves all the way up.", CommandOptionType.SingleValue)]
        public string TargetVersion { get; }

        [Option("--start-version", "Version used as starting point for already existing databases. Default: 0", CommandOptionType.SingleValue)]
        public string StartVersion { get; }

        [Option("--scripts-prefix", "Migration scripts file names prefix. Default: V", CommandOptionType.SingleValue)]
        public string ScriptsPrefix { get; } = Default.SqlMigrationPrefix;

        [Option("--repeatable-scripts-prefix", "Repeatable migration scripts file names prefix. Default: R", CommandOptionType.SingleValue)]
        public string RepeatableScriptsPrefix { get; } = Default.SqlRepeatableMigrationPrefix;

        [Option("--scripts-suffix", "Migration scripts files extension. Default: .sql", CommandOptionType.SingleValue)]
        public string ScriptsSuffix { get; } = Default.SqlMigrationSuffix;

        [Option("--scripts-separator", "Migration scripts file names separator. Default: __", CommandOptionType.SingleValue)]
        public string ScriptsSeparator { get; } = Default.SqlMigrationSeparator;

        [Option("--encoding", "The encoding of migration scripts. Default: UTF-8", CommandOptionType.SingleValue)]
        public string Encoding { get; } = "UTF-8";

        [Option("--command-timeout", "The wait time in seconds before terminating the attempt to execute a migration and generating an error. Default: 30", CommandOptionType.SingleValue)]
        public int? CommandTimeout { get; } = Default.CommandTimeout;

        [Option("--out-of-order", "Allows migration scripts to be run “out of order”. Default: false", CommandOptionType.SingleValue)]
        public bool OutOfOrder { get; } = Default.OutOfOrder;

        [Option("--erase-disabled", "When set, ensures that Evolve will never erase schemas. Highly recommended in production. Default: false", CommandOptionType.SingleValue)]
        public bool EraseDisabled { get; } = Default.IsEraseDisabled;

        [Option("--erase-on-validation-error", "When set, Evolve will erase the database schemas and will re-execute migration scripts from scratch if validation phase fails. Intended to be used in development only. Default: fasle", CommandOptionType.SingleValue)]
        public bool EraseOnValidationError { get; } = Default.MustEraseOnValidationError;

        [Option("--enable-cluster-mode", "By default, Evolve will use a session level lock to coordinate the migration on multiple nodes. Default: true", CommandOptionType.SingleValue)]
        public bool EnableClusterMode { get; } = Default.EnableClusterMode;

        [Option("-a|--embedded-resource-assembly", "When set, Evolve will scan the given list of assembly to load embedded migration scripts.", CommandOptionType.MultipleValue)]
        public string[] EmbeddedResourceLocations { get; }

        [Option("-f|--embedded-resource-filter", "When set, exclude embedded migration scripts that do not start with one of these filters.", CommandOptionType.MultipleValue)]
        public string[] EmbeddedResourceFilters { get; }
     
        [Option("--retry-repeatable", "When set, execute repeatedly all repeatable migrations for as long as the number of errors decreases, so that you can name them more easily. Default: false", CommandOptionType.SingleValue)]
        public bool RetryRepeatableMigrationsUntilNoError { get; } = Default.RetryRepeatableMigrationsUntilNoError;

        [Option("--transaction-mode", "Scope of the Evolve transaction. Default: CommitEach", CommandOptionType.SingleValue)]
        [AllowedValues("CommitEach", "CommitAll", "RollbackAll", IgnoreCase = true)]
        public TransactionKind TransactionMode { get; } = Default.TransactionMode;

        [Option("--skip-next-migrations", "When set, mark all subsequent migrations as applied. Default: false", CommandOptionType.SingleValue)]
        public bool SkipNextMigrations { get; }

        // Cassandra
        [Option("--keyspace", "A list of keyspaces managed by Evolve (Cassandra only).", CommandOptionType.MultipleValue)]
        public string[] Keyspaces { get; }

        [Option("--metadata-table-keyspace", "The keyspace in which the metadata table is/should be (Cassandra only).", CommandOptionType.SingleValue)]
        public string MetadataTableKeyspace { get; }
    }
}
