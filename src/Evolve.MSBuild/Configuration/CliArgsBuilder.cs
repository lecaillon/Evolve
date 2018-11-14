using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Evolve.MSBuild
{
    public abstract class CliArgsBuilder
    {
        private const string EnvVarPatternMatching = @"\$\{(\w+)\}|\$(\w+)";
        private const string EnvVarPrefix = @"${";
        private const string EnvVarSuffix = @"}";

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
            Placeholders = Datasource.Where(x => x.Key.StartsWith("Evolve.Placeholder.", StringComparison.OrdinalIgnoreCase))
                                                      .ToDictionary(x => x.Key.Replace("Evolve.Placeholder.", PlaceholderPrefix ?? "${") + PlaceholderSuffix ?? "}", x => x.Value);

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

        protected Dictionary<string, string> Datasource { get; } = new Dictionary<string, string>();
        protected string ConfigFile { get; }
        protected string Env { get; }

        protected string Command { get; set; }
        protected string Database { get; set; }
        protected string ConnectionString { get; set; }
        protected string[] Locations { get; set; }
        protected string EraseDisabled { get; set; }
        protected string EraseOnValidationError { get; set; }
        protected string Encoding { get; set; }
        protected string SqlMigrationPrefix { get; set; }
        protected string SqlMigrationSeparator { get; set; }
        protected string SqlMigrationSuffix { get; set; }
        protected string[] Schemas { get; set; }
        protected string MetadataTableSchema { get; set; }
        protected string MetadataTableName { get; set; }
        protected string PlaceholderPrefix { get; set; }
        protected string PlaceholderSuffix { get; set; }
        protected string TargetVersion { get; set; }
        protected string StartVersion { get; set; }
        protected Dictionary<string, string> Placeholders { get; set; } = new Dictionary<string, string>();
        protected string EnableClusterMode { get; set; }
        protected string OutOfOrder { get; set; }
        protected string CommandTimeout { get; set; }


        public virtual string Build()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Read the value of a variable from the configuration file.
        ///     Replace environment variables by their values if needed.
        /// </summary>
        protected string ReadValue(string key)
        {
            if (Datasource.TryGetValue(key, out string value))
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

        private string[] SplitCommaSeparatedString(string value) 
            => value?.Split(';')?.Where(s => s != null && s.Trim() != string.Empty)?.Distinct(StringComparer.OrdinalIgnoreCase)?.ToArray();
    }
}