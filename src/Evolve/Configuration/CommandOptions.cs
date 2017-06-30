namespace Evolve.Configuration
{
    /// <summary>
    ///     The base commands for Evolve.
    /// </summary>
    public enum CommandOptions
    {
        /// <summary>
        ///     Does nothing.
        /// </summary>
        DoNothing,

        /// <summary>
        ///     Migrates the database.
        /// </summary>
        Migrate,

        /// <summary>
        ///     Corrects checksums of the applied migrations in the metadata table,
        ///     with the ones from migration scripts.
        /// </summary>
        Repair,

        /// <summary>
        ///     Erases the database schemas listed in <see cref="IEvolveConfiguration.Schemas"/>.
        ///     Only works if Evolve has created the schema at first or found it empty.
        ///     Otherwise Evolve won't do anything.
        /// </summary>
        Erase
    }
}
