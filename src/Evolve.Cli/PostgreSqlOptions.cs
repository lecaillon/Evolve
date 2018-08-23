using CommandLine;

namespace Evolve.Cli
{
    [Verb("postgresql", HelpText = "Evolve with PostgreSQL")]
    internal class PostgreSqlOptions : SqlOptions
    {
        public override string Driver => "npgsql";
    }
}
