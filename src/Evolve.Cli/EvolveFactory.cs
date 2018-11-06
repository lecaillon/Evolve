namespace Evolve.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SQLite;
    using System.Linq;
    using Cassandra.Data;
    using Dialect;
    using Migration;
    using MySql.Data.MySqlClient;
    using Npgsql;

    internal static class EvolveFactory
    {
        public static Evolve Build(Program options, Action<string> logInfoDelegate = null)
        {
            IDbConnection cnn = null;

            switch (options.Database)
            {
                case DBMS.MySQL:
                    cnn = new MySqlConnection(options.ConnectionString);
                    break;
                case DBMS.MariaDB:
                    cnn = new MySqlConnection(options.ConnectionString);
                    break;
                case DBMS.PostgreSQL:
                    cnn = new NpgsqlConnection(options.ConnectionString);
                    break;
                case DBMS.SQLite:
                    cnn = new SQLiteConnection(options.ConnectionString);
                    break;
                case DBMS.SQLServer:
                    cnn = new SqlConnection(options.ConnectionString);
                    break;
                case DBMS.Cassandra:
                    cnn = new CqlConnection(options.ConnectionString);
                    break;
                default:
                    break;
            }

            var evolve = new Evolve(cnn, logInfoDelegate)
            {
                Command = options.Command,
                Locations = options.Locations,
                Schemas = options.Schemas,
                MetadataTableSchema = options.MetadataTableSchema,
                MetadataTableName = options.MetadataTableName,
                TargetVersion = ParseVersion(options.TargetVersion, MigrationVersion.MaxVersion),
                SqlMigrationSuffix = options.ScriptsSuffix
            };

            if (options.Placeholders != null)
            {
                evolve.Placeholders = MapPlaceholders(options.Placeholders, "${", "}");
            }

            if (options.Database == DBMS.Cassandra)
            {
                evolve.Schemas = options.Keyspaces;
                evolve.MetadataTableSchema = options.MetadataTableKeyspace;
            }

            return evolve;
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
    }
}
