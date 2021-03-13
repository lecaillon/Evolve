namespace Evolve.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SQLite;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Cassandra.Data;
    using Dialect;
    using Migration;
    using MySqlConnector;
    using Npgsql;

    internal static class EvolveFactory
    {
        public static Evolve Build(Program options, Action<string> logInfoDelegate = null)
        {
            var cnn = CreateConnection(options.Database, options.ConnectionString);
            var evolve = new Evolve(cnn, logInfoDelegate)
            {
                Command = options.Command,
                Locations = options.Locations,
                Schemas = options.Schemas,
                MetadataTableSchema = options.MetadataTableSchema,
                MetadataTableName = options.MetadataTableName,
                StartVersion = ParseVersion(options.StartVersion, MigrationVersion.MinVersion),
                TargetVersion = ParseVersion(options.TargetVersion, MigrationVersion.MaxVersion),
                SqlMigrationPrefix = options.ScriptsPrefix,
                SqlRepeatableMigrationPrefix = options.RepeatableScriptsPrefix,
                SqlMigrationSuffix = options.ScriptsSuffix,
                SqlMigrationSeparator = options.ScriptsSeparator,
                PlaceholderPrefix = options.PlaceholderPrefix,
                PlaceholderSuffix = options.PlaceholderSuffix,
                Encoding = ParseEncoding(options.Encoding),
                CommandTimeout = options.CommandTimeout,
                OutOfOrder = options.OutOfOrder,
                IsEraseDisabled = options.EraseDisabled,
                MustEraseOnValidationError = options.EraseOnValidationError,
                EnableClusterMode = options.EnableClusterMode,
                RetryRepeatableMigrationsUntilNoError = options.RetryRepeatableMigrationsUntilNoError,
                TransactionMode = options.TransactionMode,
                SkipNextMigrations = options.SkipNextMigrations,
            };

            if (options.Placeholders != null)
            {
                evolve.Placeholders = MapPlaceholders(options.Placeholders, options.PlaceholderPrefix, options.PlaceholderSuffix);
            }

            if (options.EmbeddedResourceLocations != null)
            {
                evolve.EmbeddedResourceAssemblies = LoadAssemblies(options.EmbeddedResourceLocations);
                evolve.EmbeddedResourceFilters = options.EmbeddedResourceFilters;
            }

            if (options.Database == DBMS.Cassandra)
            {
                evolve.Schemas = options.Keyspaces;
                evolve.MetadataTableSchema = options.MetadataTableKeyspace;
            }

            return evolve;
        }

        private static DbConnection CreateConnection(DBMS database, string cnnStr)
        {
            DbConnection cnn = null;

            switch (database)
            {
                case DBMS.MySQL:
                    cnn = new MySqlConnection(cnnStr);
                    break;
                case DBMS.MariaDB:
                    cnn = new MySqlConnection(cnnStr);
                    break;
                case DBMS.PostgreSQL:
                    cnn = new NpgsqlConnection(cnnStr);
                    break;
                case DBMS.SQLite:
                    cnn = new SQLiteConnection(cnnStr);
                    break;
                case DBMS.SQLServer:
                    cnn = new SqlConnection(cnnStr);
                    break;
                case DBMS.Cassandra:
                    cnn = new CqlConnection(cnnStr);
                    break;
                case DBMS.CockroachDB:
                    cnn = new NpgsqlConnection(cnnStr);
                    break;
                default:
                    break;
            }

            return cnn;
        }

        private static Dictionary<string, string> MapPlaceholders(string[] placeholders, string prefix, string suffix)
        {
            try
            {
                return placeholders.Select(i => i.Split(':')).ToDictionary(i => prefix + i[0] + suffix, i => i[1]);
            }
            catch
            {
                throw new EvolveConfigurationException("Error parsing --placeholder. Format is \"key:value\"");
            }
        }

        private static MigrationVersion ParseVersion(string version, MigrationVersion defaultIfEmpty) =>
            !string.IsNullOrEmpty(version) ? new MigrationVersion(version) : defaultIfEmpty;

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

        private static List<Assembly> LoadAssemblies(string[] assemblies)
        {
            var list = new List<Assembly>();
            foreach (var assembly in assemblies)
            {
                try
                {
                    list.Add(Assembly.LoadFrom(assembly));
                }
                catch (Exception ex)
                {
                    throw new EvolveConfigurationException($"Error loading assembly from --embedded-resource-assembly {assembly}", ex);
                }
            }

            return list;
        }
    }
}
