using System;

namespace Evolve
{
    public class EvolveConfigurationException : EvolveException
    {
        private const string EvolveConfigurationError = "Evolve configuration error: ";

        public EvolveConfigurationException(string message) : base(EvolveConfigurationError + message) { }

        public EvolveConfigurationException(string message, Exception innerException) : base(EvolveConfigurationError + message, innerException) { }
    }
}
