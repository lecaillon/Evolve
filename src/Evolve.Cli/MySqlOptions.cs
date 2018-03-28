using CommandLine;

namespace Evolve.Cli
{
    [Verb("mysql", HelpText = "Evolve with MySQL")]
    internal class MySqlOptions : SqlOptions { }
}
