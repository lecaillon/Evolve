using Evolve.Connection;

namespace Evolve.Dialect
{
    public interface IDatabaseHelperFactory
    {
        DatabaseHelper GetDatabaseHelper(DBMS dbmsType, WrappedConnection connection);
    }
}