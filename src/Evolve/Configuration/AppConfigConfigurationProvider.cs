#if NET

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Evolve.Migration;

namespace Evolve.Configuration
{
    public class AppConfigConfigurationProvider : EvolveConfigurationProviderBase
    {
        private const string IncorrectFileFormat = "Incorrect file format: {0}.";
        private const string IncorrectEncodingValue = "Encoding does not support this value: {0}. See https://msdn.microsoft.com/en-us/library/system.text.encoding.getencodings(v=vs.110).aspx for all possible names.";
        private const string InvalidVersionPatternMatching = "Migration version {0} is invalid. Version must respect this regex: ^[0-9]+(?:.[0-9]+)*$";

        protected override void Configure()
        {
            var configMap = new ExeConfigurationFileMap()
            {
                ExeConfigFilename = _filePath
            };

            System.Configuration.Configuration config = null;
            try
            {
                config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            }
            catch (Exception ex)
            {
                throw new EvolveConfigurationException(string.Format(IncorrectFileFormat, _filePath), ex);
            }

            var appSettings = config.AppSettings.Settings;

            // ConnectionString
            if (!appSettings[ConnectionString]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                string cnn = appSettings[ConnectionString].Value;
                if (!config.ConnectionStrings.ConnectionStrings[cnn]?.ConnectionString.IsNullOrWhiteSpace() ?? false)
                {
                    _configuration.ConnectionString = config.ConnectionStrings.ConnectionStrings[cnn].ConnectionString;
                }
                else
                {
                    _configuration.ConnectionString = cnn;
                }
            }

            // Driver
            if (!appSettings[Driver]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                _configuration.Driver = appSettings[Driver].Value;
            }

            // IsEraseDisabled
            if (!appSettings[EraseDisabled]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                try
                {
                    _configuration.IsEraseDisabled = Convert.ToBoolean(appSettings[EraseDisabled].Value);
                }
                catch { }
            }

            // Erase
            if (!appSettings[Erase]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                try
                {
                    _configuration.MustErase = Convert.ToBoolean(appSettings[Erase].Value);
                }
                catch { }
            }

            // EraseOnValidationError
            if (!appSettings[EraseOnValidationError]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                try
                {
                    _configuration.MustEraseOnValidationError = Convert.ToBoolean(appSettings[EraseOnValidationError].Value);
                }
                catch { }
            }

            // Repair
            if (!appSettings[Repair]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                try
                {
                    _configuration.MustRepair = Convert.ToBoolean(appSettings[Repair].Value);
                }
                catch { }
            }

            // Locations
            if (!appSettings[Locations]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                _configuration.Locations = appSettings[Locations].Value
                                                                 .Split(';')
                                                                 .Where(s => !s.IsNullOrWhiteSpace())
                                                                 .Distinct(StringComparer.OrdinalIgnoreCase)
                                                                 .ToList();
            }

            // Encoding
            if (!appSettings[Encoding]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                try
                {
                    _configuration.Encoding = System.Text.Encoding.GetEncoding(appSettings[Encoding].Value);
                }
                catch
                {
                    throw new EvolveConfigurationException(string.Format(IncorrectEncodingValue, appSettings[Encoding].Value));
                }
            }

            // SqlMigrationPrefix
            if (!appSettings[SqlMigrationPrefix]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                _configuration.SqlMigrationPrefix = appSettings[SqlMigrationPrefix].Value;
            }

            // SqlMigrationSeparator
            if (!appSettings[SqlMigrationSeparator]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                _configuration.SqlMigrationSeparator = appSettings[SqlMigrationSeparator].Value;
            }

            // SqlMigrationSuffix
            if (!appSettings[SqlMigrationSuffix]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                _configuration.SqlMigrationSuffix = appSettings[SqlMigrationSuffix].Value;
            }

            // Schemas
            if (!appSettings[Schemas]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                _configuration.Schemas = appSettings[Schemas].Value
                                                             .Split(';')
                                                             .Where(s => !s.IsNullOrWhiteSpace())
                                                             .Distinct(StringComparer.OrdinalIgnoreCase)
                                                             .ToList();
            }

            // MetadaTableSchema
            if (!appSettings[MetadaTableSchema]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                _configuration.MetadataTableSchema = appSettings[MetadaTableSchema].Value;
            }

            // MetadaTableName
            if (!appSettings[MetadaTableName]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                _configuration.MetadaTableName = appSettings[MetadaTableName].Value;
            }

            // PlaceholderPrefix
            if (!appSettings[PlaceholderPrefix]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                _configuration.PlaceholderPrefix = appSettings[PlaceholderPrefix].Value;
            }

            // PlaceholderSuffix
            if (!appSettings[PlaceholderSuffix]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                _configuration.PlaceholderSuffix = appSettings[PlaceholderSuffix].Value;
            }

            // TargetVersion
            if (!appSettings[TargetVersion]?.Value.IsNullOrWhiteSpace() ?? false)
            {
                try
                {
                    _configuration.TargetVersion = new MigrationVersion(appSettings[TargetVersion].Value);
                }
                catch
                {
                    throw new EvolveConfigurationException(string.Format(InvalidVersionPatternMatching, appSettings[TargetVersion].Value));
                }
            }

            // Placeholder
            string prefix = _configuration.PlaceholderPrefix;
            string suffix = _configuration.PlaceholderSuffix;
            _configuration.Placeholders = new Dictionary<string, string>();
            appSettings.AllKeys
                       .Where(x => x.StartsWith(Placeholder, StringComparison.OrdinalIgnoreCase))
                       .ToList()
                       .ForEach(k => _configuration.Placeholders.Add(k.Replace(Placeholder, prefix) + suffix, appSettings[k].Value));
        }
    }
}

#endif
