using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Evolve.MsBuild
{
    public class EvolveBoot : Task
    {
        public string ProjectOutputPath { get; set; }

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "PSG !!!");
            Log.LogMessage(MessageImportance.High, ProjectOutputPath);

            return true;
        }
    }
}
