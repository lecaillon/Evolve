#if NET452

using System;
using System.Configuration;

namespace Evolve.Configuration
{
    public class AppConfigConfigurationProvider : EvolveConfigurationProviderBase
    {
        private const string NotSupportedAppConfigFileName = "AppConfigConfigurationProvider is designed to manage App.Config or Web.config files only.";

        protected override void Configure()
        {
            if(!(_configurationPath.Equals("web.config", StringComparison.InvariantCultureIgnoreCase)
              || _configurationPath.Equals("app.config", StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new EvolveConfigurationException(NotSupportedAppConfigFileName);
            }

            var configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = _configurationPath;
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            _configuration.SqlMigrationPrefix = config.AppSettings.Settings[Key_SqlMigrationPrefix]?.Value ?? _configuration.SqlMigrationPrefix;


            //var loc = config.AppSettings.Settings["Evolve.Locations"]?.Value?.Split(';');

            //string s = config.AppSettings.Settings["Evolve.SqlMigrationPrefix"]?.Value;
            //Encoding sss = Encoding.GetEncoding(config.AppSettings.Settings["Evolve.Encoding"]?.Value);

            //string cnn = config.ConnectionStrings.ConnectionStrings["Pro!d"]?.ConnectionString;

        }
    }


}

#endif
