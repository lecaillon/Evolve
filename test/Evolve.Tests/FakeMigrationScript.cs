using Evolve.Migration;

namespace Evolve.Tests
{
    public class FakeMigrationScript : MigrationScript
    {
        public FakeMigrationScript(string content)
            : base("1", "no description", "no name", content)
        { }
    }
}
