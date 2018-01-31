using System;
using System.Collections.Generic;

namespace Evolve.Test.Utilities
{
    internal class DockerContainerBuilderOptions
    {
        public string FromImage { get; set; }
        public string Tag { get; set; }
        public string Name { get; set; }
        public IList<string> Env { get; set; }
        public string ExposedPort { get; set; }
        public string HostPort { get; set; }
        public TimeSpan? DelayAfterStartup { get; set; }
        public bool RemovePreviousContainer { get; set; }
    }
}
