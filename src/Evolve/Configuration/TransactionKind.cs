namespace Evolve.Configuration
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
    }
}
