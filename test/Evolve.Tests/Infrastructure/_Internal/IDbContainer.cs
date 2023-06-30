using System.Data.Common;
using System.Threading.Tasks;

namespace EvolveDb.Tests.Infrastructure
{
    public interface IDbContainer
    {
        Task<bool> Start(bool fromScratch = false);
        string CnxStr { get; }
        int TimeOutInSec { get; }
        DbConnection CreateDbConnection();
    }
}
