namespace EvolveDb.Configuration
{
    /// <summary>
    ///     Define Evolve transaction mode.
    /// </summary>
    public enum TransactionKind
    {
        /// <summary>
        ///     Commit each successful script and rollback only the one that fails (default mode).
        /// </summary>
        CommitEach,

        /// <summary>
        ///     Commit all the scripts at once and rollback them all if one fails.
        ///     Either all succedeed or nothing is applied.
        /// </summary>
        CommitAll,

        /// <summary>
        ///     Execute the scripts of the migration and then rollback them all, in order
        ///     to preview/validate the changes Evolve would make to the database.
        /// </summary>
        RollbackAll
    }
}
