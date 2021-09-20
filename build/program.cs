using Cake.Console;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Build;
using Cake.Core;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Common.Tools.DotNetCore.Pack;
using Cake.Common.Tools.DotNetCore.NuGet.Push;
using Cake.Core.IO;
using System.Reflection;
using VerifyTests;
using System.Linq;
using System.Text.RegularExpressions;

var proj = "src/Cake.Console/Cake.Console.csproj";
var testProj = "src/Cake.Console.Tests/Cake.Console.Tests.csproj";
var version = "1.2.0.4";
var config = "Release";

var host = new CakeHostBuilder().BuildHost(args);

host.Task("Build")
    .Does(c =>
    {
        var sett = new DotNetCoreBuildSettings { Configuration = config, NoLogo = true };
        c.DotNetCoreBuild(proj, sett);
        c.DotNetCoreBuild(testProj, sett);
    });

var testTask = host.Task("Test")
    .IsDependentOn("Build");

var tests = new []{
    "unknown",
    "host",
    "cli",
    "cli --target=task2",
    "cli --target=task1 -v=Diagnostic",
    "cli --target=task1",
    "cli --target=taskB",
    "cli --target=taskB --exclusive",
    "cli --description",
    "cli --dryrun",
    "cli --dryrun --target=TaskB",
    "cli --dryrun --target=TaskB --exclusive",
    "cli --version",
    "cli --info",
    "cli --tree",
    "cli --help",
    "cli -h",
    "cli --target=printargs --arg1=1 --arg2=x --super-long-arg=super-long-value,hello"
};
foreach (var t in tests)
{
    var name = "Test_" + t.Replace(" ", "_");
    host.Task(name).Verify(c => Run(t));
    testTask.IsDependentOn(name);
}

host.Task("Pack")
    .IsDependentOn("Test")
    .Does(c => c.DotNetCorePack(proj, new DotNetCorePackSettings { Configuration = config, NoBuild = true, NoLogo = true }));

host.Task("Push")
    .WithCriteria(c => c.GitHubActions().IsRunningOnGitHubActions)
    .IsDependentOn("Pack")
    .Does(c => c.DotNetCoreNuGetPush($"Cake.Console.{version}.nupkg", new DotNetCoreNuGetPushSettings
    {
        ApiKey = c.Environment.GetEnvironmentVariable("NUGET_API_KEY"),
        Source = c.Environment.GetEnvironmentVariable("PUSH_SOURCE"),
        SkipDuplicate = true
    }));

host.Task("Default")
    .IsDependentOn("Push");

host.RunTarget("Default");

string Run(string cmd)
{
    var settings = new ProcessSettings()
        .SetRedirectStandardOutput(true)
        .SetRedirectStandardError(true)
        .WithArguments(a => a.Append($"run -p {testProj} --no-build -- {cmd}"));

    using (var process = host.Context.ProcessRunner.Start("dotnet", settings))
    {
        var t = string.Join("\n", process.GetStandardOutput());
        var err = string.Join("\n", process.GetStandardError());
        return t + err;
    }
}