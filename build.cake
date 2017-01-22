#addin "Cake.Docker"
#addin "Cake.Compression"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "FullBuild");
var configuration = Argument("configuration", "Debug");
var buildNumber = Argument("buildNumber", "0");

var version = "0.4.0." + buildNumber;

var cafeDirectory = Directory("./src/cafe");
var cafeProject = cafeDirectory + File("project.json");
var cafeUnitTestProject = Directory("./test/cafe.Test/project.json");
var cafeIntegrationTestProject = Directory("./test/cafe.IntegrationTest/project.json");

var buildSettings = new DotNetCoreBuildSettings { VersionSuffix = buildNumber, Configuration = configuration };

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/cafe/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore")
    .Does(() =>
{
    DotNetCoreRestore(cafeProject);
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetCoreBuild(cafeProject, buildSettings);
});

Task("UnitTest")
    .Does(() =>
    {
        DotNetCoreRestore(cafeUnitTestProject);      
        DotNetCoreBuild(cafeUnitTestProject, buildSettings);
        DotNetCoreTest(cafeUnitTestProject);
    });

Task("IntegrationTest")
    .Does(() =>
    {
        DotNetCoreRestore(cafeIntegrationTestProject);      
        DotNetCoreBuild(cafeIntegrationTestProject, buildSettings);
        DotNetCoreTest(cafeIntegrationTestProject);
    });

Task("Publish")
    .Does(() => 
    {
        Information("Publishing {0}", configuration);
        DotNetCorePublish(cafeProject, new DotNetCorePublishSettings { Runtime = "win10-x64", Configuration = configuration, VersionSuffix = buildNumber });
        // Later: DotNetCorePublish(cafeProject, new DotNetCorePublishSettings { Runtime = "centos.7-x64", Configuration = configuration, VersionSuffix = buildNumber });
        // Later: DotNetCorePublish(cafeProject, new DotNetCorePublishSettings { Runtime = "ubuntu.16.04-x64", Configuration = configuration, VersionSuffix = buildNumber });
    });

var archiveDirectory =  Directory("archive");

Task("Archive")
    .Does(() => 
    {
        Information("Archiving {0}", configuration);
        CreateDirectory(archiveDirectory);
        Zip(cafeWindowsPublishDirectory, archiveDirectory  + File("cafe-win10-x64-" + version + ".zip"));
    });

var cafeWindowsContainerImage = "cafe:windows";

Task("Build-WindowsImage")
    .IsDependentOn("Publish")
    .Does(() => {
        DockerBuild(new DockerBuildSettings { File = cafeDirectory + File("Dockerfile-windows"), Tag = new[] { cafeWindowsContainerImage } }, cafeDirectory);
    });

Task("Run-CafeServerDockerContainer")
    .IsDependentOn("Build-WindowsImage")
    .Does(() => {
        DockerRun(new DockerRunSettings { Interactive = true }, cafeWindowsContainerImage, "server", new string[0]);
    });


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("IncrementalBuild")
    .IsDependentOn("Build")
    .IsDependentOn("UnitTest")
    .IsDependentOn("IntegrationTest")
    .IsDependentOn("Publish")
    .IsDependentOn("Archive");

Task("FullBuild")
    .IsDependentOn("Clean")
    .IsDependentOn("IncrementalBuild")
    .IsDependentOn("Build-WindowsImage");

//////////////////////////////////////////////////////////////////////
// TESTING TARGETS
//////////////////////////////////////////////////////////////////////

var cafeWindowsPublishDirectory = buildDir + Directory("netcoreapp1.1/win10-x64/publish");

Task("ShowHelp")
    .Does(() => {
        RunCafe("-h");
    });

Task("ShowChefHelp")
    .Does(() => {
        RunCafe("chef -h");
    });

Task("ShowChefRunHelp")
    .Does(() => {
        RunCafe("chef run -h");
    });

Task("ShowChefStatus")
    .Does(() => {
        RunCafe("chef status");
    });

Task("ShowJobStatus")
    .Does(() => {
        RunCafe("job all");
    });

Task("RunChef")
    .Does(() => {
        RunCafe("chef run");
    }); 


Task("RunServer")
    .Does(() => {
        RunCafe("server"); 
    });

var oldVersion = "12.16.42";

Task("DownloadOldVersion")
    .Does(() =>
    {
        RunCafe("chef download {0}", oldVersion);
    });

Task("InstallOldVersion")
    .Does(() =>
    {
        RunCafe("chef install {0}", oldVersion);
    });

var newVersion = "12.17.44";

Task("DownloadNewVersion")
    .Does(() =>
    {
        RunCafe("chef download {0}", newVersion);
    });

Task("InstallNewVersion")
    .Does(() =>
    {
        RunCafe("chef install {0}", newVersion);
    });


Task("BootstrapPolicy")
    .Does(() =>
    {
        RunCafe(@"chef bootstrap policy: cafe-demo group: qa config: C:\Users\mhedg\.chef\client.rb validator: C:\Users\mhedg\.chef\cafe-demo-validator.pem");
    });

Task("RegisterService")
    .Does(() =>
    {
        RunCafe(@"service register");
    });

Task("UnregisterService")
    .Does(() =>
    {
        RunCafe(@"service unregister");
    });

Task("PauseChef")
    .Does(() =>
    {
        RunCafe(@"chef pause");
    });

Task("ResumeChef")
    .Does(() =>
    {
        RunCafe(@"chef resume");
    });

Task("StopService")
    .Does(() =>
    {
        RunCafe(@"service stop");
    });


Task("StartService")
    .Does(() =>
    {
        RunCafe(@"service start");
    });

public void RunCafe(string argument, params string[] formatParameters) 
{
  var arguments = string.Format(argument, formatParameters);
  var processSettings =  new ProcessSettings { Arguments = arguments}.UseWorkingDirectory(cafeWindowsPublishDirectory);
  Information("Running cafe.exe from {0}", cafeWindowsPublishDirectory);
  var exitCode = StartProcess(cafeWindowsPublishDirectory + File("cafe.exe"), processSettings);
  Information("Exit code: {0}", exitCode);
  if (exitCode < 0) throw new Exception(string.Format("cafe.exe exited with code: {0}", exitCode));
}

Task("AcceptanceTest")
    .IsDependentOn("ShowHelp")
    .IsDependentOn("ShowChefHelp")
    .IsDependentOn("ShowChefRunHelp")
    .IsDependentOn("RegisterService")
    .IsDependentOn("DownloadOldVersion")
    .IsDependentOn("InstallOldVersion")
    .IsDependentOn("BootstrapPolicy")
    .IsDependentOn("ShowChefStatus")
    .IsDependentOn("ShowJobStatus")
    .IsDependentOn("DownloadNewVersion")
    .IsDependentOn("InstallNewVersion")
    .IsDependentOn("RunChef")
    .IsDependentOn("PauseChef")
    .IsDependentOn("ResumeChef")
    .IsDependentOn("StopService")
    .IsDependentOn("UnregisterService");

Task("RunServerInDocker")
    .Does(() => {
        var settings = new DockerRunSettings
        {
            Interactive = true,
            Rm = true
        };
        DockerRun(settings, "cafe:windows", string.Empty, new string[0]);
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
