using System;
using System.Diagnostics.CodeAnalysis;

namespace Evolve.MSBuild
{
    [SuppressMessage("Usage", "CA2237: Mark ISerializable types with serializable")]
    [SuppressMessage("Design", "CA1032: Implement standard exception constructors")]
    public class EvolveMSBuildException : Exception
    {
        public EvolveMSBuildException(string message) : base(message) { }
        public EvolveMSBuildException(string message, Exception innerException) : base(message, innerException) { }
    }
}
