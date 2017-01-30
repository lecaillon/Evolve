using Evolve.Configuration;
using Evolve.Driver;
using System;

namespace Evolve.Connection
{
    /// <summary>
    ///     A strategy for obtaining a <see cref="IDbConnection"/>.
    /// </summary>
    public interface IConnectionProvider : IDisposable
    {
        /// <summary>
        ///     Initialize the connection provider.
        /// </summary>
        /// <param name="configuration"> The main Evolve configuration. </param>
        void Configure(IEvolveConfiguration configuration);

        /// <summary>
        ///     Gets the <see cref="IDriver"/> used to interact with the database.
        /// </summary>
        IDriver Driver { get; }

        /// <summary>
        ///     Returns a <see cref="IWrappedConnection"/>.
        /// </summary>
        /// <returns> A <see cref="IWrappedConnection"/>. </returns>
        IWrappedConnection GetConnection();
    }
}
