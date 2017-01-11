using System.Data;

namespace Evolve.Core.Driver
{
    public interface IDriver
    {
        /// <summary>
        /// Creates an uninitialized IDbConnection object for the specific Driver
        /// </summary>
        IDbConnection CreateConnection();
    }
}
