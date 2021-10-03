using System;
using System.Diagnostics.CodeAnalysis;

namespace EvolveDb
{
    [SuppressMessage("Design", "CA1032: Implement standard exception constructors")]
    [SuppressMessage("Usage", "CA2237: Mark ISerializable types with serializable")]
    public class EvolveException : Exception
    {
        public EvolveException(string message) : base(message) { }

        public EvolveException(string message, Exception innerEx) : base($"{message} {innerEx.Message}" , innerEx) { }
    }
}
