using System;

namespace Evolve
{
    public class EvolveValidationException : EvolveException
    {
        public EvolveValidationException(string message) : base(message) { }
    }
}
