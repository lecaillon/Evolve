using System;
using System.Configuration;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Evolve.MsBuild
{
    public class EvolveBoot : Task
    {
        public string TargetPath { get; set; }

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "");
            Log.LogMessage(MessageImportance.High, "Start Task");
            Log.LogMessage(MessageImportance.High, TargetPath);

            string configPath = Directory.GetFiles(Path.GetDirectoryName(TargetPath), "*.config")[0];
            Log.LogMessage(MessageImportance.High, configPath);

            // http://stackoverflow.com/questions/8320543/cant-cast-to-my-custom-configurationsection
            var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = configPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            Log.LogMessage(MessageImportance.High, config.AppSettings.Settings["Test"].Value);

            Log.LogMessage(MessageImportance.High, "End Task");
            Log.LogMessage(MessageImportance.High, "");

            return true;
        }

    }
}
