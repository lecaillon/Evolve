using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Evolve.MSBuild
{
    public abstract class CliArgsBuilder
    {
        private const string EnvVarPatternMatching = @"\$\{(\w+)\}|\$(\w+)";
        private const string EnvVarPrefix = @"${";
        private const string EnvVarSuffix = @"}";
        protected const string IncorrectFileFormat = "Incorrect Evolve configuration file format at: {0}.";

        public CliArgsBuilder(string configFile, string env = null)
        {
            ConfigFile = configFile;
            Env = env;

            Command = ReadValue("Evolve.Command");
            Database = ReadValue("Evolve.Database");
            ConnectionString = ReadValue("Evolve.ConnectionString");
            Locations = SplitCommaSeparatedString(ReadValue("Evolve.Locations"));
            EraseDisabled = ReadValue("Evolve.EraseDisabled");
            EraseOnValidationError = ReadValue("Evolve.EraseOnValidationError");
            Encoding = ReadValue("Evolve.Encoding");
            SqlMigrationPrefix = ReadValue("Evolve.SqlMigrationPrefix");
            SqlMigrationSeparator = ReadValue("Evolve.SqlMigrationSeparator");
            SqlMigrationSuffix = ReadValue("Evolve.SqlMigrationSuffix");
            Schemas = SplitCommaSeparatedString(ReadValue("Evolve.Schemas"));
            MetadataTableSchema = ReadValue("Evolve.MetadataTableSchema");
            MetadataTableName = ReadValue("Evolve.MetadataTableName");
            PlaceholderPrefix = ReadValue("Evolve.PlaceholderPrefix");
            PlaceholderSuffix = ReadValue("Evolve.PlaceholderSuffix");
            TargetVersion = ReadValue("Evolve.TargetVersion");
            StartVersion = ReadValue("Evolve.StartVersion");
            EnableClusterMode = ReadValue("Evolve.EnableClusterMode");
            OutOfOrder = ReadValue("Evolve.OutOfOrder");
            CommandTimeout = ReadValue("Evolve.CommandTimeout");
            EmbeddedResourceAssemblies = SplitCommaSeparatedString(ReadValue("Evolve.EmbeddedResourceAssemblies"));
            EmbeddedResourceFilters = SplitCommaSeparatedString(ReadValue("Evolve.EmbeddedResourceFilters"));
            Placeholders = Datasource.Where(x => x.Key.StartsWith("Evolve.Placeholder.", StringComparison.OrdinalIgnoreCase))
                                                      .Select(x => x.Key.Replace("Evolve.Placeholder.", "") + ":" + x.Value)
                                                      .ToArray();

            // Cassandra
            if (ReadValue("Evolve.Keyspaces") != null)
            {
                Schemas = SplitCommaSeparatedString(ReadValue("Evolve.Keyspaces"));
            }
            if (ReadValue("Evolve.MetadataTableKeyspace") != null)
            {
                MetadataTableSchema = ReadValue("Evolve.MetadataTableKeyspace");
            }
        }

        protected abstract Dictionary<string, string> Datasource { get; }
        public string ConfigFile { get; }
        public string Env { get; }

        public string Command { get; protected set; }
        public string Database { get; protected set; }
        public string ConnectionString { get; protected set; }
        public string[] Locations { get; protected set; }
        public string EraseDisabled { get; protected set; }
        public string EraseOnValidationError { get; protected set; }
        public string Encoding { get; protected set; }
        public string SqlMigrationPrefix { get; protected set; }
        public string SqlMigrationSeparator { get; protected set; }
        public string SqlMigrationSuffix { get; protected set; }
        public string[] Schemas { get; protected set; }
        public string MetadataTableSchema { get; protected set; }
        public string MetadataTableName { get; protected set; }
        public string PlaceholderPrefix { get; protected set; }
        public string PlaceholderSuffix { get; protected set; }
        public string TargetVersion { get; protected set; }
        public string StartVersion { get; protected set; }
        public string[] Placeholders { get; protected set; }
        public string EnableClusterMode { get; protected set; }
        public string OutOfOrder { get; protected set; }
        public string CommandTimeout { get; protected set; }
        public string[] EmbeddedResourceAssemblies { get; protected set; }
        public string[] EmbeddedResourceFilters { get; protected set; }

        /// <summary>
        ///     Returns the command-line argumements needed by the Evolve CLI 
        ///     or null if the MSBuild task is disable (when Evolve.Command is empty)
        /// </summary>
        /// <exception cref="EvolveMSBuildException"> When a required option is missing. </exception>
        public virtual string Build()
        {
            if (Command is null)
            {
                return null;
            }
            else
            {
                if (Database is null)
                {
                    throw new EvolveMSBuildException("Evolve.Database option is required. Allowed values are: postgresql, sqlite, sqlserver, mysql, mariadb or cassandra. See https://evolve-db.netlify.com/configuration for more informations.");
                }
                if (ConnectionString is null)
                {
                    throw new EvolveMSBuildException("Evolve.ConnectionString option is required. See https://evolve-db.netlify.com/configuration for more informations.");
                }
                if (Locations is null && EmbeddedResourceAssemblies is null)
                {
                    throw new EvolveMSBuildException("Evolve.Locations or Evolve.EmbeddedResourceAssemblies option is required. See https://evolve-db.netlify.com/configuration for more informations.");
                }
            }

            var builder = new StringBuilder();
            AppendArg(builder, null, Command, false);
            AppendArg(builder, null, Database, false);
            AppendArg(builder, "-c", ConnectionString, true);
            AppendArgs(builder, "-l", Locations, true);
            AppendArgs(builder, "-s", Schemas, true);
            AppendArg(builder, "--metadata-table-schema", MetadataTableSchema, true);
            AppendArg(builder, "--metadata-table", MetadataTableName, true);
            AppendArgs(builder, "-p", Placeholders, true);
            AppendArg(builder, "--placeholder-prefix", PlaceholderPrefix, false);
            AppendArg(builder, "--placeholder-suffix", PlaceholderSuffix, false);
            AppendArg(builder, "--target-version", TargetVersion, false);
            AppendArg(builder, "--start-version", StartVersion, false);
            AppendArg(builder, "--scripts-prefix", SqlMigrationPrefix, false);
            AppendArg(builder, "--scripts-suffix", SqlMigrationSuffix, false);
            AppendArg(builder, "--scripts-separator", SqlMigrationSeparator, false);
            AppendArg(builder, "--encoding", Encoding, false);
            AppendArg(builder, "--command-timeout", CommandTimeout, false);
            AppendArg(builder, "--out-of-order", OutOfOrder, false);
            AppendArg(builder, "--erase-disabled", EraseDisabled, false);
            AppendArg(builder, "--erase-on-validation-error", EraseOnValidationError, false);
            AppendArg(builder, "--enable-cluster-mode", EnableClusterMode, false);
            AppendArgs(builder, "-a", EmbeddedResourceAssemblies, true);
            AppendArgs(builder, "-f", EmbeddedResourceFilters, true);

            return builder.ToString().TrimEnd();
        }

        /// <summary>
        ///     Read the value of a variable from the configuration file.
        ///     Replace environment variables by their values if needed.
        /// </summary>
        protected string ReadValue(string key)
        {
            if (Datasource.TryGetValue(key, out string value))
            {
                value = Normalize(value);
                if (value is null || value.Trim() == string.Empty) // .NET35 does not support IsNullOrWhiteSpace()
                {
                    return null;
                }

                return value;
            }
            else
            {
                return null;
            }
        }

        protected static string Normalize(string value)
        {
            foreach (Match match in Regex.Matches(value, EnvVarPatternMatching))
            {
                string cleanEnvVar = match.Value.Replace(EnvVarPrefix, string.Empty)
                                                .Replace(EnvVarSuffix, string.Empty);

                string envVarValue = Environment.GetEnvironmentVariable(cleanEnvVar);
                if (envVarValue != null)
                {
                    value = value.Replace(match.Value, envVarValue);
                }
            }

            return value;
        }

        private static string[] SplitCommaSeparatedString(string value) 
            => value?.Split(';')?.Where(s => s != null && s.Trim() != string.Empty)?.Distinct(StringComparer.OrdinalIgnoreCase)?.ToArray();

        private static void AppendArg(StringBuilder builder, string option, string value, bool quoted)
        {
            if (value is null)
            {
                return;
            }

            builder.Append(option ?? "");
            builder.Append(option is null ? "" : "=");
            builder.Append(quoted ? "\"" + value + "\"" : value);
            builder.Append(" ");
        }

        private static void AppendArgs(StringBuilder builder, string option, string[] values, bool quoted)
        {
            if (values is null)
            {
                return;
            }

            foreach (string value in values)
            {
                AppendArg(builder, option, value, quoted);
            }
        }
    }
}