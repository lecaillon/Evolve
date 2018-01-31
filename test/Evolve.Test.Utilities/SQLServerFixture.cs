using System;

namespace Evolve.Test.Utilities
{
    public class SQLServerFixture : IDisposable
    {
        public SQLServerFixture()
        {
            MsSql = new SQLServerDockerContainer();
        }

        public void Start(bool fromScratch = false)
        {
            MsSql.Start(fromScratch);
        }

        public SQLServerDockerContainer MsSql { get; }
        public string DbPwd => MsSql.DbPwd;
        public string DbUser => MsSql.DbUser;

        public void Dispose()
        {
            MsSql.Dispose();
        }
    }
}
