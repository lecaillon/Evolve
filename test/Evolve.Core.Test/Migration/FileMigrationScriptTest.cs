using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Evolve.Migration;
using Xunit;

namespace Evolve.Core.Test.Migration
{
    public class FileMigrationScriptTest
    {
        [Fact(DisplayName = "CalculateChecksum_should_not_return_null")]
        public void CalculateChecksum_should_not_return_null()
        {
            var script = new FileMigrationScript(TestContext.ValidMigrationScriptPath, "2.3.1", "Migration description");
            string checksum = script.CalculateChecksum();
            Assert.False(string.IsNullOrEmpty(checksum));
        }

        [Fact(DisplayName = "ValidateChecksum_should_work_with_both_crlf_and_lf_versions")]
        public void ValidateChecksum_should_work_with_both_crlf_and_lf_versions()
        {
            // Arrange
            var crlfScript = new FileMigrationScript(TestContext.ValidMigrationScriptPath, "2.3.1", "Migration description");
            string lfCheckSum = new FileMigrationScript(TestContext.ValidLfMigrationScriptPath, "1.3.2", "Migration description lf").CalculateChecksum();

            // Assert
            crlfScript.ValidateChecksum(lfCheckSum);
            Assert.NotEqual(File.ReadAllText(TestContext.ValidMigrationScriptPath), File.ReadAllText(TestContext.ValidLfMigrationScriptPath));
        }

        [Fact(DisplayName = "ValidateChecksum_should_work_with_old_checksum_version")]
        public void ValidateChecksum_should_work_with_old_checksum_version()
        {
            // Arrange
            FileMigrationLoader loader = new FileMigrationLoader();

            // Assert
            foreach (FileMigrationScript script in loader.GetMigrations(new List<string> { "Resources/Scripts_SQL_3" }, "V", "__", ".sql"))
            {
                script.ValidateChecksum(FallbackCheck(script.Path));
            }
        }

        [Fact(DisplayName = "ValidateChecksum_throws_EvolveValidationException_when_checksums_mismatch")]
        public void ValidateChecksum_throws_EvolveValidationException_when_checksums_mismatch()
        {
            // Arrange
            var crlfScript = new FileMigrationScript(TestContext.ValidMigrationScriptPath, "2.3.1", "Migration description");

            // Assert
            Assert.Throws<EvolveValidationException>(() => crlfScript.ValidateChecksum("checksums mismatch"));
        }

        [Fact(DisplayName = "CalculateChecksum_should_be_the_same_with_both_crlf_and_lf_versions")]
        public void CalculateChecksum_should_be_the_same_with_both_crlf_and_lf_versions()
        {
            // Arrange
            string crlfCheckSum = new FileMigrationScript(TestContext.ValidMigrationScriptPath, "2.3.1", "Migration description").CalculateChecksum();
            string lfCheckSum = new FileMigrationScript(TestContext.ValidLfMigrationScriptPath, "1.3.2", "Migration description lf").CalculateChecksum();

            // Assert
            Assert.Equal(crlfCheckSum, lfCheckSum);
            Assert.NotEqual(File.ReadAllText(TestContext.ValidMigrationScriptPath), File.ReadAllText(TestContext.ValidLfMigrationScriptPath));
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
