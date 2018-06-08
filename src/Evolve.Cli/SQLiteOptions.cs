using CommandLine;

namespace Evolve.Cli
{
    [Verb("sqlite", HelpText = "Evolve with SQLite")]
    internal class SQLiteOptions : SqlOptions
    {
        public override string Driver => "sqlite";
    }
}
