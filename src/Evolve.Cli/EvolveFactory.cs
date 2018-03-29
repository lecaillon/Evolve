using Evolve.Configuration;
using Evolve.Migration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Evolve.Cli
{
    internal static class EvolveFactory
    {
        public static Evolve Build(IDbConnection connection, Options options)
        {
            var evolve = new Evolve(connection, Console.WriteLine)
            {
                Command = MapToCommandOptions(options.Command),
                CommandTimeout = options.Timeout,
                EnableClusterMode = !options.DisableClusterMode,
                Encoding = ParseEncoding(options.Encoding),
                IsEraseDisabled = !options.EnableErase,
                Locations = options.Locations,
                MetadataTableName = options.MetadataTableName,
                OutOfOrder = options.OutOfOrder,
                SqlMigrationSuffix = options.ScriptsSuffix,
                PlaceholderPrefix = options.PlaceholderPrefix,
                PlaceholderSuffix = options.PlaceholderSuffix,
                Placeholders = MapPlaceholders(options.Placeholders),
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
                    evolve.IsEraseDisabled = !cassandraOptions.EnableErase;
                    evolve.MustEraseOnValidationError = cassandraOptions.EraseOnValidationError;
                    evolve.SqlMigrationSuffix = cassandraOptions.ScriptsSuffix;
                    break;

            }

            return evolve;
        }

        private static MigrationVersion ParseVersion(string version, MigrationVersion defaultIfEmpty) =>
            !String.IsNullOrEmpty(version) ? new MigrationVersion(version) : defaultIfEmpty;

        private static Dictionary<string, string> MapPlaceholders(IEnumerable<string> placeholders) =>
            placeholders.Select(i => i.Split(':')).ToDictionary(i => i[0], i => i[1]);

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

        private static CommandOptions MapToCommandOptions(Commands command)
        {
            switch (command)
            {
                case Commands.migrate:
                    return CommandOptions.Migrate;
                case Commands.erase:
                    return CommandOptions.Erase;
                case Commands.repair:
                    return CommandOptions.Repair;
                default:
                    return CommandOptions.DoNothing;
            }
        }
    }
}
