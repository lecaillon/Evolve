using System;
using System.Collections.Generic;
using Evolve.Connection;
using Evolve.Dialect.PostgreSQL;
using Evolve.Dialect.SQLite;

namespace Evolve.Dialect
{
    public static class DatabaseHelperFactory
    {
        private static Dictionary<DBMS, Func<WrappedConnection, DatabaseHelper>> _dbmsMap = new Dictionary<DBMS, Func<WrappedConnection, DatabaseHelper>>
        {
            [DBMS.SQLite] = wcnn => new SQLiteDatabase(wcnn),
            [DBMS.PostgreSQL] = wcnn => new PostgreSQLDatabase(wcnn),
        };

        public static DatabaseHelper GetDatabaseHelper(DBMS dbmsType, WrappedConnection connection)
        {
            _dbmsMap.TryGetValue(dbmsType, out Func<WrappedConnection, DatabaseHelper> dbHelperCreationDelegate);
            if(dbHelperCreationDelegate == null)
            {

            }

            return dbHelperCreationDelegate(connection);
        }
    }
}
