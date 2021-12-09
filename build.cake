#tool nuget:?package=ReportGenerator&version=4.8.6
#tool nuget:?package=NuGet.CommandLine

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var version = XmlPeek(File("./build/common.props"), "/Project/PropertyGroup/Version/text()");

var framework = "net5.0";
var sln = "./Evolve.sln";
var reportGeneratorPath = $"./tools/ReportGenerator.4.8.6/tools/{framework}/" + (IsRunningOnUnix() ? "ReportGenerator.dll" : "ReportGenerator.exe");
var distDir = "./dist";
var distDirFullPath = MakeAbsolute(Directory($"{distDir}")).FullPath;
var publishDir = "./publish";
var publishDirFullPath = MakeAbsolute(Directory($"{publishDir}")).FullPath;
var winWarpPacker = "./build/warp/windows-x64.warp-packer.exe";
var linuxWarpPacker = "./build/warp/linux-x64.warp-packer";
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

Task("build").Does(() =>
{
    DotNetBuild(sln, new DotNetBuildSettings
    {
        Configuration = configuration,
        Verbosity = DotNetVerbosity.Minimal
    });
});

Task("test").Does(() =>
{
    var pathFilter = Environment.GetEnvironmentVariable("APPVEYOR") == "True" ? "\\SCassandra" : "";

    DotNetTest("./test/Evolve.Tests", new DotNetTestSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => args.AppendSwitchQuoted("--filter", "Category!=Cli")
                                            .Append(logger)
                                            .Append("/p:AltCover=true")
                                            .Append("/p:AltCoverForce=true")
                                            .Append("/p:AltCoverCallContext=[Fact]|[Theory]")
                                            .Append("/p:AltCoverAssemblyFilter=Evolve.Tests|xunit.runner|MySqlConnector|xunit.assert|xunit.core|xunit.execution.dotnet")
                                            .Append($"/p:AltCoverPathFilter={pathFilter}")
                                            .Append("/p:AltCoverTypeFilter=Evolve.Utilities.Check|SimpleJSON.JSON|SimpleJSON.JSONArray|SimpleJSON.JSONBool|SimpleJSON.JSONLazyCreator|SimpleJSON.JSONNode|SimpleJSON.JSONNull|SimpleJSON.JSONNumber|SimpleJSON.JSONObject|SimpleJSON.JSONString|ConsoleTables.ConsoleTable|ConsoleTables.ConsoleTableOptions")
                                            .Append($"/p:AltCoverReport={publishDirFullPath}/coverage.xml")
    });
});

Task("report-coverage").Does(() =>
{
    ReportGenerator(report: $"{publishDir}/coverage.xml", targetDir: $"{publishDir}/coverage", new ReportGeneratorSettings
    {
        ReportTypes = new[] { ReportGeneratorReportType.Badges, ReportGeneratorReportType.Cobertura, ReportGeneratorReportType.HtmlInline_AzurePipelines_Dark },
        Verbosity = ReportGeneratorVerbosity.Info,
        ToolPath = reportGeneratorPath
    });
});

Task("test-cli").Does(() =>
{
    DotNetTest("./test/Evolve.Tests", new DotNetTestSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => args.AppendSwitchQuoted("--filter", "Category=Cli")
                                            .Append(logger)
    });
});

Task("win-publish-cli").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    DotNetPublish("./src/Evolve.Cli", new DotNetPublishSettings
    {
        Configuration = configuration,
        OutputDirectory = publishDir + "/cli/win-x64",
        Runtime = "win-x64"
    });
});

Task("linux-publish-cli").WithCriteria(() => IsRunningOnUnix()).Does(() =>
{
    DotNetPublish("./src/Evolve.Cli", new DotNetPublishSettings
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

Task("pack-evolve-tool").WithCriteria(() => IsRunningOnWindows()).Does(() =>
{
    DotNetPack("./src/Evolve.Tool/", new DotNetPackSettings 
    {
        OutputDirectory = distDir,
		Configuration = configuration,
		NoBuild = false
    });
});

Task("default")
    .IsDependentOn("clean")
    .IsDependentOn("build")
    .IsDependentOn("test")
    .IsDependentOn("report-coverage")
    .IsDependentOn("win-publish-cli")
    .IsDependentOn("win-warp-cli")
    .IsDependentOn("linux-publish-cli")
    .IsDependentOn("linux-warp-cli")
    .IsDependentOn("test-cli")
    .IsDependentOn("pack-evolve")
	.IsDependentOn("pack-evolve-tool");

RunTarget(target);