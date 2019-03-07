using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
    public class FileMigrationScript : MigrationScript
    {
        private const string IncorrectMigrationChecksum = "Validate failed: invalid checksum for migration: {0}.";

        /// <summary>
        ///     Initialize a new versioned file migration
        /// </summary>
        public FileMigrationScript(string path, string version, string description, Encoding encoding = null)
            : base(version,
                   description,
                   name: System.IO.Path.GetFileName(Check.FileExists(path, nameof(path))),
                   content: File.ReadAllText(path, encoding ?? Encoding.UTF8),
                   type: MetadataType.Migration)
        {
            Path = path;
            Encoding = encoding ?? Encoding.UTF8;
        }

        /// <summary>
        ///     Initialize a new repeatable file migration
        /// </summary>
        public FileMigrationScript(string path, string description, Encoding encoding = null)
            : base(version: null,
                   description,
                   name: System.IO.Path.GetFileName(Check.FileExists(path, nameof(path))),
                   content: File.ReadAllText(path, encoding ?? Encoding.UTF8),
                   type: MetadataType.RepeatableMigration)
        {
            Path = path;
            Encoding = encoding ?? Encoding.UTF8;
        }

        public string Path { get; }

        public Encoding Encoding { get; }

        /// <summary>
        ///     Validates the given <paramref name="checksum"/> against the <see cref="MigrationScript"/>.
        ///     If the validation fails, use the pre v1.8.0 version of the method.
        /// </summary>
        /// <param name="checksum"> The given checksum. </param>
        /// <exception cref="Exception"> Throws when the validation fails. </exception>
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
