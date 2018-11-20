namespace Evolve.Cli
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Configuration;
    using Dialect;
    using McMaster.Extensions.CommandLineUtils;

    class Program
    {
        private static readonly Evolve Default = new Evolve(null);

        static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

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
                Console.WriteLine(ex);
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        [Argument(0, Description = "postgresql | sqlite | sqlserver | mysql | mariadb | cassandra")]
        [Required]
        [AllowedValues("postgresql", "sqlite", "sqlserver", "mysql", "mariadb", "cassandra", IgnoreCase = true)]
        public DBMS Database { get; }

        [Argument(1, Description = "migrate | erase | repair")]
        [Required]
        [AllowedValues("migrate", "erase", "repair", IgnoreCase = true)]
        public CommandOptions Command { get; }

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

        // Cassandra
        [Option("--keyspace", "A list of keyspaces managed by Evolve (Cassandra only).", CommandOptionType.MultipleValue)]
        public string[] Keyspaces { get; }

        [Option("--metadata-table-keyspace", "The keyspace in which the metadata table is/should be (Cassandra only).", CommandOptionType.SingleValue)]
        public string MetadataTableKeyspace { get; }
    }
}

// postgresql erase --connection-string="Server=127.0.0.1;Port=5432;Database=my_database;User Id=postgres;Password=Password12!;" --schema=public --schema=unittest --location="C:\Projets\Evolve\test\Evolve.IntegrationTest\PostgreSQL\Resources\Sql_Scripts\Migration" --placeholder schema1:unittest
// sqlite erase --connection-string="Data Source=C:\Users\lecai\Downloads\my_database.db" --location="C:\Projets\Evolve\test\Evolve.IntegrationTest\SQLite\Resources\Sql_Scripts\Migration" --placeholder table4:table_4
// mysql erase --connection-string="Server=127.0.0.1;Port=3306;Database=my_database;Uid=root;Pwd=Password12!;SslMode=none;" --location="C:\Projets\Evolve\test\Evolve.IntegrationTest\MySQL\Resources\Sql_Scripts\Migration"
// sqlserver erase --connection-string="Server=127.0.0.1;Database=my_database_2;User Id=sa;Password=Password12!;" --location="C:\Projets\Evolve\test\Evolve.IntegrationTest\SQLServer\Resources\Sql_Scripts\Migration" --placeholder db:my_database_2 --placeholder schema2:dbo --target-version="8_9"
// cassandra erase --connection-string="Contact Points=127.0.0.1;Port=9042;Cluster Name=evolve" --location="C:\Projets\Evolve\test\Evolve.IntegrationTest\Cassandra\Resources\Cql_Scripts\Migration" --scripts-suffix=".cql" --keyspace="my_keyspace" --metadata-table-keyspace="evolve_change_log"
