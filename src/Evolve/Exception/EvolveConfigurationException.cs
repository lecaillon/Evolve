using System;
using System.Diagnostics.CodeAnalysis;

namespace EvolveDb
{
    [SuppressMessage("Design", "CA1032: Implement standard exception constructors")]
    public class EvolveConfigurationException : EvolveException
    {
        public EvolveConfigurationException(string message) : base(message) { }

        public EvolveConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
