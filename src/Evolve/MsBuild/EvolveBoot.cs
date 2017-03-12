#if NET

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
    ///         1- Change the MsBuild.exe current directory to the output folder.
    ///         2- Locate the application configuration file (app.config or web.config).
    ///         3- Copy sql migration files and folders to the output directory.
    ///         4- Run the Evolve command (migrate, clean...) defined in the configuration file.
    ///         5- Restore the original MsBuild.exe default directory.
    ///     </para>
    /// </summary>
    [LoadInSeparateAppDomain]
    [Serializable]
    public class EvolveBoot : AppDomainIsolatedTask
    {
        private const string MigrationFolderCopyError = "Evolve cannot copy the migration folders to the output directory.";

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
        ///     The directory of the primary output file for the build.
        /// </summary>
        public string TargetDir => Path.GetDirectoryName(TargetPath);

        /// <summary>
        ///     Full path to the App.config or Web.config
        /// </summary>
        public string EvolveConfigurationFile => TargetPath + ".config";

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

                var evolve = new Evolve(EvolveConfigurationFile, logInfoDelegate: msg => LogInfo(msg));
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
                    var targetDirectory = new DirectoryInfo(Path.Combine(TargetDir, sourceDirectory.Name));
                    CopyAll(sourceDirectory, targetDirectory);
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
            Log.LogErrorFromException(ex, true, false, "Evolve");
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

            //"  _____               _             "
            //" | ____|__   __ ___  | |__   __ ___ "
            //" |  _|  \ \ / // _ \ | |\ \ / // _ \"
            //" | |___  \ V /| (_) || | \ V /|  __/"
            //" |_____|  \_/  \___/ |_|  \_/  \___|"
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
