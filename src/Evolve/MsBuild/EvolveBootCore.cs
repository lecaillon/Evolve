#if NETSTANDARD

using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Evolve.MsBuild
{
    public class EvolveBootCore : Task
    {
        /// <summary>
        ///     The absolute path name of the primary output file for the build.
        /// </summary>
        [Required]
        public string TargetPath { get; set; }

        /// <summary>
        ///     The directory of the project (includes the trailing backslash '\').
        /// </summary>
        [Required]
        public string ProjectDir { get; set; }

        /// <summary>
        ///     The directory of the Evolve NuGet package build folder (includes the trailing backslash '\').
        /// </summary>
        [Required]
        public string EvolveNugetPackageBuildDir { get; set; }

        /// <summary>
        ///     The directory of the primary output file for the build.
        /// </summary>
        public string TargetDir => Path.GetDirectoryName(TargetPath);

        /// <summary>
        ///     Full path to the deps file of the project.
        /// </summary>
        public string EvolveDepsFile => ProjectDir + Path.GetFileNameWithoutExtension(TargetPath) + ".deps.json";

        /// <summary>
        ///     The directory of the Nuget package repository.
        /// </summary>
        public string NugetPackageDir => Path.GetFullPath(Path.Combine(EvolveNugetPackageBuildDir, @"../../../.."));

        /// <summary>
        ///     Runs the task.
        /// </summary>
        /// <returns> true if successful; otherwise, false. </returns>
        public override bool Execute()
        {
            string originalCurrentDirectory = Directory.GetCurrentDirectory();

            try
            {
                WriteHeader();
                LogInfo($"TargetPath: {TargetPath}");
                LogInfo($"ProjectDir: {ProjectDir}");
                LogInfo($"EvolveNugetPackageBuildDir: {EvolveNugetPackageBuildDir}");
                LogInfo($"NugetPackageDir: {NugetPackageDir}");
                LogInfo($"EvolveDepsFile: {EvolveDepsFile}");

                var evolve = new Evolve(logInfoDelegate: msg => LogInfo(msg));
                evolve.ConnectionString = @"Server=127.0.0.1;Port=5432;Database=my_database;User Id=postgres;Password=postgres;";
                evolve.Driver = "npgsql";
                evolve.Erase();

                Directory.SetCurrentDirectory(TargetDir);

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

        #region Logger

        private void LogError(Exception ex)
        {
            Log.LogErrorFromException(ex, true, true, "Evolve");
        }

        private void LogInfo(string msg)
        {
            Log.LogMessage(MessageImportance.High, msg);
        }

        private void WriteHeader()
        {
            Log.LogMessage(MessageImportance.High, @"__________            ______            ");
            Log.LogMessage(MessageImportance.High, @"___  ____/__   __________  /__   ______ ");
            Log.LogMessage(MessageImportance.High, @"__  __/  __ | / /  __ \_  /__ | / /  _ \");
            Log.LogMessage(MessageImportance.High, @"_  /___  __ |/ // /_/ /  / __ |/ //  __/");
            Log.LogMessage(MessageImportance.High, @"/_____/  _____/ \____//_/  _____/ \___/ ");
        }

        private void WriteFooter()
        {
        }

        private void WriteNewLine()
        {
            Log.LogMessage(MessageImportance.High, string.Empty);
        }

        #endregion
    }
}

#endif
