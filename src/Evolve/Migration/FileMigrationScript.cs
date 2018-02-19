using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public class FileMigrationScript : MigrationScript
    {
        private const string IncorrectMigrationChecksum = "Validate failed: invalid checksum for migration: {0}.";

        public FileMigrationScript(string path, string version, string description, Encoding textEncoding = null)
            : base(version,
                   description,
                   System.IO.Path.GetFileName(Check.FileExists(path, nameof(path))),
                   textEncoding)
        {
            Path = path;
        }

        public string Path { get; set; }

        public override IEnumerable<string> LoadSqlStatements(Dictionary<string, string> placeholders, Encoding encoding, string delimiter)
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

        public override void ValidateChecksum(string checksum)
        {
            Check.NotNull(checksum, nameof(checksum));

            try
            {
                base.ValidateChecksum(checksum);
            }
            catch (Exception ex)
            {
                if (checksum != FallbackCheck())
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        ///     Returns the checksum where <code>crlf</code> and <code>lf</code> line endings have been previously normalized to <code>lf</code>.
        /// </summary>
        /// <returns></returns>
        public override string CalculateChecksum()
        {
            using (var md5 = MD5.Create())
            {
                byte[] checksum = md5.ComputeHash(Encoding.UTF8.GetBytes(NormalizeLineEndings(File.ReadAllText(Path, Encoding))));
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }

        /// <summary>
        ///     Calculate the checksum with the pre v1.8.0 version.
        /// </summary>
        private string FallbackCheck()
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
    }
}
