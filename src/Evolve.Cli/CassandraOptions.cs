using CommandLine;
using System.Collections.Generic;

namespace Evolve.Cli
{
    [Verb("cassandra", HelpText = "Evolve with Cassandra (driver: CassandraCSharpDriver)")]
    internal class CassandraOptions : Options
    {
        public override string Driver => "cassandra";

        [Option('k', "metadata-table-keyspace", HelpText = "The keyspace in which the metadata table is/should be", Required = true)]
        public string MetadataTableKeyspace { get; set; }

        [Option('l', "scripts-locations", HelpText = "Paths to scan recursively for migration scripts.", Default = new[] { "Cql_Scripts" })]
        public new IEnumerable<string> Locations { get; set; }

        [Option("keyspaces", HelpText = "A list of keyspaces managed by Evolve. If empty, the default schema for the datasource connection is used.")]
        public IEnumerable<string> Keyspaces { get; set; }

        [Option("erase-disabled", HelpText = "When set, ensures that Evolve will never erase schemas. Highly recommended in production.", Default = false)]
        public new bool EraseDisabled { get; set; }

        [Option("erase-on-validation-error", HelpText = "When set, if validation phase fails, Evolve will erase the keyspaces and will re-execute migration scripts from scratch. Intended to be used in development only.", Default = false)]
        public new bool EraseOnValidationError { get; set; }

        [Option("scripts-suffix", HelpText = "Migration scripts files extension.", Default = ".cql")]
        public new string ScriptsSuffix { get; set; }
    }
}
