using System.Data;
using Evolve.Utilities;

namespace Evolve.Connection
{
    /// <summary>
    ///     A startegy for obtaining a <see cref="WrappedConnection"/> from an existing <see cref="IDbConnection"/>.
    /// </summary>
    public class ConnectionProvider : IConnectionProvider
    {
        private readonly IDbConnection _connection;
        private WrappedConnection _wrappedConnection;

        /// <summary>
        ///     Initializes a new instance of a <see cref="ConnectionProvider"/> from the given <paramref name="connection"/>.
        /// </summary>
        public ConnectionProvider(IDbConnection connection)
        {
            _connection = Check.NotNull(connection, nameof(connection));
        }

        /// <summary>
        ///     Returns a wrapped <see cref="System.Data.IDbConnection"/> from an existing <see cref="IDbConnection"/>.
        /// </summary>
        public WrappedConnection GetConnection()
        {
            if(_wrappedConnection == null)
            {
                _wrappedConnection = new WrappedConnection(_connection, false);
            }

            return _wrappedConnection;
        }
    }
}
