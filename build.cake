///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var version = XmlPeek(File("./build/common.props"), "/Project/PropertyGroup/Version/text()");

var sln = "./Evolve.sln";
var distDir = "./dist";
var publishDir = "./publish";
var winWarpPacker = "./warp/windows-x64.warp-packer.exe";
var linuxWarpPacker = "./warp/linux-x64.warp-packer";
var framework = "netcoreapp2.1";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx => 
{ 
    Information($"Building Evolve {version}");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("clean").Does(() =>
{
    CreateDirectory(distDir);
    
    CleanDirectories(distDir);
    CleanDirectories(publishDir);
    CleanDirectories($"./**/obj/{framework}");
    CleanDirectories(string.Format("./**/obj/{0}", configuration));
    CleanDirectories(string.Format("./**/bin/{0}", configuration));
});

Task("win-build").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    MSBuild(sln, new MSBuildSettings
    {
        Configuration = configuration,
        Verbosity = Verbosity.Minimal,
        Restore = true
    });
});

Task("linux-build").WithCriteria(() => IsRunningOnUnix()).Does(() =>
{
    DotNetCoreBuild("./test/Evolve.Tests", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        Verbosity = DotNetCoreVerbosity.Minimal
    });
});

Task("test").Does(() =>
{
    DotNetCoreTest("./test/Evolve.Tests", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => args.AppendSwitchQuoted("--filter", "Category!=Cli")
    });
});

Task("test-cli").Does(() =>
{
    DotNetCoreTest("./test/Evolve.Tests", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => args.AppendSwitchQuoted("--filter", "Category=Cli")
    });
});

Task("win-publish-cli").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    DotNetCorePublish("./src/Evolve.Cli", new DotNetCorePublishSettings
    {
        Configuration = configuration,
        OutputDirectory = publishDir + "/cli/win-x64",
        Runtime = "win-x64"
    });
});

Task("linux-publish-cli").WithCriteria(() => IsRunningOnUnix()).Does(() =>
{
    DotNetCorePublish("./src/Evolve.Cli", new DotNetCorePublishSettings
    {
        Configuration = configuration,
        OutputDirectory = publishDir + "/cli/linux-x64",
        Runtime = "linux-x64"
    });
});

Task("win-warp-cli").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    StartProcess(winWarpPacker, new ProcessSettings().WithArguments
    (
        args => args.Append($"--arch windows-x64")
                    .Append($"--input_dir {publishDir}/cli/win-x64")
                    .Append($"--exec Evolve.Cli.exe")
                    .Append($"--output {distDir}/evolve.exe")
    ));
});

Task("linux-warp-cli").WithCriteria(() => IsRunningOnUnix()).Does(() =>
{
    StartProcess(linuxWarpPacker, new ProcessSettings().WithArguments
    (
        args => args.Append($"--arch linux-x64")
                    .Append($"--input_dir {publishDir}/cli/linux-x64")
                    .Append($"--exec Evolve.Cli")
                    .Append($"--output {distDir}/evolve")
    ));
});

Task("pack-evolve").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    NuGetPack("./src/Evolve/Evolve.nuspec", new NuGetPackSettings 
    {
        OutputDirectory = distDir,
        Version = version
    });
});

Task("pack-evolve.msbuild.windows.x64").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    NuGetPack("./src/Evolve.MSBuild/Evolve.MSBuild.Windows.x64.nuspec", new NuGetPackSettings 
    {
        DevelopmentDependency = true,
        OutputDirectory = distDir,
        Version = version
    });
});

Task("default")
    .IsDependentOn("clean")
    .IsDependentOn("win-build")
    .IsDependentOn("linux-build")
    .IsDependentOn("test")
    .IsDependentOn("win-publish-cli")
    .IsDependentOn("win-warp-cli")
    .IsDependentOn("linux-publish-cli")
    .IsDependentOn("linux-warp-cli")
    .IsDependentOn("test-cli")
    .IsDependentOn("pack-evolve.msbuild.windows.x64")
    .IsDependentOn("pack-evolve");

RunTarget(target);
