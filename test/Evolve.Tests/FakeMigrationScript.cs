using Evolve.Metadata;
using Evolve.Migration;

namespace Evolve.Tests
{
    internal class FakeMigrationScript : MigrationScript
    {
        public FakeMigrationScript(string content)
            : base("1", "no desc", "no name", content, MetadataType.Migration)
        { }
    }
}
