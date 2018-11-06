namespace Evolve.Cli
{
    using System.ComponentModel.DataAnnotations;
    using Configuration;
    using Dialect;
    using McMaster.Extensions.CommandLineUtils;

    class Program
    {
        static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            var evolve = EvolveFactory.Build(this, msg => console.WriteLine(msg));
            evolve.ExecuteCommand();

            return 1;
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

        [Option("-l|--location", "Paths to scan recursively for migration scripts.", CommandOptionType.MultipleValue)]
        public string[] Locations { get; } = new[] { "Sql_Scripts" };

        [Option("-s|--schema", "A list of schemas managed by Evolve. If empty, the default schema for the datasource connection is used.", CommandOptionType.MultipleValue)]
        public string[] Schemas { get; }

        [Option("--metadata-table-schema", "The schema in which the metadata table is/should be", CommandOptionType.SingleValue)]
        public string MetadataTableSchema { get; }

        [Option("--metadata-table", "The name of the metadata table.", CommandOptionType.SingleValue)]
        public string MetadataTableName { get; } = "changelog";

        [Option("-p|--placeholder", "Placeholders are strings to replace in migration scripts. Format for commandline is \"key:value\".", CommandOptionType.MultipleValue)]
        public string[] Placeholders { get; }

        [Option("--target-version", "Target version to reach. If empty it evolves all the way up.", CommandOptionType.SingleValue)]
        public string TargetVersion { get; }

        [Option("--scripts-suffix", "Migration scripts files extension.", CommandOptionType.SingleValue)]
        public string ScriptsSuffix { get; } = ".sql";

        // Cassandra
        [Option("--keyspace", "A list of keyspaces managed by Evolve (Cassandra only).", CommandOptionType.MultipleValue)]
        public string[] Keyspaces { get; }

        [Option("--metadata-table-keyspace", "The keyspace in which the metadata table is/should be (Cassandra only).", CommandOptionType.SingleValue)]
        public string MetadataTableKeyspace { get; }
    }
}

// dotnet publish -c Release -r win-x64
// dotnet publish -c Release -r linux-x64
// warp-packer --arch windows-x64 --exec Evolve.Cli.exe --output evolve.exe --input_dir .

// postgresql erase --connection-string="Server=127.0.0.1;Port=5432;Database=my_database;User Id=postgres;Password=Password12!;" --schema=public --schema=unittest --location="C:\Projets\Evolve\test\Evolve.IntegrationTest\PostgreSQL\Resources\Sql_Scripts\Migration" --placeholder schema1:unittest
// sqlite erase --connection-string="Data Source=C:\Users\lecai\Downloads\my_database.db" --location="C:\Projets\Evolve\test\Evolve.IntegrationTest\SQLite\Resources\Sql_Scripts\Migration" --placeholder table4:table_4
// mysql erase --connection-string="Server=127.0.0.1;Port=3306;Database=my_database;Uid=root;Pwd=Password12!;SslMode=none;" --location="C:\Projets\Evolve\test\Evolve.IntegrationTest\MySQL\Resources\Sql_Scripts\Migration"
// sqlserver erase --connection-string="Server=127.0.0.1;Database=my_database_2;User Id=sa;Password=Password12!;" --location="C:\Projets\Evolve\test\Evolve.IntegrationTest\SQLServer\Resources\Sql_Scripts\Migration" --placeholder db:my_database_2 --placeholder schema2:dbo --target-version="8_9"
// cassandra erase --connection-string="Contact Points=127.0.0.1;Port=9042;Cluster Name=evolve" --location="C:\Projets\Evolve\test\Evolve.IntegrationTest\Cassandra\Resources\Cql_Scripts\Migration" --scripts-suffix=".cql" --keyspace="my_keyspace" --metadata-table-keyspace="evolve_change_log"
