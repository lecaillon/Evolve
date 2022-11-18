using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using EvolveDb.Metadata;
using EvolveDb.Utilities;

namespace EvolveDb.Migration
{
    /// <summary>
    ///     Provide a common abstraction for versioned and repeatable migrations.
    /// </summary>
    public abstract class MigrationScript : MigrationBase
    {
        private const string IncorrectMigrationChecksum = "Validate failed: invalid checksum for migration: {0}.";
        private const string OptionTransactionOff = "evolve-tx-off";
        private const string OptionAlwayRepeat = "evolve-repeat-always";
        private const string OptionRepeatableDependencies = "evolve-repeatable-deps";
        private bool _optionsAlreadyFetched = false;
        private bool _isTransactionEnabled = true;
        private bool _isAlwayRepeat = false;
        private IEnumerable<string> _repeatableDeps = Enumerable.Empty<string>();

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
        public virtual bool IsTransactionEnabled
        {
            get
            {
                FetchOptions();
                return _isTransactionEnabled;
            }
        }

        /// <summary>
        ///     Returns true if the special comment "evolve-repeat-always" is found in the first line of the script, false otherwise.
        /// </summary>
        public virtual bool MustRepeatAlways
        {
            get
            {
                FetchOptions();
                return _isAlwayRepeat;
            }
        }

        /// <summary>
        ///     Enumerable of repeatable dependencies.
        /// </summary>
        public virtual IEnumerable<string> RepeatableDependencies
        {
            get
            {
                FetchOptions();
                return _repeatableDeps;
            }
        }

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

        private void FetchOptions()
        {
            if (_optionsAlreadyFetched)
            {
                return;
            }
            if (Content.IsNullOrWhiteSpace())
            {
                _optionsAlreadyFetched = true;
                return;
            }
            using var file = new StringReader(Content);
            while (file.ReadLine() is string line
                && !line.IsNullOrWhiteSpace()
                && line.IndexOf("evolve") >= 0)
            {
                if (line.IndexOf(OptionTransactionOff, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _isTransactionEnabled = false;
                }
                if (Type == MetadataType.RepeatableMigration
                    && line.IndexOf(OptionAlwayRepeat, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _isAlwayRepeat = true;
                }
                if (Type == MetadataType.RepeatableMigration
                    && line.IndexOf(OptionRepeatableDependencies) >= 0)
                {
                    _repeatableDeps = DependencyHelper
                        .Get(line, OptionRepeatableDependencies)
                        .Where(item => !item.IsNullOrWhiteSpace())
                        .Select(item => item.Trim())
                        .ToList();
                }
            }
        }
    }
}
