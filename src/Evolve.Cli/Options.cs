using CommandLine;
using System.Collections.Generic;

namespace Evolve.Cli
{
    internal abstract class Options
    {
        public abstract string Driver { get; }

        [Value(0, MetaName = "command", HelpText = "migrate | erase | repair", Required = true)]
        public Command Command { get; set; }

        [Value(1, MetaName = "connection-string", HelpText = "The connection string to the target engine.", Required = true)]
        public string ConnectionString { get; set; }

        [Option('l', "scripts-locations", HelpText = "Paths to scan recursively for migration scripts.", Default = new[] { "Sql_Scripts" })]
        public IEnumerable<string> Locations { get; set; }

        [Option("timeout", HelpText = "The timetout in seconds for Evolve execution.", Default = 30)]
        public int Timeout { get; set; }

        [Option('t', "metadata-table", HelpText = "The name of the metadata table.", Default = "evolve_change_log")]
        public string MetadataTableName { get; set; }

        [Option("enable-erase", HelpText = "Allows Evolve to erase schema and tables. Intended to be used in development only.", Default = false)]
        public bool EnableErase { get; set; }

        [Option("erase-on-validation-error", HelpText = "When set, if validation phase fails, Evolve will erase the database schemas and will re-execute migration scripts from scratch. Intended to be used in development only.", Default = false)]
        public bool EraseOnValidationError { get; set; }

        [Option('v', "target-version", HelpText = "Target version to reach. If empty it evolves all the way up.", Required = false)]
        public string TargetVersion { get; set; }

        [Option("start-version", HelpText = "Version used as starting point for already existing databases.", Default = "0")]
        public string StartVersion { get; set; }

        [Option("out-of-order", HelpText = "Allows migration scripts to be run “out of order”. If you already have versions 1 and 3 applied, and now a version 2 is found, it will be applied too instead of being ignored.", Default = false)]
        public bool OutOfOrder { get; set; }

        [Option("encoding", HelpText = "The encoding of migration scripts.", Default = "UTF-8")]
        public string Encoding { get; set; }

        [Option("scripts-prefix", HelpText = "Migration scripts file names prefix", Default = "V")]
        public string MigrationScriptsPrefix { get; set; }

        [Option("scripts-separator", HelpText = "Migration scripts file names separator", Default = "__")]
        public string MigrationScriptsSeparator { get; set; }

        [Option("scripts-suffix", HelpText = "Migration scripts files extension.", Default = ".sql")]
        public string ScriptsSuffix { get; set; }

        [Option("placeholder-prefix", HelpText = "The prefix of the placeholders.", Default = "${")]
        public string PlaceholderPrefix { get; set; }

        [Option("placeholder-suffix", HelpText = "The suffix of the placeholders.", Default = "}")]
        public string PlaceholderSuffix { get; set; }

        [Option("placeholders", HelpText = "Placeholders are strings prefixed by: “Evolve.Placeholder.” to replace in migration scripts. Format for commandline is \"key:value\".", Required = false)]
        public IEnumerable<string> Placeholders { get; set; }

        [Option("disable-cluster-mode", HelpText = "By default, Evolve will use a session level lock to coordinate the migration on multiple nodes. This prevents two distinct Evolve executions from executing an Evolve command on the same database at the same time. If this flag is set, it will not be the case.", Default = false)]
        public bool DisableClusterMode { get; set; }
    }
}
