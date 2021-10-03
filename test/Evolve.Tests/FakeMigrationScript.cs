using EvolveDb.Metadata;
using EvolveDb.Migration;

namespace EvolveDb.Tests
{
    internal class FakeMigrationScript : MigrationScript
    {
        public FakeMigrationScript(string content)
            : base("1", "no desc", "no name", content, MetadataType.Migration)
        { }
    }
}
