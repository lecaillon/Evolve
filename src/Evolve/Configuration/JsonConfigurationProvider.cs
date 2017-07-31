#if NETCORE || NET45

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Evolve.Configuration
{
    public class JsonConfigurationProvider : EvolveConfigurationProviderBase
    {
        private Dictionary<string, string> _datasource = null;

        private const string IncorrectFileFormat = "Incorrect Evolve configuration file format at: {0}.";

        public string ConfigFile2
        {
            get
            {
                string fileName = Path.GetFileNameWithoutExtension(ConfigFile);
                string dir = Path.GetDirectoryName(ConfigFile);
                string ext = Path.GetExtension(ConfigFile);
                string configFile2 = Path.Combine(dir, $"{fileName}.{EnvironmentName}{ext}");

                return File.Exists(configFile2) ? configFile2 : null;
            }
        }

        protected override Dictionary<string, string> Datasource
        {
            get
            {
                if (_datasource != null)
                {
                    return _datasource;
                }

                try
                {
                    _datasource = LoadConfigurationFromFile(ConfigFile);
                }
                catch (Exception ex)
                {
                    throw new EvolveConfigurationException(string.Format(IncorrectFileFormat, ConfigFile), ex);
                }

                try
                {
                    if (ConfigFile2 != null)
                    {
                        var datasource = LoadConfigurationFromFile(ConfigFile2);
                        datasource.ToList().ForEach(x => _datasource[x.Key] = x.Value);
                    }
                }
                catch (Exception ex)
                {
                    throw new EvolveConfigurationException(string.Format(IncorrectFileFormat, ConfigFile2), ex);
                }

                return _datasource;
            }
        }

        private Dictionary<string, string> LoadConfigurationFromFile(string file)
        {
            using (StreamReader s = File.OpenText(file))
            {
                using (var r = new JsonTextReader(s))
                {
                    var jObject = (JObject)JToken.ReadFrom(r);
                    return jObject.Children<JProperty>()
                                  .Where(x => x.Name.StartsWith("Evolve.", StringComparison.OrdinalIgnoreCase))
                                  .Where(x => x.Value is JValue)
                                  .ToDictionary(x => x.Name, x => x.Value.ToString(), StringComparer.OrdinalIgnoreCase);
                }
            }
        }
    }
}

#endif