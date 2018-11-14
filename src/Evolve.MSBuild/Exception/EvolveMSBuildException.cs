using System;

namespace Evolve.MSBuild
{
    public class EvolveMSBuildException : Exception
    {
        public EvolveMSBuildException(string message) : base(message) { }
        public EvolveMSBuildException(string message, Exception innerException) : base(message, innerException) { }
    }
}
