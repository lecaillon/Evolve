using System.Runtime.InteropServices;
using Xunit;

namespace Evolve.Cli.IntegrationTest
{
    public sealed class SkipOnLinuxFactAttribute : FactAttribute
    {
        public SkipOnLinuxFactAttribute()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Skip = "Test skipped on Linux";
            }
        }
    }
}
