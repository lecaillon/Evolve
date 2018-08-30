using CommandLine;

namespace Evolve.Cli
{
    [Verb("postgresql", HelpText = "Evolve with PostgreSQL (driver: Npgsql)")]
    internal class PostgreSqlOptions : SqlOptions
    {
        public override string Driver => "postgresql";
    }

    [Verb("npgsql", Hidden = true, HelpText = "Evolve with PostgreSQL (driver: Npgsql)")]
    internal class NpgsqlOptions : SqlOptions
    {
        public override string Driver => "npgsql";
    }
}
