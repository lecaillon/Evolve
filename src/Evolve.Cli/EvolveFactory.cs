using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Evolve.Configuration;
using Evolve.Migration;

namespace Evolve.Cli
{
    internal static class EvolveFactory
    {
        public static Evolve Build(Options options)
        {
            var evolve = new Evolve(logInfoDelegate: Console.WriteLine)
            {
                Command = MapToCommandOptions(options.Command),
                CommandTimeout = options.CommandTimeout,
                ConnectionString = options.ConnectionString,
                Driver = options.Driver,
                EnableClusterMode = !options.DisableClusterMode,
                Encoding = ParseEncoding(options.Encoding),
                IsEraseDisabled = options.EraseDisabled,
                Locations = options.Locations,
                MetadataTableName = options.MetadataTableName,
                OutOfOrder = options.OutOfOrder,
                SqlMigrationSuffix = options.ScriptsSuffix,
                PlaceholderPrefix = options.PlaceholderPrefix,
                PlaceholderSuffix = options.PlaceholderSuffix,
                Placeholders = MapPlaceholders(options.Placeholders, options.PlaceholderPrefix, options.PlaceholderSuffix),
                SqlMigrationPrefix = options.MigrationScriptsPrefix,
                SqlMigrationSeparator = options.MigrationScriptsSeparator,
                StartVersion = ParseVersion(options.StartVersion, MigrationVersion.MinVersion),
                TargetVersion = ParseVersion(options.TargetVersion, MigrationVersion.MaxVersion)
            };

            switch (options)
            {
                case SqlOptions sqlOptions:
                    evolve.MetadataTableSchema = sqlOptions.MetadataTableSchema;
                    evolve.Schemas = sqlOptions.Schemas;
                    break;
                case CassandraOptions cassandraOptions:
                    evolve.MetadataTableSchema = cassandraOptions.MetadataTableKeyspace;
                    evolve.Locations = cassandraOptions.Locations;
                    evolve.Schemas = cassandraOptions.Keyspaces;
                    evolve.IsEraseDisabled = cassandraOptions.EraseDisabled;
                    evolve.MustEraseOnValidationError = cassandraOptions.EraseOnValidationError;
                    evolve.SqlMigrationSuffix = cassandraOptions.ScriptsSuffix;
                    break;
            }

            return evolve;
        }

        private static MigrationVersion ParseVersion(string version, MigrationVersion defaultIfEmpty) =>
            !string.IsNullOrEmpty(version) ? new MigrationVersion(version) : defaultIfEmpty;

        private static Dictionary<string, string> MapPlaceholders(IEnumerable<string> placeholders, string prefix, string suffix)
        {
            try
            {
                return placeholders.Select(i => i.Split(':')).ToDictionary(i => prefix + i[0] + suffix, i => i[1]);
            }
            catch
            {
                throw new EvolveConfigurationException("Error parsing --placeholders. Format is \"key:value\"");
            }
        }

        private static Encoding ParseEncoding(string encoding)
        {
            try
            {
                return Encoding.GetEncoding(encoding);
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        private static CommandOptions MapToCommandOptions(Command command)
        {
            switch (command)
            {
                case Command.migrate:
                    return CommandOptions.Migrate;
                case Command.erase:
                    return CommandOptions.Erase;
                case Command.repair:
                    return CommandOptions.Repair;
                default:
                    return CommandOptions.DoNothing;
            }
        }
    }
}
