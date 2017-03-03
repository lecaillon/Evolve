using Evolve.Utilities;
using System;
using System.Data;
using System.Reflection;

namespace Evolve.Driver
{
    public abstract class ReflectionBasedDriver : IDriver
    {
#if NETSTANDARD
        protected const string IDbConnectionImplementationNotFound = "";
#else
        protected const string IDbConnectionImplementationNotFound = "The IDbConnection implementation could not be found in the assembly {0}. Ensure that the assembly is located in the application directory.";
#endif
        /// <summary>
        ///     Initializes a new instance of <see cref="ReflectionBasedDriver" /> with
        ///     type names that are loaded from the specified assembly.
        /// </summary>
        /// <param name="driverAssemblyName"> Assembly to load the DbConnection type from. </param>
        /// <param name="connectionTypeName"> DbConnection type name. </param>
        protected ReflectionBasedDriver(string driverAssemblyName, string connectionTypeName)
        {
            DriverTypeName = new AssemblyQualifiedTypeName(connectionTypeName, driverAssemblyName);
            DbConnectionType = TypeFromLoadedAssembly();
            if(DbConnectionType == null)
            {
                DbConnectionType = TypeFromAssembly();
            }
        }

        public Type DbConnectionType { get; set; }

        protected AssemblyQualifiedTypeName DriverTypeName { get; }

        public IDbConnection CreateConnection()
        {
            return (IDbConnection)Activator.CreateInstance(DbConnectionType);
        }

        /// <summary>
        ///     Try to return a DbConnection from an already loaded assembly.
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

#if NETSTANDARD

        /// <summary>
        ///     Try to return a DbConnection from an assembly.
        /// </summary>
        /// <returns> A DbConnection type or null. </returns>
        protected virtual Type TypeFromAssembly()
        {
            throw new NotImplementedException();
        }

#else

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

#endif

        protected class AssemblyQualifiedTypeName
        {
            public AssemblyQualifiedTypeName(string type, string assembly)
            {
                Type = Check.NotNullOrEmpty(type, nameof(type));
                Assembly = Check.NotNullOrEmpty(assembly, nameof(assembly));
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
