using System;
using System.Collections.Generic;
using EvolveDb.Connection;
using EvolveDb.Dialect.Cassandra;
using EvolveDb.Dialect.CockroachDB;
using EvolveDb.Dialect.MySQL;
using EvolveDb.Dialect.PostgreSQL;
using EvolveDb.Dialect.SQLite;
using EvolveDb.Dialect.SQLServer;
using EvolveDb.Utilities;

namespace EvolveDb.Dialect
{
    internal static class DatabaseHelperFactory
    {
        private const string UnknownDBMS = "Unknown DBMS {0}.";

        private static readonly Dictionary<DBMS, Func<WrappedConnection, DatabaseHelper>> _dbmsMap = new Dictionary<DBMS, Func<WrappedConnection, DatabaseHelper>>
        {
            [DBMS.SQLite]           = wcnn => new SQLiteDatabase(wcnn),
            [DBMS.PostgreSQL]       = wcnn => new PostgreSQLDatabase(wcnn),
            [DBMS.MySQL]            = wcnn => new MySQLDatabase(wcnn),
            [DBMS.MariaDB]          = wcnn => new MySQLDatabase(wcnn),
            [DBMS.SQLServer]        = wcnn => new SQLServerDatabase(wcnn),
            [DBMS.Cassandra]        = wcnn => new CassandraCluster(wcnn),
            [DBMS.CockroachDB]      = wcnn => new CockroachDBCluster(wcnn),
        };

        public static DatabaseHelper GetDatabaseHelper(DBMS dbmsType, WrappedConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            _dbmsMap.TryGetValue(dbmsType, out Func<WrappedConnection, DatabaseHelper>? dbHelperCreationDelegate);
            if(dbHelperCreationDelegate is null)
            {
                throw new EvolveException(string.Format(UnknownDBMS, dbmsType));
            }

            return dbHelperCreationDelegate(connection);
        }
    }
}
