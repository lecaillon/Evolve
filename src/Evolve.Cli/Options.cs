using System.Collections.Generic;
using CommandLine;

namespace Evolve.Cli
{
    internal abstract class Options
    {
        public abstract string Driver { get; }

        [Value(0, MetaName = "command", Required = true, HelpText = "migrate | erase | repair")]
        public Command Command { get; set; }

        [Option('c', "connection-string", Required = true, HelpText = "The connection string to the target database engine. Must have the necessary privileges to execute ddl.")]
        public string ConnectionString { get; set; }

        [Option('p', "driver-assembly-path", Required = true, HelpText = "Path to the application folder to migrate or to any folder that contains database driver assemblies.")]
        public string DriverAssemblyPath { get; set; }

        [Option('l', "locations", Default = new[] { "Sql_Scripts" }, HelpText = "Paths to scan recursively for migration scripts.")]
        public IEnumerable<string> Locations { get; set; }

        [Option('t', "metadata-table", Default = "changelog", HelpText = "The name of the metadata table.")]
        public string MetadataTableName { get; set; }

        [Option('v', "target-version", Required = false, HelpText = "Target version to reach. If empty it evolves all the way up.")]
        public string TargetVersion { get; set; }

        [Option("start-version", Default = "0", HelpText = "Version used as starting point for already existing databases.")]
        public string StartVersion { get; set; }

        [Option("out-of-order", Default = false, HelpText = "Allows migration scripts to be run “out of order”. If you already have versions 1 and 3 applied, and now a version 2 is found, it will be applied too instead of being ignored.")]
        public bool OutOfOrder { get; set; }

        [Option("erase-disabled", Default = false, HelpText = "When set, ensures that Evolve will never erase schemas. Highly recommended in production.")]
        public bool EraseDisabled { get; set; }

        [Option("erase-on-validation-error", Default = false, HelpText = "When set, if validation phase fails, Evolve will erase the database schemas and will re-execute migration scripts from scratch. Intended to be used in development only.")]
        public bool EraseOnValidationError { get; set; }

        [Option("command-timeout", Default = 30, HelpText = "The wait time in seconds before terminating the attempt to execute a migration and generating an error.")]
        public int CommandTimeout { get; set; }

        [Option("encoding", Default = "UTF-8", HelpText = "The encoding of migration scripts.")]
        public string Encoding { get; set; }

        [Option("scripts-prefix", Default = "V", HelpText = "Migration scripts file names prefix")]
        public string MigrationScriptsPrefix { get; set; }

        [Option("scripts-separator", Default = "__", HelpText = "Migration scripts file names separator")]
        public string MigrationScriptsSeparator { get; set; }

        [Option("scripts-suffix", Default = ".sql", HelpText = "Migration scripts files extension.")]
        public string ScriptsSuffix { get; set; }

        [Option("placeholder-prefix", Default = "${", HelpText = "The prefix of the placeholders.")]
        public string PlaceholderPrefix { get; set; }

        [Option("placeholder-suffix", Default = "}", HelpText = "The suffix of the placeholders.")]
        public string PlaceholderSuffix { get; set; }

        [Option("placeholders", Required = false, HelpText = "Placeholders are strings to replace in migration scripts. Format for commandline is \"key:value\".")]
        public IEnumerable<string> Placeholders { get; set; }

        [Option("disable-cluster-mode", Default = false, HelpText = "By default, Evolve will use a session level lock to coordinate the migration on multiple nodes. This prevents two distinct Evolve executions from executing an Evolve command on the same database at the same time. If this flag is set, it will not be the case.")]
        public bool DisableClusterMode { get; set; }
    }
}
