using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
     public abstract class MigrationScript : MigrationBase, IMigrationScript
    {
        private readonly Func<TextReader> _migrationStream;

#if NET35
        private string _checkSum;
#else
        private readonly Lazy<string> _checkSum;
#endif

        protected MigrationScript(string version, string name, string description, Func<TextReader> migrationStream)
            : base(
                version,
                description,
                name,
                MetadataType.Migration)
        {
            _migrationStream = migrationStream;
#if !NET35
            _checkSum = new Lazy<string>(CalculateChecksum);
#endif
        }

#if NET35
        private readonly object _csLock = new object();

        public string CheckSum
        {
            get
            {
                if (_checkSum == null)
                {
                    lock (_csLock)
                    {
                        if (_checkSum == null)
                        {
                            _checkSum = CalculateChecksum();
                        }
                    }

                }
                return _checkSum;
            }
        }
#else
        public string CheckSum => _checkSum.Value;
#endif

        private string CalculateChecksum()
        {
            using (var md5 = MD5.Create())
            {
                byte[] checksum = md5.ComputeHash(
                    Encoding.UTF8.GetBytes(WithNormalizedLineEndings(StreamToString()))
                    );
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }

        private static string WithNormalizedLineEndings(string str)
        {
            return str.Replace("\r\n", "\n");
        }

        private string StreamToString()
        {
            using (var sqlStream = _migrationStream())
            {
                return sqlStream.ReadToEnd();
            }
        }

        public IEnumerable<string> LoadSqlStatements(IDictionary<string, string> placeholders, string delimiter)
        {
            if (placeholders == null || placeholders.Count == 0)
            {
                return MigrationUtil.SplitSqlStatements(StreamToString(), delimiter);
            }

            StringBuilder sql = new StringBuilder(StreamToString());
            foreach (var entry in placeholders)
            {
                sql.Replace(entry.Key, entry.Value);
            }
            return MigrationUtil.SplitSqlStatements(sql.ToString(), delimiter);
        }


    }

}
