using System;

namespace Evolve
{
    public class EvolveConfigurationException : Exception
    {
        public EvolveConfigurationException() : base() { }

        public EvolveConfigurationException(string message) : base(message) { }

        public EvolveConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
