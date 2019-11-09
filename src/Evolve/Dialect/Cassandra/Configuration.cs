using System.IO;
using System.Text;

namespace Evolve.Dialect.Cassandra
{
    internal sealed class Configuration
    {
        public const string ConfigurationFile = "evolve.cassandra.json";
        public const string DefaultKeyspaceKey = "_default";

        public static bool ConfigurationFileExists() => File.Exists(ConfigurationFile);
        public static string GetConfiguration() => File.ReadAllText(ConfigurationFile, Encoding.UTF8);
    }
}
