using System.Collections.Generic;
using System.Text;
using Evolve.Migration;

namespace Evolve.Configuration
{
    /// <summary>
    ///     <para>
    ///         Evolve configuration for sql migrations.
    ///     </para>
    ///     <para>
    ///         Sql migrations have the following file name structure: prefixVERSIONseparatorDESCRIPTIONsuffix
    ///         Example: V1_3_1__Migration_description.sql
    ///     </para>
    ///     <para>
    ///         Placeholders are strings to replace in sql migrations.
    ///         Example: ${schema}
    ///     </para>
    /// </summary>
    public interface IEvolveConfiguration
    {
        /// <summary>
        ///     <para>
        ///         Returns the connection string to the database.
        ///     </para>
        ///     <para>
        ///         Must have the necessary privileges to execute ddl.
        ///     </para>
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        ///     Returns the full name of the driver used to interact with the database. (default: Evolve.Driver.SqlClientDriver)
        /// </summary>
        string Driver { get; set; }

        /// <summary>
        ///     Returns the paths (separated by semicolon) to scan recursively for migrations. (default: Sql_Scripts)
        /// </summary>
        IEnumerable<string> Locations { get; set; }

        /// <summary>
        ///     Returns the encoding of Sql migrations. (default: UTF-8)
        /// </summary>
        Encoding Encoding { get; set; }

        /// <summary>
        ///     Returns the file name prefix for sql migrations. (default: V)
        /// </summary>
        string SqlMigrationPrefix { get; set; }

        /// <summary>
        ///     Returns the file name separator for sql migrations. (default: __)
        /// </summary>
        string SqlMigrationSeparator { get; set; }

        /// <summary>
        ///     Returns the file name suffix for sql migrations. (default: .sql)
        /// </summary>
        string SqlMigrationSuffix { get; set; }

        /// <summary>
        ///     Returns the semicolon separated list of schema managed by Evolve. 
        ///     (default: If empty, the default schema for the datasource connection.)
        /// </summary>
        IEnumerable<string> Schemas { get; set; }

        /// <summary>
        ///     Returns the schema containing the metadata table. 
        ///     (default: The default schema for the datasource connection, or the first defined in Schemas if exists.)
        /// </summary>
        string MetadataTableSchema { get; set; }

        /// <summary>
        ///     Returns the metadata table name. (default: changelog)
        /// </summary>
        string MetadaTableName { get; set; }

        /// <summary>
        ///     Returns the prefix of the placeholders. (default: ${)
        /// </summary>
        string PlaceholderPrefix { get; set; }

        /// <summary>
        ///     Returns the suffix of the placeholders. (default: })
        /// </summary>
        string PlaceholderSuffix { get; set; }

        /// <summary>
        ///     <para>
        ///         Returns the list of supplied placeholders/values defined in the configuration file.
        ///     </para>
        ///     <para>
        ///         Placeholders are strings prefixed by: "Evolve.Placeholder." to replace in sql migrations.
        ///     </para>
        ///     <para>
        ///         Example: ${schema} will be replaced by the value defined by the property 
        ///         Evolve.Placeholder.schema in the configuration file.
        ///     </para>
        /// </summary>
        Dictionary<string, string> Placeholders { get; set; }

        /// <summary>
        ///     Returns the target version to reach. If null or empty it evolves all the way up.
        /// </summary>
        MigrationVersion TargetVersion { get; set; }
    }
}
