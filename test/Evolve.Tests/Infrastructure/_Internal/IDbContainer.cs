using System;
using System.Data;

namespace Evolve.Tests.Infrastructure
{
    public interface IDbContainer : IDisposable
    {
        bool Start(bool fromScratch = false);
        string CnxStr { get; }
        int TimeOutInSec { get; }
        IDbConnection CreateDbConnection();
    }
}
