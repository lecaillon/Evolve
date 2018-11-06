///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var version = Argument("productversion", "2.0.0");

var sln = "./Evolve.sln";
var distDir = "./dist";
var publishDir = "./publish";
var winWarpPacker = "./warp/windows-x64.warp-packer.exe";
var linuxWarpPacker = "./warp/linux-x64.warp-packer";
var framework = "netcoreapp2.1";

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("clean").Does(() =>
{ 
    CleanDirectories(distDir);
    CleanDirectories(publishDir);
    CleanDirectories($"./**/obj/{framework}");
    CleanDirectories(string.Format("./**/obj/{0}", configuration));
    CleanDirectories(string.Format("./**/bin/{0}", configuration));
});

Task("build").Does(() =>
{
    DotNetCoreBuild(sln, new DotNetCoreBuildSettings
    {
        Configuration = configuration, 
        ArgumentCustomization = args => args.Append($"/p:Version={version}")
    });
});

Task("test").Does(() =>
{
    DotNetCoreTest("./test/Evolve.Tests", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true
    });
});

Task("win-publish").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    DotNetCorePublish("./src/Evolve.Cli", new DotNetCorePublishSettings
    {
        Configuration = configuration,
        OutputDirectory = publishDir + "/cli/win-x64",
        Runtime = "win-x64",
        ArgumentCustomization = args => args.Append($"/p:Version={version}")
    });
});

Task("win-warp").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    StartProcess(winWarpPacker, new ProcessSettings().WithArguments
    (
        args => args.Append($"--arch windows-x64")
                    .Append($"--input_dir {publishDir}/cli/win-x64")
                    .Append($"--exec Evolve.Cli.exe")
                    .Append($"--output {distDir}/evolve.exe")
    ));
});

Task("pack-evolve").Does(() =>
{
    DotNetCorePack("./src/Evolve", new DotNetCorePackSettings 
    {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = distDir,
        ArgumentCustomization = args => args.Append($"/p:Version={version}")
    });
});

Task("default")
    .IsDependentOn("clean")
    .IsDependentOn("build")
    .IsDependentOn("test")
    .IsDependentOn("win-publish")
    .IsDependentOn("win-warp")
    .IsDependentOn("pack-evolve");

RunTarget(target);