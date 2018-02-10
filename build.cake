///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var sln = GetFiles("./Evolve.sln").First();
var slnTest = GetFiles("./Evolve.Test.Package.sln").First();
var distDir = MakeAbsolute(Directory("./dist"));
var version = XmlPeek(File("./build/common.props"), "/Project/PropertyGroup/PackageVersion/text()");
var envHome = Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx => { Information($"Building Evolve {version}"); });

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean").Does(() =>
{ 
    CleanDirectory($@"{envHome}/.nuget/packages/evolve/{version}");
});

Task("Restore").Does(() =>
{
    if(IsRunningOnWindows()) NuGetRestore(sln);
    DotNetCoreRestore(sln.ToString());
});

Task("Build").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    MSBuild(sln, settings => settings.SetConfiguration(configuration)
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

Task("Pack").Does(() => 
{
    var settings = new NuGetPackSettings
    {
        OutputDirectory = distDir.FullPath,
        Version = version,
        Properties = new Dictionary<string, string> 
        {
            { "Configuration", configuration }
        }
    };

    if (IsRunningOnWindows())
    {
        NuGetPack("./src/Evolve/Evolve.nuspec", settings);
    }
    else
    {
        NuGetPack("./src/Evolve/Evolve-Core.nuspec", settings);
    }
});

Task("Restore Test-Package").Does(() =>
{
    var feed = new 
    {
        Name = "Localhost",
        Source = IsRunningOnWindows() ? distDir.FullPath.Replace('/', '\\') : distDir.FullPath
    };

    if (!NuGetHasSource(feed.Source))
    {
        NuGetAddSource(feed.Name, feed.Source);
    }

    if(IsRunningOnWindows()) NuGetRestore(slnTest);
    DotNetCoreRestore(slnTest.ToString());

    if (!NuGetHasSource(feed.Source))
    {
        NuGetRemoveSource(feed.Name, feed.Source);
    }
});

Task("Build Test-Package").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    MSBuild(slnTest, settings => settings.SetConfiguration(configuration)
                                         .SetVerbosity(Verbosity.Minimal));
});

Task("Build .NET Core Test-Package").Does(() =>
{
    foreach(var project in GetFiles("./test-package/**/Evolve.*Core*.Test.csproj"))
    {
        DotNetCoreBuild(project.FullPath, new DotNetCoreBuildSettings 
        {
            Configuration = configuration,
            ArgumentCustomization = args => args.Append($"--no-restore"),
        });
    }
});

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test .NET")
    .IsDependentOn("Test .NET Core")
    .IsDependentOn("Pack")
    .IsDependentOn("Restore Test-Package")
    .IsDependentOn("Build Test-Package")
    .IsDependentOn("Build .NET Core Test-Package");

Task("Test-Package")
    .IsDependentOn("Restore Test-Package")
    .IsDependentOn("Build Test-Package")
    .IsDependentOn("Build .NET Core Test-Package");

RunTarget(target);
