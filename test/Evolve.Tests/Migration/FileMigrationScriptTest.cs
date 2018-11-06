using System;
using System.IO;
using System.Security.Cryptography;
using Evolve.Migration;
using Xunit;

namespace Evolve.Tests.Migration
{
    public class FileMigrationScriptTest
    {
        [Fact]
        public void CalculateChecksum_should_not_return_null()
        {
            var script = new FileMigrationScript(TestContext.CrLfScriptPath, "2.3.1", "Migration description");
            string checksum = script.CalculateChecksum();
            Assert.False(string.IsNullOrEmpty(checksum));
        }

        [Fact]
        public void ValidateChecksum_should_work_with_both_crlf_and_lf_versions()
        {
            // Arrange
            var crlfScript = new FileMigrationScript(TestContext.CrLfScriptPath, "2.3.1", "Migration description");
            string lfCheckSum = new FileMigrationScript(TestContext.LfScriptPath, "2.3.2", "Migration description lf").CalculateChecksum();

            // Assert
            crlfScript.ValidateChecksum(lfCheckSum);
            Assert.NotEqual(File.ReadAllText(TestContext.CrLfScriptPath), File.ReadAllText(TestContext.LfScriptPath));
        }

        [Fact]
        public void ValidateChecksum_should_work_with_old_checksum_version()
        {
            // Arrange
            FileMigrationLoader loader = new FileMigrationLoader();

            // Assert
            foreach (FileMigrationScript script in loader.GetMigrations(new [] { "Resources/LF_CRLF" }, "V", "__", ".sql"))
            {
                script.ValidateChecksum(FallbackCheck(script.Path));
            }
        }

        [Fact]
        public void ValidateChecksum_throws_EvolveValidationException_when_checksums_mismatch()
        {
            // Arrange
            var crlfScript = new FileMigrationScript(TestContext.CrLfScriptPath, "2.3.1", "Migration description");

            // Assert
            Assert.Throws<EvolveValidationException>(() => crlfScript.ValidateChecksum("checksums mismatch"));
        }

        [Fact]
        public void CalculateChecksum_should_be_the_same_with_both_crlf_and_lf_versions()
        {
            // Arrange
            string crlfCheckSum = new FileMigrationScript(TestContext.CrLfScriptPath, "2.3.1", "Migration description").CalculateChecksum();
            string lfCheckSum = new FileMigrationScript(TestContext.LfScriptPath, "2.3.2", "Migration description lf").CalculateChecksum();

            // Assert
            Assert.Equal(crlfCheckSum, lfCheckSum);
            Assert.NotEqual(File.ReadAllText(TestContext.CrLfScriptPath), File.ReadAllText(TestContext.LfScriptPath));
        }

        /// <summary>
        ///     Calculate the checksum with the pre v1.8.0 version.
        /// </summary>
        private string FallbackCheck(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    byte[] checksum = md5.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
            }
        }
    }
}
