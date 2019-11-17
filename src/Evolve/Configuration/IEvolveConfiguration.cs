using System.Collections.Generic;
using System.Reflection;
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
        ///     Gets or sets the paths (separated by semicolon) to scan recursively for migrations. (default: Sql_Scripts)
        /// </summary>
        IEnumerable<string> Locations { get; set; }

        /// <summary>
        ///     <para>
        ///         The base command for Evolve. (default: doNothing)
        ///     </para>
        /// 
        ///     <para>
        ///         <see cref="CommandOptions.DoNothing"/> : Does nothing.
        ///     </para>
        ///     <para>
        ///         <see cref="CommandOptions.Migrate"/> : Migrates the database.
        ///     </para>
        ///     <para>
        ///         <see cref="CommandOptions.Erase"/> : Erases the database schemas listed in <see cref="Schemas"/>.
        ///                                              Only works if Evolve has created the schema at first or found it empty.
        ///                                              Otherwise Evolve won't do anything.
        ///     </para>
        ///     <para>
        ///         <see cref="CommandOptions.Repair"/> : Corrects checksums of the applied migrations in the metadata table,
        ///                                               with the ones from migration scripts.
        ///     </para>
        ///     <para>
        ///         <see cref="CommandOptions.Info"/> : Prints details about migrations mainly based on the EvolveMetadata table.
        ///     </para>
        /// </summary>
        CommandOptions Command { get; set; }

        /// <summary>
        ///     <para>
        ///         When true, ensures that Evolve will never erase schemas. (default: false;)
        ///     </para>
        ///     <para>
        ///         Highly recommended in production !
        ///     </para>
        /// </summary>
        bool IsEraseDisabled { get; set; }

        /// <summary>
        ///     <para>
        ///         When true, if incoherent migration checksums are found during validation phase,
        ///         Evolve will erase the database schemas and will re-execute migration scripts from scratch. (default: false)
        ///     </para>
        ///     <para>
        ///         Do not use in production !
        ///     </para>
        ///     <para>
        ///         Obviously useful during development.
        ///     </para>
        /// </summary>
        bool MustEraseOnValidationError { get; set; }

        /// <summary>
        ///     Gets or sets the encoding of Sql migrations. (default: UTF-8)
        /// </summary>
        Encoding Encoding { get; set; }

        /// <summary>
        ///     Gets or sets the file name prefix for sql migrations. (default: V)
        /// </summary>
        string SqlMigrationPrefix { get; set; }

        /// <summary>
        ///     Gets or sets the file name prefix for sql repeatable migrations. (default: R)
        /// </summary>
        string SqlRepeatableMigrationPrefix { get; set; }

        /// <summary>
        ///     Gets or sets the file name separator for sql migrations. (default: __)
        /// </summary>
        string SqlMigrationSeparator { get; set; }

        /// <summary>
        ///     Gets or sets the file name suffix for sql migrations. (default: .sql)
        /// </summary>
        string SqlMigrationSuffix { get; set; }

        /// <summary>
        ///     Gets or sets the semicolon separated list of schema managed by Evolve. 
        ///     (default: If empty, the default schema for the datasource connection.)
        /// </summary>
        IEnumerable<string> Schemas { get; set; }

        /// <summary>
        ///     Gets or sets the schema containing the metadata table. 
        ///     (default: If empty, the first schema defined in <see cref="Schemas"/> or the one of the datasource connection.)
        /// </summary>
        string MetadataTableSchema { get; set; }

        /// <summary>
        ///     Gets or sets the metadata table name. (default: changelog)
        /// </summary>
        string MetadataTableName { get; set; }

        /// <summary>
        ///     Gets or sets the prefix of the placeholders. (default: ${)
        /// </summary>
        string PlaceholderPrefix { get; set; }

        /// <summary>
        ///     Gets or sets the suffix of the placeholders. (default: })
        /// </summary>
        string PlaceholderSuffix { get; set; }

        /// <summary>
        ///     <para>
        ///         Gets or sets the list of supplied placeholders/values defined in the configuration file.
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
        ///     Gets or sets the target version to reach. If null or empty, it evolves all the way up.
        /// </summary>
        MigrationVersion TargetVersion { get; set; }

        /// <summary>
        ///     Gets or sets the version used as a starting point for migrations. If null or empty it returns 0.
        /// </summary>
        MigrationVersion StartVersion { get; set; }

        /// <summary>
        ///     When true, Evolve will use a session level lock to coordinate the migrations on multiple nodes. (default: true)
        /// </summary>
        bool EnableClusterMode { get; set; }

        /// <summary>
        ///     When true, allows migrations to be run "out of order".
        ///     If you already have versions 1 and 3 applied, and now a version 2 is found,
        ///     it will be applied too instead of being ignored. (default: false;)
        /// </summary>
        bool OutOfOrder { get; set; }

        /// <summary>
        ///     Gets or sets the wait time before terminating the attempt to execute 
        ///     a migration and generating an error. (The default is 30 seconds.)
        /// </summary>
        int? CommandTimeout { get; set; }

        /// <summary>
        ///     When set, Evolve will scan the given list of assembly to load embedded migration scripts.
        /// </summary>
        IEnumerable<Assembly> EmbeddedResourceAssemblies { get; set; }

        /// <summary>
        ///     When set, exclude embedded migration scripts that do not start with one of these filters.
        /// </summary>
        IEnumerable<string> EmbeddedResourceFilters { get; set; }
    }
}
