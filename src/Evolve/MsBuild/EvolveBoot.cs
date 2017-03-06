#if NET

using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Evolve.MsBuild
{
    [LoadInSeparateAppDomain]
    [Serializable]
    public class EvolveBoot : AppDomainIsolatedTask
    {
        [Required]
        public string TargetPath { get; set; }
        public string TargetDir => Path.GetDirectoryName(TargetPath);
        public string EvolveConfigurationFile => TargetPath + ".config";

        public override bool Execute()
        {
            string originalCurrentDirectory = Directory.GetCurrentDirectory();

            try
            {
                WriteHeader();

                Directory.SetCurrentDirectory(TargetDir);

                var evolve = new Evolve(EvolveConfigurationFile);
                evolve.Migrate();

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
            finally
            {
                Directory.SetCurrentDirectory(originalCurrentDirectory);
                WriteFooter();
            }
        }

        private void LogError(Exception ex)
        {
            Log.LogErrorFromException(ex, true, false, "Evolve");
        }

        private void LogInfo(string msg)
        {
            Log.LogMessage(MessageImportance.High, msg);
        }

        private void WriteHeader()
        {
            Log.LogMessage(MessageImportance.High, @"  _____               _             ");
            Log.LogMessage(MessageImportance.High, @" | ____|__   __ ___  | |__   __ ___ ");
            Log.LogMessage(MessageImportance.High, @" |  _|  \ \ / // _ \ | |\ \ / // _ \");
            Log.LogMessage(MessageImportance.High, @" | |___  \ V /| (_) || | \ V /|  __/");
            Log.LogMessage(MessageImportance.High, @" |_____|  \_/  \___/ |_|  \_/  \___|");

            //"   _     _     _     _     _     _  "
            //"  / \   / \   / \   / \   / \   / \ "
            //" ( E ) ( v ) ( o ) ( l ) ( v ) ( e )"
            //"  \_/   \_/   \_/   \_/   \_/   \_/ "

            //" ____  ____  ____  ____  ____  ____ "
            //"||E ||||v ||||o ||||l ||||v ||||e ||"
            //"||__||||__||||__||||__||||__||||__||"
            //"|/__\||/__\||/__\||/__\||/__\||/__\|"

            //"__________            ______            "
            //"___  ____/__   __________  /__   ______ "
            //"__  __/  __ | / /  __ \_  /__ | / /  _ \"
            //"_  /___  __ |/ // /_/ /  / __ |/ //  __/"
            //"/_____/  _____/ \____//_/  _____/ \___/ "
        }

        private void WriteFooter()
        {

        }

        private void WriteNewLine()
        {
            Log.LogMessage(MessageImportance.High, string.Empty);
        }
    }
}

#endif
