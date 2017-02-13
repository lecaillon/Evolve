using System.Data;
using Evolve.Utilities;

namespace Evolve.Connection
{
    public class ConnectionProvider : IConnectionProvider
    {
        private readonly IDbConnection _connection;
        private IWrappedConnection _wrappedConnection;

        public ConnectionProvider(IDbConnection connection)
        {
            _connection = Check.NotNull(connection, nameof(connection));
        }

        public IWrappedConnection GetConnection()
        {
            if(_wrappedConnection == null)
            {
                _wrappedConnection = new WrappedConnection(_connection);
            }

            return _wrappedConnection;
        }
    }
}
