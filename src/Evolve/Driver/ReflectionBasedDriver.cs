#if NET

using System;
using System.Data;
using System.Reflection;
using Evolve.Utilities;

namespace Evolve.Driver
{
    /// <summary>
    ///     <para>
    ///         Base class for database drivers loaded by reflection.
    ///     </para>
    ///     <para>
    ///         In order to load the driver type, the current working directory 
    ///         must be the one where the application to Evolve is located.
    ///         
    ///         If not, the driver type must be in an already loaded asssembly.
    ///     </para>
    /// </summary>
    public abstract class ReflectionBasedDriver : IDriver
    {
        protected const string IDbConnectionImplementationNotFound = "The IDbConnection implementation could not be found in the assembly {0}. Ensure that the assembly is located in the application directory.";
      
        /// <summary>
        ///     Initializes a new instance of <see cref="ReflectionBasedDriver" /> with
        ///     type names that are loaded from the specified assembly.
        /// </summary>
        /// <param name="driverAssemblyName"> Assembly to load the DbConnection type from. </param>
        /// <param name="connectionTypeName"> DbConnection type name. </param>
        protected ReflectionBasedDriver(string driverAssemblyName, string connectionTypeName)
        {
            DriverTypeName = new AssemblyQualifiedTypeName(Check.NotNullOrEmpty(connectionTypeName, nameof(connectionTypeName)), 
                                                           Check.NotNullOrEmpty(driverAssemblyName, nameof(driverAssemblyName)));
            DbConnectionType = TypeFromLoadedAssembly();
            if(DbConnectionType == null)
            {
                DbConnectionType = TypeFromAssembly();
            }
        }

        protected AssemblyQualifiedTypeName DriverTypeName { get; }

        protected Type DbConnectionType { get; set; }

        /// <summary>
        ///     <para>
        ///         Creates an IDbConnection object for the specific Driver.
        ///     </para>
        ///     <para>
        ///         The connectionString is used to open a connection to the database to
        ///         force a load of the driver while the application current directory
        ///         is temporary changed to a folder where are stored the native dependencies.
        ///         
        ///         In the .NET framework world it is useless though.
        ///     </para>
        /// </summary>
        /// <param name="connectionString"> The connection string. </param>
        /// <returns>An IDbConnection object for the specific Driver.</returns>
        public IDbConnection CreateConnection(string connectionString)
        {
            var cnn = (IDbConnection)Activator.CreateInstance(DbConnectionType);
            cnn.ConnectionString = connectionString;
            return cnn;
        }

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

        /// <summary>
        ///     Try to return a DbConnection from an assembly.
        /// </summary>
        /// <returns> A DbConnection type. </returns>
        /// <exception cref="EvolveException"> When the DbConnection type can't be loaded. </exception>
        protected virtual Type TypeFromAssembly()
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

#endif
