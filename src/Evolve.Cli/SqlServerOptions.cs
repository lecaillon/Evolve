using CommandLine;

namespace Evolve.Cli
{
    [Verb("sqlserver", HelpText = "Evolve with SQLServer")]
    internal class SqlServerOptions : SqlOptions { }
}
