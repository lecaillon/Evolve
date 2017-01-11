using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Evolve.Core.Utilities;

namespace Evolve.Core.Driver
{
    public abstract class ReflectionBasedDriver : IDriver
    {
        protected const string IDbConnectionImplementationNotFound = "The IDbConnection implementation in the assembly {0} could not be found. Ensure that the assembly is located in the application directory.";

        /// <summary>
        /// Initializes a new instance of <see cref="ReflectionBasedDriver" /> with
        /// type names that are loaded from the specified assembly.
        /// </summary>
        /// <param name="driverAssemblyName">Assembly to load the types from.</param>
        /// <param name="connectionTypeName">Connection type name.</param>
        protected ReflectionBasedDriver(string driverAssemblyName, string connectionTypeName)
        {
            ConnectionType = TypeFromAssembly(connectionTypeName, driverAssemblyName);

        }

        public IDbConnection CreateConnection()
        {
            throw new NotImplementedException();
        }

        protected Type TypeFromAssembly(string type, string assembly)
        {
            return TypeFromAssembly(new AssemblyQualifiedTypeName(type, assembly));
        }

        public Type ConnectionType { get; set; }

        // http://stackoverflow.com/questions/658446/how-do-i-find-the-fully-qualified-name-of-an-assembly
        // "[System.Reflection.AssemblyName]::GetAssemblyName(\"$(TargetPath)\").FullName"

        // http://stackoverflow.com/questions/37895278/how-to-load-assemblies-located-in-a-folder-in-net-core-console-app

        /// <summary>
        /// Returns a <see cref="Type"/> from an already loaded Assembly or an
        /// Assembly that is loaded with a partial name.
        /// </summary>
        /// <param name="name">An <see cref="AssemblyQualifiedTypeName" />.</param>
        /// <returns>
        /// A <see cref="Type"/> object that represents the specified type,
        /// or <see langword="null" /> if the type cannot be loaded.
        /// </returns>
        private Type TypeFromAssembly(AssemblyQualifiedTypeName name)
        {
            try
            {
                // Try to get the type from an already loaded assembly
                Type type = Type.GetType(name.ToString());

                if (type != null)
                {
                    return type;
                }

                //if (name.Assembly == null)
                //{
                //    // No assembly was specified for the type, so just fail
                //    string message = "Could not load type " + name + ". Possible cause: no assembly name specified.";
                //    log.Warn(message);
                //    if (throwOnError) throw new TypeLoadException(message);
                //    return null;
                //}

                //Assembly assembly = Assembly.Load(name.Assembly);

                //if (assembly == null)
                //{
                //    log.Warn("Could not load type " + name + ". Possible cause: incorrect assembly name specified.");
                //    return null;
                //}

                //type = assembly.GetType(name.Type, throwOnError);

                //if (type == null)
                //{
                //    log.Warn("Could not load type " + name + ".");
                //    return null;
                //}

                return type;
            }
            catch // (Exception e)
            {
                //if (log.IsErrorEnabled)
                //{
                //    log.Error("Could not load type " + name + ".", e);
                //}
                //if (throwOnError) throw;
                return null;
            }
        }

        private class AssemblyQualifiedTypeName
        {
            public AssemblyQualifiedTypeName(string type, string assembly = null)
            {
                Type = Check.NotNullOrEmpty(type, nameof(type));
                Assembly = assembly;
            }

            public string Type { get; }
            public string Assembly { get; }

            public override string ToString()
            {
                if (string.IsNullOrWhiteSpace(Assembly))
                {
                    return Type;
                }

                return string.Concat(Type, ", ", Assembly);
            }
        }
    }
}
