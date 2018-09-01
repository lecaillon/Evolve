using System.Collections.Generic;
using CommandLine;

namespace Evolve.Cli
{
    internal abstract class SqlOptions : Options
    {
        [Option('s', "metadata-table-schema", HelpText = "The schema in which the metadata table is/should be")]
        public string MetadataTableSchema { get; set; }

        [Option("schemas", HelpText = "A semicolon separated list of schema managed by Evolve. If empty, the default schema for the datasource connection is used.")]
        public IEnumerable<string> Schemas { get; set; }
    }
}
