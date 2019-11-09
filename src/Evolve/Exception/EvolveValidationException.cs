using System.Diagnostics.CodeAnalysis;

namespace Evolve
{
    [SuppressMessage("Design", "CA1032: Implement standard exception constructors")]
    public class EvolveValidationException : EvolveException
    {
        public EvolveValidationException(string message) : base(message) { }
    }
}
