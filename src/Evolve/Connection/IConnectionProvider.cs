using Evolve.Configuration;
using Evolve.Driver;
using System;
using System.Data;

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
        ///     Dispose of a used <see cref="IDbConnection"/>
        /// </summary>
        /// <param name="cnx"> The connection to clean up. </param>
        void CloseConnection(IDbConnection cnx);

        /// <summary>
        ///     Gets the <see cref="IDriver"/> used to interact with the database.
        /// </summary>
        IDriver Driver { get; }

        /// <summary>
        ///     Returns an open <see cref="IDbConnection"/>.
        /// </summary>
        /// <returns> An opened <see cref="IDbConnection"/>. </returns>
        IDbConnection GetConnection();
    }
}
