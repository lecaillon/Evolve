using System;
using System.Data.Common;

namespace Evolve.Tests.Infrastructure
{
    public interface IDbContainer : IDisposable
    {
        bool Start(bool fromScratch = false);
        string CnxStr { get; }
        int TimeOutInSec { get; }
        DbConnection CreateDbConnection();
    }
}
