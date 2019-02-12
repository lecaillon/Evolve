using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinyJson;

namespace Evolve.MSBuild
{
    public class JsonCliArgsBuilder : CliArgsBuilder
    {
        private Dictionary<string, string> _datasource = null;

        public JsonCliArgsBuilder(string configFile, string env = null) 
            : base(configFile, env)
        {
        }

        public string ConfigFile2
        {
            get
            {
                string fileName = Path.GetFileNameWithoutExtension(ConfigFile);
                string dir = Path.GetDirectoryName(ConfigFile);
                string ext = Path.GetExtension(ConfigFile);
                string configFile2 = Path.Combine(dir, $"{fileName}.{Env}{ext}");

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
                    throw new EvolveMSBuildException(string.Format(IncorrectFileFormat, ConfigFile), ex);
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
                    throw new EvolveMSBuildException(string.Format(IncorrectFileFormat, ConfigFile2), ex);
                }

                return _datasource;
            }
        }

        private Dictionary<string, string> LoadConfigurationFromFile(string file)
        {
            return File.ReadAllText(file)
                       .FromJson<Dictionary<string, object>>()
                       .Where(kv => kv.Key.StartsWith("Evolve.", StringComparison.OrdinalIgnoreCase))
                       .ToDictionary(x => x.Key, x => x.Value.ToString(), StringComparer.OrdinalIgnoreCase);
        }
    }
}
