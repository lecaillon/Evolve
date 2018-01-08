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
        private readonly Func<Stream> _migrationStream;
        private readonly bool _normalizeChecksum;
        private readonly Encoding _encoding;

#if NET35
        private string _checkSum;
#else
        private readonly Lazy<string> _checkSum;
#endif

        protected MigrationScript(string version, string name, string description, Func<Stream> migrationStream,Encoding textEncoding = null, bool normalizeChecksum = false)
            : base(
                version,
                description,
                name,
                MetadataType.Migration)
        {
            _encoding = textEncoding ?? Encoding.UTF8;
            _migrationStream = migrationStream;
            _normalizeChecksum = normalizeChecksum;
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
                byte[] checksum;
                if (_normalizeChecksum)
                {
                    checksum = md5.ComputeHash(Encoding.UTF8.GetBytes(WithNormalizedLineEndings(StreamToString())));
                }
                else using (var byteStream = _migrationStream())
                {
                    checksum = md5.ComputeHash(byteStream);
                }

                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }
        
        

        private static string WithNormalizedLineEndings(string str)
        {
            return str.Replace("\r\n", "\n");
        }

        private string StreamToString()
        {
            using (var byteStream = _migrationStream())        
            using (var sqlStream = new StreamReader(byteStream,_encoding))
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

            var sql = new StringBuilder(StreamToString());
            foreach (var entry in placeholders)
            {
                sql.Replace(entry.Key, entry.Value);
            }
            return MigrationUtil.SplitSqlStatements(sql.ToString(), delimiter);
        }


    }

}
