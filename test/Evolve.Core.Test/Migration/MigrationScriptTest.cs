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
            var script = new MigrationScript(TestContext.ValidMigrationScriptPath, "1.3.1", "Migration description");
            string checksum = script.CalculateChecksum();
            Assert.False(string.IsNullOrEmpty(checksum));
        }

        [Fact(DisplayName = "LoadSQL_returns_string_and_replace_placeholders")]
        public void LoadSQL_returns_string_and_replace_placeholders()
        {
            var script = new MigrationScript(TestContext.ValidMigrationScriptPath, "1.3.1", "Migration description");
            var placeholders = new Dictionary<string, string>
            {
                ["${schema}"] = "my_schema",
                ["${nothing}"] = "nil",
            };
            string sql = script.LoadSqlStatements(placeholders, Encoding.UTF8, null).First();
            Assert.False(sql.Contains("${schema}"));
            Assert.True(sql.Contains("my_schema"));
        }
    }
}
