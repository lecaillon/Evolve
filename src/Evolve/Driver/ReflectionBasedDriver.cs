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
        ///     Initializes a new instance of <see cref="ReflectionBasedDriver"/> with
        ///     type names that are loaded from the specified assembly.
        /// </summary>
        /// <param name="driverAssemblyName"> Assembly to load the DbConnection type from. </param>
        /// <param name="connectionTypeName"> DbConnection type name. </param>
        protected ReflectionBasedDriver(string driverAssemblyName, string connectionTypeName)
        {
            DriverTypeName = new AssemblyQualifiedTypeName(Check.NotNullOrEmpty(connectionTypeName, nameof(connectionTypeName)), 
                                                           Check.NotNullOrEmpty(driverAssemblyName, nameof(driverAssemblyName)));
        }

        protected AssemblyQualifiedTypeName DriverTypeName { get; }

        /// <summary>
        ///     <para>
        ///         Try to load the <see cref="DbConnectionType"/> from an already loaded assembly.
        ///         <see cref="ReflectionBasedDriver.TypeFromLoadedAssembly()"/>
        ///     </para>
        ///     <para>
        ///         If the <see cref="DbConnectionType"/> is not in memory, load it from the driver assembly.
        ///         <see cref="ReflectionBasedDriver.TypeFromAssembly()"/>
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
        ///     Creates an IDbConnection object for the specific Driver.
        /// </summary>
        public virtual IDbConnection CreateConnection(string connectionString)
        {
            Check.NotNullOrEmpty(connectionString, nameof(connectionString));

            var cnn = (IDbConnection)Activator.CreateInstance(DbConnectionType);
            cnn.ConnectionString = connectionString;
            return cnn;
        }

        /// <summary>
        ///     Try to return a DbConnection from an assembly.
        /// </summary>
        /// <returns> A DbConnection type. </returns>
        /// <exception cref="EvolveException"> When the DbConnection type can't be loaded. </exception>
        protected abstract Type TypeFromAssembly();

        /// <summary>
        ///     Attempt to return a DbConnection from an already loaded assembly.
        /// </summary>
        /// <returns> A DbConnection type or null. </returns>
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
