namespace Evolve.Connection
{
    /// <summary>
    ///     A strategy for obtaining a <see cref="IDbConnection"/>.
    /// </summary>
    public interface IConnectionProvider
    {
        /// <summary>
        ///     Returns a <see cref="IWrappedConnection"/>.
        /// </summary>
        /// <returns> A <see cref="IWrappedConnection"/>. </returns>
        IWrappedConnection GetConnection();
    }
}
