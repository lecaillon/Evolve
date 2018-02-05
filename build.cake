///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var solutionFile = GetFiles("./*.sln").First();
var distDir = Directory("./dist");
var version = XmlPeek(File("./build/common.props"), "/Project/PropertyGroup/PackageVersion/text()");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx => { Information($"Building Evolve {version}"); });

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Restore").Does(() =>
{
    if(IsRunningOnWindows()) NuGetRestore(solutionFile);
    DotNetCoreRestore(solutionFile.ToString());
});

Task("Build").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    MSBuild(solutionFile, settings => settings.SetConfiguration(configuration)
                                              .SetVerbosity(Verbosity.Minimal));
});

Task("Test .NET").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    foreach(var project in GetFiles("./test/**/Evolve.*Test*.csproj").Where(x => !x.GetFilename().FullPath.Contains("Core"))
                                                                     .Where(x => !x.GetFilename().FullPath.Contains("Utilities"))
                                                                     .OrderBy(x => x.GetFilename().ToString())) // integrationTest first !
    {
        DotNetCoreTest(project.FullPath, new DotNetCoreTestSettings 
        {
            Configuration = configuration,
            NoBuild = true,
            ArgumentCustomization = args => args.Append($"--no-restore --filter \"Category!=Standalone\""),
        });

        DotNetCoreTest(project.FullPath, new DotNetCoreTestSettings 
        {
            Configuration = configuration,
            NoBuild = true,
            ArgumentCustomization = args => args.Append($"--no-restore --filter \"Category=Standalone\""), // standalone tests !
        });
    }
});

Task("Test .NET Core").Does(() =>
{
    foreach(var project in GetFiles("./test/**/Evolve.Core*.Test*.csproj").Where(x => !x.GetFilename().FullPath.Contains("Resources")))
    {
        DotNetCoreTest(project.FullPath, new DotNetCoreTestSettings 
        {
            Configuration = configuration,
            NoBuild = IsRunningOnWindows(),
            ArgumentCustomization = args => args.Append($"--no-restore --filter \"Category!=Standalone\""),
        });
    }
});

Task("Pack").WithCriteria(() => IsRunningOnWindows()).Does(() => 
{
    NuGetPack("./src/Evolve/Evolve.nuspec", new NuGetPackSettings
    {
        OutputDirectory = distDir,
        Version = version,
        Properties = new Dictionary<string, string> 
        {
            { "Configuration", configuration }
        }
    });
});

Task("Default")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test .NET")
    .IsDependentOn("Test .NET Core")
    .IsDependentOn("Pack");

RunTarget(target);
