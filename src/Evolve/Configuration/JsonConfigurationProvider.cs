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

        protected override Dictionary<string, string> Datasource
        {
            get
            {
                if (_datasource == null)
                {
                    try
                    {
                        using (StreamReader file = File.OpenText(_filePath))
                        {
                            using (var reader = new JsonTextReader(file))
                            {
                                var jObject = (JObject)JToken.ReadFrom(reader);
                                _datasource = jObject.Children<JProperty>()
                                                     .Where(x => x.Name.StartsWith("Evolve.", StringComparison.OrdinalIgnoreCase))
                                                     .Where(x => x.Value is JValue)
                                                     .ToDictionary(x => x.Name, x => x.Value.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new EvolveConfigurationException(string.Format(IncorrectFileFormat, _filePath), ex);
                    }
                }

                return _datasource;
            }
        }
    }
}

#endif