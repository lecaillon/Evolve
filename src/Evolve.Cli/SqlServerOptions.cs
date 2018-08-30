using CommandLine;

namespace Evolve.Cli
{
    [Verb("sqlserver", HelpText = "Evolve with SQLServer (driver: SqlClient)")]
    internal class SqlServerOptions : SqlOptions
    {
        public override string Driver => "sqlserver";
    }

    [Verb("sqlclient", Hidden = true, HelpText = "Evolve with SQLServer (driver: SqlClient)")]
    internal class SqlClientOptions : SqlOptions
    {
        public override string Driver => "sqlclient";
    }
}
