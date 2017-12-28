using System.Collections.Generic;
using System.Linq;
using System.Text;
using Evolve.Migration;
using Xunit;

namespace Evolve.Core.Test.Migration
{
    public class MigrationScriptTest
    {
        [Fact(DisplayName = "CalculateChecksum_should_not_return_null")]
        public void CalculateChecksum_should_not_return_null()
        {
            var script = new FileMigrationScript(TestContext.ValidMigrationScriptPath, "1.3.1", "Migration description");
            string checksum = script.CheckSum;
            Assert.False(string.IsNullOrEmpty(checksum));
        }

        [Fact(DisplayName = "LoadSQL_returns_string_and_replace_placeholders")]
        public void LoadSQL_returns_string_and_replace_placeholders()
        {
            var script = new FileMigrationScript(TestContext.ValidMigrationScriptPath, "1.3.1", "Migration description");
            var placeholders = new Dictionary<string, string>
            {
                ["${schema}"] = "my_schema",
                ["${nothing}"] = "nil",
            };
            string sql = script.LoadSqlStatements(placeholders,null).First();
            Assert.DoesNotContain("${schema}", sql);
            Assert.Contains("my_schema", sql);
        }
    }
}
