using System;

namespace Evolve
{
    public class EvolveException : Exception
    {
        public EvolveException() : base() { }

        public EvolveException(string message) : base(message) { }

        public EvolveException(string message, Exception innerException) : base(message, innerException) { }
    }
}
