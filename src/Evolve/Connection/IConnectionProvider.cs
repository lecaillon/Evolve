namespace Evolve.Connection
{
    /// <summary>
    ///     A strategy for obtaining a <see cref="WrappedConnection"/>.
    /// </summary>
    public interface IConnectionProvider
    {
        /// <summary>
        ///     Returns a <see cref="WrappedConnection"/>.
        /// </summary>
        /// <returns> A <see cref="WrappedConnection"/>. </returns>
        WrappedConnection GetConnection();
    }
}
