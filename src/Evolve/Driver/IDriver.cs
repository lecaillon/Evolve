using System.Data;

namespace Evolve.Driver
{
    public interface IDriver
    {
        /// <summary>
        ///     Creates an IDbConnection object for the specific Driver.
        /// </summary>
        IDbConnection CreateConnection(string connectionString);
    }
}
