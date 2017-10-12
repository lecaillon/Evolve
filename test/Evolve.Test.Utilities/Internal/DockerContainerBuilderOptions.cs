using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
