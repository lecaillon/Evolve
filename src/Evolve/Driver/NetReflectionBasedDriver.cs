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
    ///         In order to load the driver <see cref="Type"/>, the current directory 
    ///         must be the one where the application to evolve is located.
    ///         
    ///         If not, the driver <see cref="Type"/> must be in an already loaded asssembly.
    ///     </para>
    /// </summary>
    public abstract class NetReflectionBasedDriver : ReflectionBasedDriver
    {
        protected const string DriverTypeNotFound = "Driver connection Type not be found in assembly {0}. Ensure that the assembly is located in the application directory.";

        /// <summary>
        ///     Initializes a new instance of <see cref="NetReflectionBasedDriver"/> with
        ///     type names that are loaded from the specified assembly.
        /// </summary>
        /// <param name="driverAssemblyName"> Assembly to load the driver Type from. </param>
        /// <param name="connectionTypeName"> Name of the driver Type. </param>
        protected NetReflectionBasedDriver(string driverAssemblyName, string connectionTypeName) : base(driverAssemblyName, connectionTypeName)
        {
        }

        /// <summary>
        ///     Returns the driver <see cref="Type"/> from the assembly specified in <see cref="DriverTypeName"/>
        /// </summary>
        /// <returns> The driver type. </returns>
        /// <exception cref="EvolveException"> When the driver type can't be loaded. </exception>
        protected override Type TypeFromAssembly()
        {
            try
            {
                var assembly = Assembly.LoadFrom(DriverTypeName.Assembly + ".dll");
                return assembly.GetType(DriverTypeName.Type);
            }
            catch(Exception ex)
            {
                throw new EvolveException(string.Format(DriverTypeNotFound, DriverTypeName.Assembly + ".dll"), ex);
            }
        }
    }
}

#endif
