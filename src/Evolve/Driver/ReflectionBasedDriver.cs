using System;
using System.Data;
using Evolve.Utilities;

namespace Evolve.Driver
{
    /// <summary>
    ///     Base class for database drivers loaded by reflection.
    /// </summary>
    public abstract class ReflectionBasedDriver : IDriver
    {
        private Type _dbConnectionType;

        /// <summary>
        ///     Initializes a new instance of a <see cref="ReflectionBasedDriver"/> with
        ///     type names that are loaded from the specified assembly.
        /// </summary>
        /// <param name="driverAssemblyName"> Assembly to load the driver Type from. </param>
        /// <param name="connectionTypeName"> Name of the driver Type. </param>
        protected ReflectionBasedDriver(string driverAssemblyName, string connectionTypeName)
        {
            DriverTypeName = new AssemblyQualifiedTypeName(Check.NotNullOrEmpty(connectionTypeName, nameof(connectionTypeName)), 
                                                           Check.NotNullOrEmpty(driverAssemblyName, nameof(driverAssemblyName)));
        }

        /// <summary>
        ///     Stores the database driver type designation
        /// </summary>
        protected AssemblyQualifiedTypeName DriverTypeName { get; }

        /// <summary>
        ///     <para>
        ///         Returns the database driver connection type.
        ///     </para>
        ///     <para>
        ///         Try to first find it from an already loaded assembly. <see cref="ReflectionBasedDriver.TypeFromLoadedAssembly()"/>
        ///     </para>
        ///     <para>
        ///         If it is not in memory, load it from the driver assembly. <see cref="ReflectionBasedDriver.TypeFromAssembly()"/>
        ///     </para>
        /// </summary>
        protected virtual Type DbConnectionType
        {
            get
            {
                if (_dbConnectionType == null)
                {
                    _dbConnectionType = TypeFromLoadedAssembly() ?? TypeFromAssembly();
                }

                return _dbConnectionType;
            }
        }

        /// <summary>
        ///     Creates an <see cref="IDbConnection"/> object for the specific driver.
        /// </summary>
        /// <param name="connectionString"> The connection string used to initialize the IDbConnection. </param>
        /// <returns> An initialized database connection. </returns>
        public virtual IDbConnection CreateConnection(string connectionString)
        {
            Check.NotNullOrEmpty(connectionString, nameof(connectionString));

            var cnn = (IDbConnection)Activator.CreateInstance(DbConnectionType);
            cnn.ConnectionString = connectionString;
            return cnn;
        }

        /// <summary>
        ///     Returns the driver <see cref="Type"/> from the assembly specified in <see cref="DriverTypeName"/>
        /// </summary>
        /// <returns> The driver type. </returns>
        /// <exception cref="EvolveException"> When the driver type can't be loaded. </exception>
        protected abstract Type TypeFromAssembly();

        /// <summary>
        ///     Attempt to return the driver <see cref="Type"/> from an already loaded assembly.
        /// </summary>
        /// <returns> The driver Type or null if not found. </returns>
        private Type TypeFromLoadedAssembly()
        {
            try
            {
                return Type.GetType(DriverTypeName.ToString());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Convenient class that stores usefull driver connection type informations.
        /// </summary>
        protected class AssemblyQualifiedTypeName
        {
            public AssemblyQualifiedTypeName(string type, string assembly)
            {
                Type = type;
                Assembly = assembly;
            }

            public string Type { get; }

            public string Assembly { get; }

            public override string ToString()
            {
                if (Assembly.IsNullOrWhiteSpace())
                {
                    return Type;
                }

                return string.Concat(Type, ", ", Assembly);
            }
        }
    }
}
