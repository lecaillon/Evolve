using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Evolve.MSBuild
{
    /// <summary>
    ///     Custom MSBuild Task that runs an Evolve command.
    /// </summary>
    public class EvolveBoot : Task
    {
        private const string EvolveJsonConfigFileNotFound = "Evolve configuration file not found at {0}.";
        private const string MigrationFolderCopyError = "Evolve cannot copy the migration folders to the output directory.";
        private const string MigrationFolderCopy = "Migration folder {0} copied to {1}.";

        /// <summary>
        ///     The configuration that you are building, either "Debug" or "Release" or "Staging"...
        /// </summary>
        [Required]
        public string Configuration { get; set; }

        /// <summary>
        ///     True if the project to migrate targets netcoreapp or NETCORE, otherwise false.
        /// </summary>
        [Required]
        public bool IsDotNetCoreProject { get; set; }

        /// <summary>
        ///     The directory of the evolve.exe CLI.
        /// </summary>
        [Required]
        public string EvolveCliDir { get; set; }

        /// <summary>
        ///     The full path of the Windows Evolve CLI.
        /// </summary>
        public string EvolveCli => Path.Combine(EvolveCliDir, "evolve.exe");

        /// <summary>
        ///     The directory of the project (includes the trailing backslash '\').
        /// </summary>
        [Required]
        public string ProjectDir { get; set; }

        /// <summary>
        ///     The absolute path name of the primary output file for the build.
        /// </summary>
        [Required]
        public string TargetPath { get; set; }

        /// <summary>
        ///     The directory of the primary output file for the build.
        /// </summary>
        public string TargetDir => Path.GetDirectoryName(TargetPath);

        /// <summary>
        ///     Full path to the app.config or evolve.json
        /// </summary>
        /// <exception cref="EvolveConfigurationException"> When configuration file is not found. </exception>
        public string EvolveConfigurationFile => IsDotNetCoreProject ? FindJsonConfigurationFile() : TargetPath + ".config";


        /// <summary>
        ///     Runs the task.
        /// </summary>
        [SuppressMessage("Design", "CA1031: Do not catch general exception types")]
        public override bool Execute()
        {
            string originalCurrentDirectory = Directory.GetCurrentDirectory();

            try
            {
                WriteHeader();

                Directory.SetCurrentDirectory(TargetDir);
                var args = GetCliArgsBuilder();
                string cmdLineArgs = args.Build();
                if (string.IsNullOrEmpty(cmdLineArgs))
                {
                    return true;
                }

                CopyMigrationProjectDirToTargetDir(args.Locations);
                using var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = EvolveCli,
                        Arguments = cmdLineArgs,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };

                LogInfo(EvolveCli + " " + cmdLineArgs);
                proc.Start();
                proc.WaitForExit();
                LogInfo(proc.StandardOutput.ReadToEnd());

                string stderr = proc.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(stderr))
                {
                    Log.LogError(stderr);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogErrorFromException(ex);
                return false;
            }
            finally
            {
                Directory.SetCurrentDirectory(originalCurrentDirectory);
            }
        }

        /// <summary>
        ///     <para>
        ///         Convienent method to avoid the forgettable user step 'always copy' on each sql migration file.
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
                throw new EvolveMSBuildException(MigrationFolderCopyError, ex);
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

        /// <summary>
        ///     Returns the path of the json configuration file or throws ex if not found.
        /// </summary>
        /// <returns> Evolve.json or evolve.json configuration file. </returns>
        /// <exception cref="EvolveMSBuildException"> When configuration file is not found. </exception>
        private string FindJsonConfigurationFile()
        {
            string file = Path.Combine(ProjectDir, "Evolve.json");
            if (File.Exists(file))
            {
                return file;
            }
            file = Path.Combine(ProjectDir, "evolve.json");
            if (File.Exists(file))
            {
                return file;
            }

            throw new EvolveMSBuildException(string.Format(EvolveJsonConfigFileNotFound, file));
        }

        private CliArgsBuilder GetCliArgsBuilder() => Path.GetExtension(EvolveConfigurationFile) == ".json"
            ? new JsonCliArgsBuilder(EvolveConfigurationFile, Configuration) as CliArgsBuilder
            : new AppConfigCliArgsBuilder(EvolveConfigurationFile, Configuration);

        private void LogErrorFromException(Exception ex) => Log.LogErrorFromException(ex, true, true, "Evolve");

        private void LogInfo(string msg) => Log.LogMessage(MessageImportance.High, msg);

        private void WriteHeader()
        {
            Log.LogMessage(MessageImportance.High, @"__________            ______            ");
            Log.LogMessage(MessageImportance.High, @"___  ____/__   __________  /__   ______ ");
            Log.LogMessage(MessageImportance.High, @"__  __/  __ | / /  __ \_  /__ | / /  _ \");
            Log.LogMessage(MessageImportance.High, @"_  /___  __ |/ // /_/ /  / __ |/ //  __/");
            Log.LogMessage(MessageImportance.High, @"/_____/  _____/ \____//_/  _____/ \___/ ");
        }
    }
}
