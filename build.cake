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

Setup(ctx => { Information($"Building Evolve {version} ..."); });

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Restore").Does(() =>
{
    NuGetRestore(solutionFile);
    DotNetCoreRestore();
});

Task("Build").IsDependentOn("Restore").Does(() =>
{
    MSBuild(solutionFile, settings => settings.SetConfiguration(configuration)
                                              .SetVerbosity(Verbosity.Minimal));
});

Task("Test .NET")
    .IsDependentOn("Build")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
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
            ArgumentCustomization = args => args.Append($"--no-restore --filter \"Category=Standalone\""),
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
            NoBuild = true,
            ArgumentCustomization = args => args.Append($"--no-restore --filter \"Category!=Standalone\""),
        });
    }
});

Task("Test All")
    .IsDependentOn("Test .NET")
    .IsDependentOn("Test .NET Core")
    .WithCriteria(() => IsRunningOnWindows());

Task("Pack")
    .IsDependentOn("Test All")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() => 
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
    .IsDependentOn("Pack")
    .WithCriteria(() => IsRunningOnWindows());

RunTarget(target);