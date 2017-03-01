#if NET452

using Evolve.Migration;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Evolve.Configuration
{
    public class AppConfigConfigurationProvider : EvolveConfigurationProviderBase
    {
        private const string IncorrectFileFormat = "Incorrect file format: {0}.";
        private const string NotSupportedAppConfigFileName = "AppConfigConfigurationProvider is designed to manage App.Config or Web.config files only.";
        private const string IncorrectEncodingValue = "Encoding does not support this value: {0}. See https://msdn.microsoft.com/en-us/library/system.text.encoding.getencodings(v=vs.110).aspx for all possible names.";
        private const string InvalidVersionPatternMatching = "Migration version {0} is invalid. Version must respect this regex: ^[0-9]+(?:.[0-9]+)*$";

        protected override void Configure()
        {
            if(!(Path.GetFileName(_filePath).Equals("web.config", StringComparison.InvariantCultureIgnoreCase)
              || Path.GetFileName(_filePath).Equals("app.config", StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new EvolveConfigurationException(NotSupportedAppConfigFileName);
            }

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
            if (!string.IsNullOrWhiteSpace(appSettings[ConnectionString]?.Value))
            {
                string cnn = appSettings[ConnectionString].Value;
                if (!string.IsNullOrWhiteSpace(config.ConnectionStrings.ConnectionStrings[cnn]?.ConnectionString))
                {
                    _configuration.ConnectionString = config.ConnectionStrings.ConnectionStrings[cnn].ConnectionString;
                }
                else
                {
                    _configuration.ConnectionString = cnn;
                }
            }

            // Driver
            if (!string.IsNullOrWhiteSpace(appSettings[Driver]?.Value))
            {
                _configuration.Driver = appSettings[Driver].Value;
            }

            // Locations
            if (!string.IsNullOrWhiteSpace(appSettings[Locations]?.Value))
            {
                _configuration.Locations = appSettings[Locations].Value
                                                                 .Split(';')
                                                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                                                 .Distinct(StringComparer.OrdinalIgnoreCase)
                                                                 .ToList();
            }

            // Encoding
            if (!string.IsNullOrWhiteSpace(appSettings[Encoding]?.Value))
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
            if (!string.IsNullOrWhiteSpace(appSettings[SqlMigrationPrefix]?.Value))
            {
                _configuration.SqlMigrationPrefix = appSettings[SqlMigrationPrefix].Value;
            }

            // SqlMigrationSeparator
            if (!string.IsNullOrWhiteSpace(appSettings[SqlMigrationSeparator]?.Value))
            {
                _configuration.SqlMigrationSeparator = appSettings[SqlMigrationSeparator].Value;
            }

            // SqlMigrationSuffix
            if (!string.IsNullOrWhiteSpace(appSettings[SqlMigrationSuffix]?.Value))
            {
                _configuration.SqlMigrationSuffix = appSettings[SqlMigrationSuffix].Value;
            }

            // Schemas
            if (!string.IsNullOrWhiteSpace(appSettings[Schemas]?.Value))
            {
                _configuration.Schemas = appSettings[Schemas].Value
                                                             .Split(';')
                                                             .Where(s => !string.IsNullOrWhiteSpace(s))
                                                             .Distinct(StringComparer.OrdinalIgnoreCase)
                                                             .ToList();
            }

            // MetadaTableSchema
            if (!string.IsNullOrWhiteSpace(appSettings[MetadaTableSchema]?.Value))
            {
                _configuration.MetadataTableSchema = appSettings[MetadaTableSchema].Value;
            }

            // MetadaTableName
            if (!string.IsNullOrWhiteSpace(appSettings[MetadaTableName]?.Value))
            {
                _configuration.MetadaTableName = appSettings[MetadaTableName].Value;
            }

            // PlaceholderPrefix
            if (!string.IsNullOrWhiteSpace(appSettings[PlaceholderPrefix]?.Value))
            {
                _configuration.PlaceholderPrefix = appSettings[PlaceholderPrefix].Value;
            }

            // PlaceholderSuffix
            if (!string.IsNullOrWhiteSpace(appSettings[PlaceholderSuffix]?.Value))
            {
                _configuration.PlaceholderSuffix = appSettings[PlaceholderSuffix].Value;
            }

            // TargetVersion
            if (!string.IsNullOrWhiteSpace(appSettings[TargetVersion]?.Value))
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
