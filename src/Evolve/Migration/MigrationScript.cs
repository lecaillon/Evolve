using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
    /// <summary>
    ///     Provide a common abstraction for versioned and repeatable migrations.
    /// </summary>
    internal abstract class MigrationScript : MigrationBase
    {
        private const string IncorrectMigrationChecksum = "Validate failed: invalid checksum for migration: {0}.";

        protected MigrationScript(string? version, string description, string name, string content, MetadataType type)
            : base(version, description, name, type)
        {
            Content = Check.NotNull(content, nameof(content));
        }

        /// <summary>
        ///     Gets the raw content of a migration script.
        /// </summary>
        public string Content { get; }

        /// <summary>
        ///     Returns false if the special comment "evolve-tx-off" is found in the first line of the script, true otherwise.
        /// </summary>
        public virtual bool IsTransactionEnabled => !OptionExists("evolve-tx-off");

        /// <summary>
        ///     Returns true if the special comment "evolve-repeat-always" is found in the first line of the script, false otherwise.
        /// </summary>
        public virtual bool MustRepeatAlways => Type == MetadataType.RepeatableMigration && OptionExists("evolve-repeat-always");

        /// <summary>
        ///     Validates the <paramref name="checksum"/> against the actual migration one.
        ///     Throws on mismatch.
        /// </summary>
        /// <param name="checksum"> The applied migration checksum. </param>
        /// <exception cref="EvolveValidationException"></exception>
        public virtual void ValidateChecksum(string? checksum)
        {
            Check.NotNull(checksum, nameof(checksum));

            if (checksum != CalculateChecksum())
            {
                throw new EvolveValidationException(string.Format(IncorrectMigrationChecksum, Name));
            }
        }

        /// <summary>
        ///     Returns the checksum where <code>crlf</code> and <code>lf</code> line endings have been previously normalized to <code>lf</code>.
        /// </summary>
        /// <returns></returns>
        public virtual string CalculateChecksum()
        {
            using var md5 = MD5.Create();
            byte[] checksum = md5.ComputeHash(Encoding.UTF8.GetBytes(NormalizeLineEndings(Content)));
            return BitConverter.ToString(checksum).Replace("-", string.Empty);
        }

        /// <summary>
        ///     <code>crlf</code> and <code>lf</code> line endings will be normalized to <code>lf</code>
        /// </summary>
        protected static string NormalizeLineEndings(string str) => str.Replace("\r\n", "\n");

        private bool OptionExists(string option)
        {
            if (Content.IsNullOrWhiteSpace())
            {
                return false;
            }

            using var file = new StringReader(Content);
            string? firstLine = file.ReadLine();
            return !(firstLine is null || firstLine.IndexOf(option, StringComparison.OrdinalIgnoreCase) == -1);
        }
    }
}
