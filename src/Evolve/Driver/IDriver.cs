using System.Data;

namespace Evolve.Driver
{
    public interface IDriver
    {
        /// <summary>
        ///     Creates an <see cref="IDbConnection"/> object for the specific driver.
        /// </summary>
        /// <param name="connectionString"> The connection string used to initialize the IDbConnection. </param>
        /// <returns> An initialized database connection. </returns>
        IDbConnection CreateConnection(string connectionString);
    }
}
