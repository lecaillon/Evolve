using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public class MigrationScript : MigrationBase
    {
        public MigrationScript(string path, string version, string description) 
            : base(version, description, System.IO.Path.GetFileName(Check.FileExists(path, nameof(path))), MetadataType.Migration)
        {
            Path = path;
        }

        public string Path { get; set; }

        public string CalculateChecksum()
        {
            using (var md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(Path))
                {
                    byte[] checksum = md5.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
            }
        }

        public IEnumerable<string> LoadSqlStatements(Dictionary<string, string> placeholders, Encoding encoding, string delimiter)
        {
            Check.NotNull(placeholders, nameof(placeholders));
            Check.NotNull(encoding, nameof(encoding));

            string sql = File.ReadAllText(Path, encoding);
            foreach (var entry in placeholders)
            {
                sql = sql.Replace(entry.Key, entry.Value);
            }

            return MigrationUtil.SplitSqlStatements(sql, delimiter);
        }
    }
}
