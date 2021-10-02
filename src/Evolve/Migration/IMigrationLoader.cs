using System.Collections.Generic;

namespace Evolve.Migration
{
    /// <summary>
    ///     Defines methods used to load all migrations (applied, pending, ignored...)
    /// </summary>
    public interface IMigrationLoader
    {
        /// <summary>
        ///     Returns a list of migration scripts ordered by version.
        /// </summary>
        /// <returns> A list of migration script. </returns>
        /// <exception cref="EvolveException"> Throws EvolveException when duplicate version found. </exception>
        IEnumerable<MigrationScript> GetMigrations();

        /// <summary>
        ///     Returns a list of repeatable migration scripts ordered by name.
        /// </summary>
        /// <returns> A list of repeatable migration script. </returns>
        /// <exception cref="EvolveException"> Throws EvolveException when duplicate name found. </exception>
        IEnumerable<MigrationScript> GetRepeatableMigrations();
    }
}
