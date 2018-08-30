using System;

namespace Evolve.Test.Utilities
{
    public class PostgreSqlFixture : IDisposable
    {
        public PostgreSqlFixture()
        {
            Pg = new PostgreSqlDockerContainer();
        }

        public void Start(bool fromScratch = false)
        {
            Pg.Start(fromScratch);
        }

        public PostgreSqlDockerContainer Pg { get; }
        public string HostPort => Pg.HostPort;
        public string DbName => Pg.DbName;
        public string DbPwd => Pg.DbPwd;
        public string DbUser => Pg.DbUser;
        public string CnxStr => Pg.CnxStr;

        public void Dispose()
        {
            Pg.Dispose();
        }
    }
}
