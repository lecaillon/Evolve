using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Evolve.Metadata;
using Evolve.Utilities;

namespace Evolve.Migration
{
    internal class FileMigrationScript : MigrationScript
    {
        private const string IncorrectMigrationType = "File migration {0} must be of type MetadataType.Migration or MetadataType.RepeatableMigration";

        public FileMigrationScript(string path, string? version, string description, MetadataType type, Encoding? encoding = null)
            : base(version,
                   description,
                   name: System.IO.Path.GetFileName(Check.FileExists(path, nameof(path))),
                   content: File.ReadAllText(path, encoding ?? Encoding.UTF8),
                   type == MetadataType.Migration || type == MetadataType.RepeatableMigration
                       ? type
                       : throw new NotSupportedException(string.Format(IncorrectMigrationType, System.IO.Path.GetFileName(path))))
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
        public override void ValidateChecksum(string? checksum)
        {
            Check.NotNull(checksum, nameof(checksum));

            try
            {
                base.ValidateChecksum(checksum);
            }
            catch
            {
                if (checksum != FallbackCheck())
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     Calculate the checksum with the pre v1.8.0 version.
        /// </summary>
        private string FallbackCheck()
        {
            using var md5 = MD5.Create();
            using FileStream stream = File.OpenRead(Path);
            byte[] checksum = md5.ComputeHash(stream);
            return BitConverter.ToString(checksum).Replace("-", string.Empty);
        }
    }
}
