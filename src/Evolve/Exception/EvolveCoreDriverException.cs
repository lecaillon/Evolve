using System;

namespace Evolve
{
    public class EvolveCoreDriverException : EvolveException
    {
        public EvolveCoreDriverException() 
            : base() { }

        public EvolveCoreDriverException(string message) 
            : base(message) { }

        public EvolveCoreDriverException(string message, string driverDetails) 
            : base(message + Environment.NewLine + driverDetails) { }

        public EvolveCoreDriverException(string message, Exception innerEx) 
            : base($"{message} {innerEx.Message}", innerEx) { }

        public EvolveCoreDriverException(string message, string driverDetails, Exception innerEx) 
            : base(message + Environment.NewLine + driverDetails + innerEx.Message, innerEx) { }
    }
}
