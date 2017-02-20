using Evolve.Migration;
using System.Collections.Generic;

namespace Evolve.Metadata
{
    public interface IEvolveMetadata
    {
        /// <summary>
        ///     Lock the access to the metadata store to others migration processes.
        ///     Only one migration at a time is authorized.
        /// </summary>
        void Lock();

        /// <summary>
        ///     Create the metadata store if not exists.
        /// </summary>
        /// <returns> Returns true if created, false if it already exists. </returns>
        bool CreateIfNotExists();

        /// <summary>
        ///     Save the metadata of an executed migration.
        /// </summary>
        /// <param name="migration"> The migration script metadata. </param>
        /// <param name="success"> True if the migration succeeded, false otherwise. </param>
        void SaveMigrationMetadata(MigrationScript migration, bool success);

        /// <summary>
        ///     Register the name of the schema created by Evolve.
        /// </summary>
        /// <param name="schemaName"> Name of the schema created by Evolve. </param>
        void SaveCreatedSchemaName(string schemaName);

        /// <summary>
        ///     Define a version used as a starting point for the future migration.
        ///     All the migration scripts prior to this mark are ignored.
        /// </summary>
        /// <param name="version"> The migration version to start with. </param>
        void TagMigrationVersion(string version);

        /// <summary>
        ///     Returns all the migration metadata.
        /// </summary>
        /// <returns> The list of all migration metadata. </returns>
        IEnumerable<MigrationMetadata> GetAllMigrationMetadata();

        /// <summary>
        ///     True if Evolve has created the schema, false otherwise.
        /// </summary>
        /// <returns></returns>
        bool CanDropSchema(string schemaName);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string FindLatestVersionTagged();
    }
}
