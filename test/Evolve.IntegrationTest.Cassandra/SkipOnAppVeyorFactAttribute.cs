using Xunit;

namespace Evolve.IntegrationTest.Cassandra
{
    public sealed class SkipOnAppVeyorFactAttribute : FactAttribute
    {
        public SkipOnAppVeyorFactAttribute()
        {
            if (TestContext.AppVeyor)
            {
                Skip = "Test skipped on AppVeyor";
            }
        }
    }
}
