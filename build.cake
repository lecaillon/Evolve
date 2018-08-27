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
var buildRunsInAppVeyor = Environment.GetEnvironmentVariable("APPVEYOR") == "True";
var buildRunsInTravisCI = Environment.GetEnvironmentVariable("TRAVIS") == "true";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx => 
{ 
    Information($"Building Evolve {version}");
    if(IsRunningOnWindows())
    { // AppVeyor
        Environment.SetEnvironmentVariable("PG_PORT", "5432");
        Environment.SetEnvironmentVariable("MYSQL_PORT", "3306");
    }
    else
    { // Travis CI
        Environment.SetEnvironmentVariable("PG_PORT", "5433");
        Environment.SetEnvironmentVariable("MYSQL_PORT", "3307");
    }
});

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

Task("Test").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    foreach(var project in GetFiles("./test/**/Evolve.*Test*.csproj").Where(x => !x.GetFilename().FullPath.Contains("Core"))
                                                                     .Where(x => !x.GetFilename().FullPath.Contains("Utilities"))
																	 .Where(x => !x.GetFilename().FullPath.Contains("Cassandra"))
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

Task("Test Core").Does(() =>
{

    foreach(var project in GetFiles("./test/**/Evolve.Core*.Test.Resources.SupportedDrivers.csproj"))
    {
        DotNetCoreBuild(project.FullPath, new DotNetCoreBuildSettings 
        {
            Configuration = configuration,
            ArgumentCustomization = args => args.Append($"--no-restore"),
        });
    }

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
    foreach(var file in GetFiles("./test-package/**/packages.config"))
    {
        XmlPoke(file, "/packages/package[@id = 'Evolve']/@version", version);
    }
  
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
    DotNetCoreRestore(slnTest.ToString(), new DotNetCoreRestoreSettings
    {
        Sources = new[] { "https://api.nuget.org/v3/index.json", feed.Source }
    });

    if (!NuGetHasSource(feed.Source))
    {
        NuGetRemoveSource(feed.Name, feed.Source);
    }
});

Task("Build Test-Package").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    MSBuild(slnTest, settings => settings.SetConfiguration(buildRunsInAppVeyor ? "AppVeyor" : configuration) // AppVeyor does not support Cassandra yet
                                         .SetVerbosity(Verbosity.Minimal));
});

Task("Build Test-Package Core").Does(() =>
{
    foreach(var project in GetFiles("./test-package/**/Evolve.*Core*.Test.csproj").Where(x => !buildRunsInAppVeyor || !x.GetFilename().FullPath.Contains("Cassandra"))
																			      )  // Travis CI: SocketException 'Connection timed out'
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
    .IsDependentOn("Test")
    .IsDependentOn("Test Core")
    .IsDependentOn("Pack")
    .IsDependentOn("Restore Test-Package")
    .IsDependentOn("Build Test-Package")
    .IsDependentOn("Build Test-Package Core");

Task("Test-Package")
    .IsDependentOn("Restore Test-Package")
    .IsDependentOn("Build Test-Package")
    .IsDependentOn("Build Test-Package Core");

Task("TestsOnly")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Test Core");

RunTarget(target);
