#tool nuget:?package=ReportGenerator&version=4.0.11

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var version = XmlPeek(File("./build/common.props"), "/Project/PropertyGroup/Version/text()");

var sln = "./Evolve.sln";
var slnTestMsbuildWinx64 = "./Evolve.Test.MSBuild.Windows.x64.sln";
var distDir = "./dist";
var distDirFullPath = MakeAbsolute(Directory($"{distDir}")).FullPath;
var publishDir = "./publish";
var publishDirFullPath = MakeAbsolute(Directory($"{publishDir}")).FullPath;
var winWarpPacker = "./warp/windows-x64.warp-packer.exe";
var linuxWarpPacker = "./warp/linux-x64.warp-packer";
var framework = "netcoreapp2.2";
var logger = Environment.GetEnvironmentVariable("TF_BUILD") == "True" ? $"-l:trx --results-directory {publishDirFullPath}" : "-l:console;verbosity=normal";

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
                                            .Append(logger)
                                            .Append("/p:AltCover=true")
                                            .Append("/p:AltCoverForce=true")
                                            .Append("/p:AltCoverCallContext=[Fact]|[Theory]")
                                            .Append("/p:AltCoverAssemblyFilter=Evolve.Tests|xunit.runner")
                                            .Append("/p:AltCoverTypeFilter=Evolve.MSBuild.AppConfigCliArgsBuilder|Evolve.Utilities.Check|TinyJson.JSONParser")
                                            .Append($"/p:AltCoverXmlReport={publishDirFullPath}/coverage.xml")
    });
});

Task("report-coverage").Does(() =>
{
    ReportGenerator($"{publishDir}/coverage.xml", $"{publishDir}/coverage", new ReportGeneratorSettings
    {
        ReportTypes = new[] { ReportGeneratorReportType.Badges, ReportGeneratorReportType.Cobertura, ReportGeneratorReportType.HtmlInline },
        Verbosity = ReportGeneratorVerbosity.Info
    });
});

Task("test-cli").Does(() =>
{
    DotNetCoreTest("./test/Evolve.Tests", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => args.AppendSwitchQuoted("--filter", "Category=Cli")
                                            .Append(logger)
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

Task("test-msbuild.windows.x64-for-net").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    foreach(var file in GetFiles("./test-msbuild-package/Windows.x64/**/packages.config"))
    {
        XmlPoke(file, "/packages/package[@id = 'Evolve.MSBuild.Windows.x64']/@version", version);
    }

    NuGetRestore(slnTestMsbuildWinx64, new NuGetRestoreSettings
    {
        Source = new[] { "https://api.nuget.org/v3/index.json", distDirFullPath.Replace('/', '\\') }
    });

    MSBuild(slnTestMsbuildWinx64, new MSBuildSettings
    {
        Configuration = configuration,
        Verbosity = Verbosity.Minimal,
        Restore = false
    });
});

Task("test-msbuild.windows.x64-for-netcore").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    NuGetRestore(slnTestMsbuildWinx64, new NuGetRestoreSettings
    {
        Source = new[] { "https://api.nuget.org/v3/index.json", distDirFullPath.Replace('/', '\\') }
    });

    foreach(var project in GetFiles("./test-msbuild-package/Windows.x64/**/Evolve.*Core*.Test.csproj"))
    {
        DotNetCoreBuild(project.FullPath, new DotNetCoreBuildSettings 
        {
            Configuration = configuration,
            Verbosity = DotNetCoreVerbosity.Minimal,
            NoRestore = true
        });
    }
});

Task("default")
    .IsDependentOn("clean")
    .IsDependentOn("win-build")
    .IsDependentOn("linux-build")
    .IsDependentOn("test")
    .IsDependentOn("report-coverage")
    .IsDependentOn("win-publish-cli")
    .IsDependentOn("win-warp-cli")
    .IsDependentOn("linux-publish-cli")
    .IsDependentOn("linux-warp-cli")
    .IsDependentOn("test-cli")
    .IsDependentOn("pack-evolve.msbuild.windows.x64")
    .IsDependentOn("test-msbuild.windows.x64-for-net")
    .IsDependentOn("test-msbuild.windows.x64-for-netcore")
    .IsDependentOn("pack-evolve");

RunTarget(target);