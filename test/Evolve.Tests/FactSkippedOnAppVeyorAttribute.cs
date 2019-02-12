using Xunit;

namespace Evolve.Tests
{
    public sealed class FactSkippedOnAppVeyorAttribute : FactAttribute
    {
        public FactSkippedOnAppVeyorAttribute()
        {
            if (TestContext.AppVeyor)
            {
                Skip = "Test skipped on AppVeyor.";
            }
        }
    }
}
