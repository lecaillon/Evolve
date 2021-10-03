using System.Collections.Generic;

namespace EvolveDb.Tests.Infrastructure
{
    internal class DockerContainerBuilderOptions
    {
        public string FromImage { get; set; }
        public string Tag { get; set; }
        public string Name { get; set; }
        public IList<string> Env { get; set; }
        public IList<string> Cmd { get; set; }
        public string ExposedPort { get; set; }
        public string HostPort { get; set; }
        public bool RemovePreviousContainer { get; set; }
    }
}
