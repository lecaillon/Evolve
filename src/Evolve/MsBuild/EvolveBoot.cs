using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Collections.Generic;

namespace Evolve.MsBuild
{
    /// <summary>
    ///     <para>
    ///         Custom MsBuild Task that runs an Evolve command.
    ///     </para>
    ///     <para>
    ///         1- Change the current directory to the application output folder. (not usefull for NETCORE/core projects but keep it that way anyway)
    ///         2- Locate the application configuration file (app.config or evolve.json).
    ///         3- Copy sql migration files and folders to the output directory.
    ///         4- Run the Evolve command (migrate, clean...) defined in the configuration file.
    ///         5- Restore the original current directory.
    ///     </para>
    /// </summary>
#if NET
    [Serializable]
    [LoadInSeparateAppDomain]
    public class EvolveBoot : AppDomainIsolatedTask
#else
    [LoadInSeparateAppDomain]
    public class EvolveBoot : Task
#endif
    {
        private const string MigrationFolderCopyError = "Evolve cannot copy the migration folders to the output directory.";
        private const string MigrationFolderCopy = "Migration folder {0} copied to {1}.";
        private const string EvolveJsonConfigFileNotFound = "Evolve configuration file not found at {0}.";

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
        ///     True if the project to migrate targets netcoreapp or NETCORE, otherwise false.
        /// </summary>
        [Required]
        public bool IsDotNetStandardProject { get; set; }

        /// <summary>
        ///     The configuration that you are building, either "Debug" or "Release" or "Staging"...
        /// </summary>
        [Required]
        public string Configuration { get; set; }

        /// <summary>
        ///     The directory of the primary output file for the build.
        /// </summary>
        public string TargetDir => Path.GetDirectoryName(TargetPath);

        /// <summary>
        ///     Full path to the app.config or evolve.json
        /// </summary>
        /// <exception cref="EvolveConfigurationException"> When configuration file is not found. </exception>
        public string EvolveConfigurationFile
        {
            get
            {
                string configFile = null;

                if(IsDotNetStandardProject)
                {
                    configFile = Path.Combine(ProjectDir, "evolve.json");
                    if (!File.Exists(configFile))
                    {
                        throw new EvolveConfigurationException(string.Format(EvolveJsonConfigFileNotFound, configFile));
                    }
                }
                else
                {
                    configFile = TargetPath + ".config";
                }

                return configFile;
            }
        }

        /// <summary>
        ///     Full path to the deps file of the project.
        /// </summary>
        public string AppDepsFile => Path.Combine(Path.GetDirectoryName(TargetPath), Path.GetFileNameWithoutExtension(TargetPath) + ".deps.json");

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

                Directory.SetCurrentDirectory(TargetDir);
                Evolve evolve = null;
#if NETCORE
                evolve = new Evolve(EvolveConfigurationFile, AppDepsFile, NugetPackageDir, logInfoDelegate: msg => LogInfo(msg), environmentName: Configuration);
#else
    #if NET45
                if (IsDotNetStandardProject)
                {
                    evolve = new Evolve(EvolveConfigurationFile, AppDepsFile, NugetPackageDir, logInfoDelegate: msg => LogInfo(msg), environmentName: Configuration);
                }
    #endif
                if (!IsDotNetStandardProject)
                {
                    evolve = new Evolve(EvolveConfigurationFile, logInfoDelegate: msg => LogInfo(msg), environmentName: Configuration);
                }
#endif
                CopyMigrationProjectDirToTargetDir(evolve.Locations);

                evolve.ExecuteCommand();

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

        /// <summary>
        ///     <para>
        ///         Convienent method that try avoiding the forgettable user step 'always copy' on each sql migration file.
        ///     </para>
        ///     <para>
        ///         Copy sql migration folders and files to the output directory if the location is under the <see cref="ProjectDir"/> folder.
        ///     </para>
        /// </summary>
        /// <param name="locations"> List of the migration folders. </param>
        private void CopyMigrationProjectDirToTargetDir(IEnumerable<string> locations)
        {
            try
            {
                foreach (var location in locations)
                {
                    if (location.StartsWith(@"\")) continue;
                    if (location.StartsWith(@"/")) continue;
                    if (location.StartsWith(@".")) continue;
                    if (location.Contains(@":")) continue;

                    string sourcePath = Path.Combine(ProjectDir, location);
                    if (!Directory.Exists(sourcePath)) continue;

                    var sourceDirectory = new DirectoryInfo(sourcePath);
                    var targetDirectory = new DirectoryInfo(Path.Combine(TargetDir, location));

                    if (targetDirectory.Exists)
                    {
                        Directory.Delete(targetDirectory.FullName, true); // clean target folder
                    }

                    CopyAll(sourceDirectory, targetDirectory);
                    LogInfo(string.Format(MigrationFolderCopy, location, targetDirectory.FullName));
                }
            }
            catch (Exception ex)
            {
                throw new EvolveException(MigrationFolderCopyError, ex);
            }
        }

        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
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
