using Xunit;

namespace EvolveDb.Tests
{
    public sealed class FactSkippedOnAppVeyorOrLocalAttribute : FactAttribute
    {
        public FactSkippedOnAppVeyorOrLocalAttribute()
        {
            if (TestContext.AppVeyor || TestContext.Local)
            {
                Skip = "Test skipped on AppVeyor and Local.";
            }
        }
    }
}
