#if NET

using System;
using System.Reflection;

namespace Evolve.Driver
{
    /// <summary>
    ///     <para>
    ///         Base class for .NET database drivers loaded by reflection.
    ///     </para>
    ///     <para>
    ///         In order to load the driver type, the current directory 
    ///         must be the one where the application to Evolve is located.
    ///         
    ///         If not, the driver type must be in an already loaded asssembly.
    ///     </para>
    /// </summary>
    public abstract class NetReflectionBasedDriver : ReflectionBasedDriver
    {
        protected const string IDbConnectionImplementationNotFound = "The IDbConnection implementation could not be found in the assembly {0}. Ensure that the assembly is located in the application directory.";

        /// <summary>
        ///     Initializes a new instance of <see cref="NetReflectionBasedDriver"/> with
        ///     type names that are loaded from the specified assembly.
        /// </summary>
        /// <param name="driverAssemblyName"> Assembly to load the DbConnection type from. </param>
        /// <param name="connectionTypeName"> DbConnection type name. </param>
        protected NetReflectionBasedDriver(string driverAssemblyName, string connectionTypeName) : base(driverAssemblyName, connectionTypeName)
        {
        }

        /// <summary>
        ///     Try to return a DbConnection from an assembly.
        /// </summary>
        /// <returns> A DbConnection type. </returns>
        /// <exception cref="EvolveException"> When the DbConnection type can't be loaded. </exception>
        protected override Type TypeFromAssembly()
        {
            try
            {
                var assembly = Assembly.LoadFrom(DriverTypeName.Assembly + ".dll");
                return assembly.GetType(DriverTypeName.Type);
            }
            catch(Exception ex)
            {
                throw new EvolveException(string.Format(IDbConnectionImplementationNotFound, DriverTypeName.Assembly + ".dll"), ex);
            }
        }
    }
}

#endif
