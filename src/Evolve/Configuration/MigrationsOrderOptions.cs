using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evolve.Configuration
{
    /// <summary>
    /// Defines the order in which versioned and repeatable migrations are applied.
    /// </summary>
    public enum MigrationsOrderOptions
    {
        /// <summary>
        ///     Versioned migrations executed 
        /// </summary>
        VersionedMigrationsFirst,

        /// <summary>
        ///     Repeatable migrations executed before versioned migrations.
        /// </summary>
        RepeatableMigrationsFirst
    }
}
