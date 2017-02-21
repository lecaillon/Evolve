#if NET452

using System;
using System.Configuration;

namespace Evolve.Configuration
{
    public class AppConfigConfigurationProvider : EvolveConfigurationProviderBase
    {
        private const string NotSupportedAppConfigFileName = "AppConfigConfigurationProvider is designed to manage App.Config or Web.config files only.";
        private const string IncorrectEncodingValue = "Encoding does not support {0}. See https://msdn.microsoft.com/en-us/library/system.text.encoding.getencodings(v=vs.110).aspx for all possible values.";

        protected override void Configure()
        {
            if(!(_filePath.Equals("web.config", StringComparison.InvariantCultureIgnoreCase)
              || _filePath.Equals("app.config", StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new EvolveConfigurationException(NotSupportedAppConfigFileName);
            }

            var configMap = new ExeConfigurationFileMap()
            {
                ExeConfigFilename = _filePath
            };

            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var appSettings = config.AppSettings.Settings;

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

            if (!string.IsNullOrWhiteSpace(appSettings[Driver]?.Value))
            {
                _configuration.Driver = appSettings[Driver].Value;
            }

            if (!string.IsNullOrWhiteSpace(appSettings[Locations]?.Value))
            {
                _configuration.Locations = appSettings[Locations].Value.Split(';');
            }

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

            if (!string.IsNullOrWhiteSpace(appSettings[SqlMigrationPrefix]?.Value))
            {
                _configuration.SqlMigrationPrefix = appSettings[SqlMigrationPrefix].Value;
            }

            if (!string.IsNullOrWhiteSpace(appSettings[SqlMigrationSeparator]?.Value))
            {
                _configuration.SqlMigrationSeparator = appSettings[SqlMigrationSeparator].Value;
            }

            if (!string.IsNullOrWhiteSpace(appSettings[SqlMigrationSuffix]?.Value))
            {
                _configuration.SqlMigrationSuffix = appSettings[SqlMigrationSuffix].Value;
            }

            if (!string.IsNullOrWhiteSpace(appSettings[DefaultSchema]?.Value))
            {
                _configuration.DefaultSchema = appSettings[DefaultSchema].Value;
            }

            if (!string.IsNullOrWhiteSpace(appSettings[MetadaTableSchema]?.Value))
            {
                _configuration.MetadaTableSchema = appSettings[MetadaTableSchema].Value;
            }

            if (!string.IsNullOrWhiteSpace(appSettings[MetadaTableName]?.Value))
            {
                _configuration.MetadaTableName = appSettings[MetadaTableName].Value;
            }

            if (!string.IsNullOrWhiteSpace(appSettings[PlaceholderPrefix]?.Value))
            {
                _configuration.PlaceholderPrefix = appSettings[PlaceholderPrefix].Value;
            }

            if (!string.IsNullOrWhiteSpace(appSettings[PlaceholderSuffix]?.Value))
            {
                _configuration.PlaceholderSuffix = appSettings[PlaceholderSuffix].Value;
            }

            if (!string.IsNullOrWhiteSpace(appSettings[TargetVersion]?.Value))
            {
                _configuration.TargetVersion = appSettings[TargetVersion].Value;
            }
        }
    }
}

#endif
