using System.Collections.Generic;

namespace Evolve.Migration
{
    public interface IMigrationLoader
    {
        /// <summary>
        ///     Returns the scripts found at <paramref name="locations"/> ordered by version.
        /// </summary>
        /// <param name="locations"> List of paths to scan recursively for migrations. </param>
        /// <param name="prefix"> File name prefix for sql migrations. </param>
        /// <param name="separator"> File name separator for sql migrations. </param>
        /// <param name="suffix"> File name suffix for sql migrations. </param>
        /// <returns> A list of migration script. </returns>
        /// <exception cref="EvolveException"> Throws EvolveException when duplicate version found. </exception>
        IEnumerable<MigrationScript> GetMigrations(IEnumerable<string> locations, string prefix, string separator, string suffix);
    }
}
