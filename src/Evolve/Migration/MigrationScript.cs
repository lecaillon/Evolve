using System.Collections.Generic;
using System.Text;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public abstract class MigrationScript : MigrationBase
    {
        private const string IncorrectMigrationChecksum = "Validate failed: invalid checksum for migration: {0}.";

        public MigrationScript(string version, string description, string name, Encoding textEncoding = null) 
            : base(version, description, name, MetadataType.Migration)
        {
            Encoding = textEncoding ?? Encoding.UTF8;
        }

        public Encoding Encoding { get; }
        
        /// <summary>
        ///     Validates the <paramref name="checksum"/> against the actual migration one.
        ///     Throws on mismatch.
        /// </summary>
        /// <param name="checksum"> The applied migration checksum. </param>
        /// <exception cref="EvolveValidationException"></exception>
        public virtual void ValidateChecksum(string checksum)
        {
            Check.NotNull(checksum, nameof(checksum));

            if (checksum != CalculateChecksum())
            {
                throw new EvolveValidationException(string.Format(IncorrectMigrationChecksum, Name));
            }
        }

        public abstract string CalculateChecksum();

        public abstract IEnumerable<string> LoadSqlStatements(Dictionary<string, string> placeholders, Encoding encoding, string delimiter);

        /// <summary>
        ///     <code>crlf</code> and <code>lf</code> line endings will be normalized to <code>lf</code>
        /// </summary>
        protected static string NormalizeLineEndings(string str) => str.Replace("\r\n", "\n");
    }
}
