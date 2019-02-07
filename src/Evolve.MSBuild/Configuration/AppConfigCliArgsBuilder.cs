#if NET35 || NET461

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Evolve.MSBuild
{
    public class AppConfigCliArgsBuilder : CliArgsBuilder
    {
        private System.Configuration.Configuration _config = null;
        private Dictionary<string, string> _datasource = null;


        public AppConfigCliArgsBuilder(string configFile, string env = null) 
            : base(configFile, env)
        {
            string cnn = _config.ConnectionStrings.ConnectionStrings[ConnectionString]?.ConnectionString;
            if (cnn != null)
            {
                ConnectionString = Normalize(cnn);
            }
        }

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
                        throw new EvolveMSBuildException(string.Format(IncorrectFileFormat, ConfigFile), ex);
                    }

                    var settings = _config.AppSettings.Settings;
                    _datasource = settings.AllKeys.ToDictionary(key => key, key => settings[key].Value, StringComparer.OrdinalIgnoreCase);
                }

                return _datasource;
            }
        }
    }
}

#else

using System;
using System.Collections.Generic;

namespace Evolve.MSBuild
{
    public class AppConfigCliArgsBuilder : CliArgsBuilder
    {
        public AppConfigCliArgsBuilder(string configFile, string env = null) 
            : base(configFile, env)
        {
            throw new NotSupportedException("This class supports .NET framework only.");
        }

        protected override Dictionary<string, string> Datasource => throw new NotSupportedException("This class supports .NET framework only.");
    }
}

#endif