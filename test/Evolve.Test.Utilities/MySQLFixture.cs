using System;

namespace Evolve.Test.Utilities
{
    public class MySQLFixture : IDisposable
    {
        public MySQLFixture()
        {
            MySql = new MySQLDockerContainer();
        }

        public void Start(bool fromScratch = false)
        {
            MySql.Start(fromScratch);
        }

        public MySQLDockerContainer MySql { get; }
        public string HostPort => MySql.HostPort;
        public string DbName => MySql.DbName;
        public string DbPwd => MySql.DbPwd;
        public string DbUser => MySql.DbUser;
        public string CnxStr => MySql.CnxStr;

        public void Dispose()
        {
            MySql.Dispose();
        }
    }
}
