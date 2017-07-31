#if NET

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Evolve.Configuration
{
    public class AppConfigConfigurationProvider : EvolveConfigurationProviderBase
    {
        private const string IncorrectFileFormat = "Incorrect file format at: {0}.";

        private System.Configuration.Configuration _config = null;
        private Dictionary<string, string> _datasource = null;

        protected override Dictionary<string, string> Datasource
        {
            get
            {
                if (_datasource == null)
                {
                    var configMap = new ExeConfigurationFileMap()
                    {
                        ExeConfigFilename = ConfigFile
                    };

                    try
                    {
                        _config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                    }
                    catch (Exception ex)
                    {
                        throw new EvolveConfigurationException(string.Format(IncorrectFileFormat, ConfigFile), ex);
                    }

                    var settings = _config.AppSettings.Settings;
                    _datasource = settings.AllKeys.ToDictionary(key => key, key => settings[key].Value, StringComparer.OrdinalIgnoreCase);
                }

                return _datasource;
            }
        }

        protected override void Configure()
        {
            base.Configure();

            // ConnectionString
            if (!ReadValue(ConnectionString).IsNullOrWhiteSpace())
            {
                string cnn = ReadValue(ConnectionString);
                if (!_config.ConnectionStrings.ConnectionStrings[cnn]?.ConnectionString.IsNullOrWhiteSpace() ?? false)
                {
                    _configuration.ConnectionString = _config.ConnectionStrings.ConnectionStrings[cnn].ConnectionString;
                }
                else
                {
                    _configuration.ConnectionString = cnn;
                }
            }
        }
    }
}

#endif
