using System.Collections.Generic;
using System.Text;

namespace Evolve.Migration
{
    internal interface IMigrationLoader
    {
        /// <summary>
        ///     Returns a list of migration scripts ordered by version.
        /// </summary>
        /// <param name="prefix"> File name prefix for sql migrations. </param>
        /// <param name="separator"> File name separator for sql migrations. </param>
        /// <param name="suffix"> File name suffix for sql migrations. </param>
        /// <param name="encoding"> Encoding of the migration script. </param>
        /// <returns> A list of migration script. </returns>
        /// <exception cref="EvolveException"> Throws EvolveException when duplicate version found. </exception>
        IEnumerable<MigrationScript> GetMigrations(string prefix, string separator, string suffix, Encoding encoding);

        /// <summary>
        ///     Returns a list of repeatable migration scripts ordered by name.
        /// </summary>
        /// <param name="prefix"> File name prefix for sql repeatable migrations. </param>
        /// <param name="separator"> File name separator for sql migrations. </param>
        /// <param name="suffix"> File name suffix for sql migrations. </param>
        /// <param name="encoding"> Encoding of the migration script. </param>
        /// <returns> A list of repeatable migration script. </returns>
        /// <exception cref="EvolveException"> Throws EvolveException when duplicate version found. </exception>
        IEnumerable<MigrationScript> GetRepeatableMigrations(string prefix, string separator, string suffix, Encoding encoding);
    }
}
