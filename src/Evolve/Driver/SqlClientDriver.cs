using System;
using System.Data;

namespace Evolve.Driver
{
#if NETSTANDARD
    public class SqlClientDriver : IDriver
    {
        public IDbConnection CreateConnection()
        {
            throw new NotImplementedException();
        }
    }
#else
    public class SqlClientDriver : IDriver
    {
        public IDbConnection CreateConnection()
        {
            return new System.Data.SqlClient.SqlConnection();
        }
    }
#endif
}
