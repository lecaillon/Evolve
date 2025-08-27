namespace EvolveDb.Configuration
{
    /// <summary>
    ///     Defines how SQL lint failures should be handled.
    /// </summary>
    public enum SqlLintFailureLevel
    {
        /// <summary>
        ///     Log lint issues as warnings and continue execution.
        /// </summary>
        Warning,

        /// <summary>
        ///     Treat lint issues as errors and stop execution.
        /// </summary>
        Error
    }
}
