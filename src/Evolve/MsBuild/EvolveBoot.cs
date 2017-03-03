#if NET

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
            Log.LogMessage(MessageImportance.High, "Start EvolveBoot task");
            Log.LogMessage(MessageImportance.High, TargetPath);

            Log.LogMessage(MessageImportance.High, "End EvolveBoot task");
            Log.LogMessage(MessageImportance.High, "");

            return true;
        }
    }
}

#endif

//<UsingTask TaskName = "Evolve.MsBuild.EvolveBoot" AssemblyFile="..\..\src\Evolve\bin\Debug\netstandard1.3\Evolve.dll" />
//<Target Name = "AfterBuild" >
//  < EvolveBoot TargetPath="$(TargetPath)" />
//</Target>

// Sql_Scripts

// Use Directory.SetCurrentDirectory($TargetDir); to mimic the comportment of the default 
// FileMigrationLoader new DirectoryInfo(@"c:\test").FullName wheen launching from MsBuild