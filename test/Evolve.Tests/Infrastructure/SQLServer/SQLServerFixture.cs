using System;

namespace EvolveDb.Tests.Infrastructure
{
    public class SQLServerFixture : DbContainerFixture<SQLServerContainer>, IDisposable
    {
        public string GetCnxStr(string dbName = "master") => CnxStr.Replace("master", dbName);
    }
}
