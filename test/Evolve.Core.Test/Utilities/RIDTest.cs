using Evolve.Utilities;
using Xunit;

namespace Evolve.Core.Test.Utilities
{
    public class RIDTest
    {
        [Theory(DisplayName = "RID_ToString_must_extract_metadata")]
        [InlineData((string)null, "RID: [] OS: [], Version: [], Architecture: []")]
        [InlineData("", "RID: [] OS: [], Version: [], Architecture: []")]
        [InlineData("win", "RID: [win] OS: [win], Version: [], Architecture: []")]
        [InlineData("win7", "RID: [win7] OS: [win7], Version: [], Architecture: []")]
        [InlineData("win-x86", "RID: [win-x86] OS: [win], Version: [], Architecture: [x86]")]
        [InlineData("win10-arm64", "RID: [win10-arm64] OS: [win10], Version: [], Architecture: [arm64]")]
        [InlineData("win10-arm64-blah", "RID: [win10-arm64-blah] OS: [win10], Version: [], Architecture: [arm64]")]
        [InlineData("centos.7-x64", "RID: [centos.7-x64] OS: [centos], Version: [7], Architecture: [x64]")]
        [InlineData("fedora.24-x64", "RID: [fedora.24-x64] OS: [fedora], Version: [24], Architecture: [x64]")]
        [InlineData("opensuse.42.1-x64", "RID: [opensuse.42.1-x64] OS: [opensuse], Version: [42.1], Architecture: [x64]")]
        [InlineData("osx.10.12-x64", "RID: [osx.10.12-x64] OS: [osx], Version: [10.12], Architecture: [x64]")]
        [InlineData("osx.10.12-x64-blah", "RID: [osx.10.12-x64-blah] OS: [osx], Version: [10.12], Architecture: [x64]")]
        public void RID_ToString_must_extract_metadata(string rid, string result)
        {
            Assert.Equal(result, new RID(rid).ToString());
        }
    }
}
