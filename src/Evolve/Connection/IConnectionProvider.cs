namespace Evolve.Connection
{
    /// <summary>
    ///     A strategy for obtaining a <see cref="WrappedConnection"/>.
    /// </summary>
    public interface IConnectionProvider
    {
        /// <summary>
        ///     Returns a wrapped <see cref="System.Data.IDbConnection"/>.
        /// </summary>
        /// <returns> A connection to the database to evolve. </returns>
        WrappedConnection GetConnection();
    }
}
