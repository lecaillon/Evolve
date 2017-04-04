using System.Data;

namespace Evolve.Driver
{
    public interface IDriver
    {
        /// <summary>
        ///     <para>
        ///         Creates an IDbConnection object for the specific Driver.
        ///     </para>
        ///     <para>
        ///         The connectionString is used to open a connection to the database to
        ///         force a load of the driver while the application current directory
        ///         is temporary changed to a folder where are stored the native dependencies.
        ///     </para>
        /// </summary>
        /// <param name="connectionString"> The connection string. </param>
        /// <returns>An IDbConnection object for the specific Driver.</returns>
        IDbConnection CreateConnection(string connectionString);
    }
}
