using System;
using System.Collections.Generic;
using System.Linq;
using Evolve.Migration;
using Evolve.Utilities;

namespace Evolve.Configuration
{
    public abstract class EvolveConfigurationProviderBase : IConfigurationProvider
    {
        private const string ValueCannotBeNull = "Configuration parameter [{0}] cannot be null or empty. Update your Evolve configuration file at: {1}.";
        private const string IncorrectEncodingValue = "Encoding does not support this value: {0}. See https://msdn.microsoft.com/en-us/library/system.text.encoding.getencodings(v=vs.110).aspx for all possible names.";
        private const string InvalidVersionPatternMatching = "{0}: Migration version {1} is invalid. Version must respect this regex: ^[0-9]+(?:.[0-9]+)*$";

        protected IEvolveConfiguration _configuration;

        #region Configuration variables

        protected const string ConnectionString = "Evolve.ConnectionString";
        protected const string Driver = "Evolve.Driver";
        protected const string Locations = "Evolve.Locations";
        protected const string EraseDisabled = "Evolve.EraseDisabled";
        protected const string Command = "Evolve.Command";
        protected const string EraseOnValidationError = "Evolve.EraseOnValidationError";
        protected const string Encoding = "Evolve.Encoding";
        protected const string SqlMigrationPrefix = "Evolve.SqlMigrationPrefix";
        protected const string SqlMigrationSeparator = "Evolve.SqlMigrationSeparator";
        protected const string SqlMigrationSuffix = "Evolve.SqlMigrationSuffix";
        protected const string Schemas = "Evolve.Schemas";
        protected const string MetadataTableSchema = "Evolve.MetadataTableSchema";
        protected const string MetadataTableName = "Evolve.MetadataTableName";
        protected const string PlaceholderPrefix = "Evolve.PlaceholderPrefix";
        protected const string PlaceholderSuffix = "Evolve.PlaceholderSuffix";
        protected const string TargetVersion = "Evolve.TargetVersion";
        protected const string StartVersion = "Evolve.StartVersion";
        protected const string Placeholder = "Evolve.Placeholder.";

        #endregion

        public void Configure(string evolveConfigurationPath, IEvolveConfiguration configuration, string environmentName = null)
        {
            ConfigFile = Check.FileExists(evolveConfigurationPath, nameof(evolveConfigurationPath));
            _configuration = Check.NotNull(configuration, nameof(configuration));
            EnvironmentName = environmentName;

            Configure();
            Validate();
        }

        public string ConfigFile { get; private set; }

        public string EnvironmentName { get; private set; }

        protected abstract Dictionary<string, string> Datasource { get; }

        protected virtual void Configure()
        {
            // ConnectionString
            if (!ReadValue(ConnectionString).IsNullOrWhiteSpace())
            {
                _configuration.ConnectionString = ReadValue(ConnectionString);
            }

            // Driver
            if (!ReadValue(Driver).IsNullOrWhiteSpace())
            {
                _configuration.Driver = ReadValue(Driver);
            }

            // Command
            if (!ReadValue(Command).IsNullOrWhiteSpace())
            {
                try
                {
                    _configuration.Command = (CommandOptions)Enum.Parse(typeof(CommandOptions), ReadValue(Command).Trim(), true);
                }
                catch { }
            }

            // IsEraseDisabled
            if (!ReadValue(EraseDisabled).IsNullOrWhiteSpace())
            {
                try
                {
                    _configuration.IsEraseDisabled = Convert.ToBoolean(ReadValue(EraseDisabled));
                }
                catch { }
            }

            // EraseOnValidationError
            if (!ReadValue(EraseOnValidationError).IsNullOrWhiteSpace())
            {
                try
                {
                    _configuration.MustEraseOnValidationError = Convert.ToBoolean(ReadValue(EraseOnValidationError));
                }
                catch { }
            }

            // Locations
            if (!ReadValue(Locations).IsNullOrWhiteSpace())
            {
                _configuration.Locations = ReadValue(Locations).Split(';')
                                                               .Where(s => !s.IsNullOrWhiteSpace())
                                                               .Distinct(StringComparer.OrdinalIgnoreCase)
                                                               .ToList();
            }

            // Encoding
            if (!ReadValue(Encoding).IsNullOrWhiteSpace())
            {
                try
                {
                    _configuration.Encoding = System.Text.Encoding.GetEncoding(ReadValue(Encoding));
                }
                catch
                {
                    throw new EvolveConfigurationException(string.Format(IncorrectEncodingValue, ReadValue(Encoding)));
                }
            }

            // SqlMigrationPrefix
            if (!ReadValue(SqlMigrationPrefix).IsNullOrWhiteSpace())
            {
                _configuration.SqlMigrationPrefix = ReadValue(SqlMigrationPrefix);
            }

            // SqlMigrationSeparator
            if (!ReadValue(SqlMigrationSeparator).IsNullOrWhiteSpace())
            {
                _configuration.SqlMigrationSeparator = ReadValue(SqlMigrationSeparator);
            }

            // SqlMigrationSuffix
            if (!ReadValue(SqlMigrationSuffix).IsNullOrWhiteSpace())
            {
                _configuration.SqlMigrationSuffix = ReadValue(SqlMigrationSuffix);
            }

            // Schemas
            if (!ReadValue(Schemas).IsNullOrWhiteSpace())
            {
                _configuration.Schemas = ReadValue(Schemas).Split(';')
                                                           .Where(s => !s.IsNullOrWhiteSpace())
                                                           .Distinct(StringComparer.OrdinalIgnoreCase)
                                                           .ToList();
            }

            // MetadaTableSchema
            if (!ReadValue(MetadataTableSchema).IsNullOrWhiteSpace())
            {
                _configuration.MetadataTableSchema = ReadValue(MetadataTableSchema);
            }

            // MetadaTableName
            if (!ReadValue(MetadataTableName).IsNullOrWhiteSpace())
            {
                _configuration.MetadataTableName = ReadValue(MetadataTableName);
            }

            // PlaceholderPrefix
            if (!ReadValue(PlaceholderPrefix).IsNullOrWhiteSpace())
            {
                _configuration.PlaceholderPrefix = ReadValue(PlaceholderPrefix);
            }

            // PlaceholderSuffix
            if (!ReadValue(PlaceholderSuffix).IsNullOrWhiteSpace())
            {
                _configuration.PlaceholderSuffix = ReadValue(PlaceholderSuffix);
            }

            // TargetVersion
            if (!ReadValue(TargetVersion).IsNullOrWhiteSpace())
            {
                try
                {
                    _configuration.TargetVersion = new MigrationVersion(ReadValue(TargetVersion));
                }
                catch
                {
                    throw new EvolveConfigurationException(string.Format(InvalidVersionPatternMatching, nameof(TargetVersion), ReadValue(TargetVersion)));
                }
            }

            // StartVersion
            if (!ReadValue(StartVersion).IsNullOrWhiteSpace())
            {
                try
                {
                    _configuration.StartVersion = new MigrationVersion(ReadValue(StartVersion));
                }
                catch
                {
                    throw new EvolveConfigurationException(string.Format(InvalidVersionPatternMatching, nameof(StartVersion), ReadValue(StartVersion)));
                }
            }

            // Placeholder
            string prefix = _configuration.PlaceholderPrefix;
            string suffix = _configuration.PlaceholderSuffix;
            _configuration.Placeholders = new Dictionary<string, string>();
            _configuration.Placeholders = Datasource.Where(x => x.Key.StartsWith(Placeholder, StringComparison.OrdinalIgnoreCase))
                                                    .ToDictionary(x => x.Key.Replace(Placeholder, prefix) + suffix, x => x.Value);
        }

        protected virtual void Validate()
        {
            IfNullOrWhiteSpaceThrowsException(_configuration.SqlMigrationPrefix, SqlMigrationPrefix);
            IfNullOrWhiteSpaceThrowsException(_configuration.SqlMigrationSeparator, SqlMigrationSeparator);
            IfNullOrWhiteSpaceThrowsException(_configuration.SqlMigrationSuffix, SqlMigrationSuffix);
            IfNullOrWhiteSpaceThrowsException(_configuration.MetadataTableName, MetadataTableName);
            IfNullOrWhiteSpaceThrowsException(_configuration.PlaceholderPrefix, PlaceholderPrefix);
            IfNullOrWhiteSpaceThrowsException(_configuration.PlaceholderSuffix, PlaceholderSuffix);

            if (_configuration.Locations == null || _configuration.Locations.Count() == 0)
            {
                throw new EvolveConfigurationException(string.Format(ValueCannotBeNull, Locations, ConfigFile));
            }

            if (_configuration.Encoding == null)
            {
                throw new EvolveConfigurationException(string.Format(ValueCannotBeNull, Encoding, ConfigFile));
            }
        }

        /// <summary>
        ///     Read the value of a variable from the configuration file.
        /// </summary>
        /// <param name="key"> The name of the variable. </param>
        /// <returns> The value of the variable. </returns>
        protected string ReadValue(string key)
        {
            Check.NotNullOrEmpty(key, nameof(key));

            if (Datasource.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        private void IfNullOrWhiteSpaceThrowsException(string value, string name)
        {
            if (value.IsNullOrWhiteSpace())
            {
                throw new EvolveConfigurationException(string.Format(ValueCannotBeNull, name, ConfigFile));
            }
        }
    }
}
