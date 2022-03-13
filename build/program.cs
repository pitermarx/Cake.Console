using Cake.Console;
using Cake.Common.Tools.DotNet;
using Cake.Common.Build;
using Cake.Core;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Common.Tools.DotNet.NuGet.Push;
using Cake.Core.IO;
using System.Linq;
using Cake.Common.IO;
using Cake.Common;
using System;

var proj = "src/Cake.Console/Cake.Console.csproj";
var testProj = "src/Cake.Console.Tests/Cake.Console.Tests.csproj";
var version = "2.1.0";
var config = "Release";

var host = new CakeHostBuilder().BuildHost(args);

host.Task("Clean")
    .WithCriteria(c => c.HasArgument("rebuild"))
    .Does(c => Delete("src/**/obj", "src/**/bin"));

host.Task("Build")
    .IsDependentOn("Clean")
    .Does(c =>
    {
        var sett = new DotNetBuildSettings { Configuration = config, NoLogo = true };
        c.DotNetBuild(proj, sett);
        c.DotNetBuild(testProj, sett);
    });

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

host.Task("Test")
    .IsDependentOn("Build")
    .DoesForEach(
        tests,
        (t, c) => c.Verify(
            $"Test_{t.Replace(" ", "_")}",
            s =>
            {
                s.ScrubLinesContaining(StringComparison.OrdinalIgnoreCase, "00:00:0");
                return Run(t);
            }));

host.Task("Pack")
    .IsDependentOn("Test")
    .Does(c => c.DotNetPack(proj, new DotNetPackSettings { Configuration = config, NoBuild = true, NoLogo = true }));

host.Task("Push")
    .WithCriteria(c => c.GitHubActions().IsRunningOnGitHubActions &&
                       c.HasEnvironmentVariable("NUGET_API_KEY"))
    .IsDependentOn("Pack")
    .Does(c =>
    {
        c.DotNetNuGetPush(
            c.GetFiles($"**/Cake.Console.{version}.nupkg").First().FullPath,
            new DotNetNuGetPushSettings
            {
                ApiKey = c.Environment.GetEnvironmentVariable("NUGET_API_KEY"),
                SkipDuplicate = true,
                Source = " https://api.nuget.org/v3/index.json"
            });
    });

host.Task("Default")
    .IsDependentOn("Push");

host.RunTarget(host.Context.Argument("target", "default"));

string Run(string cmd)
{
    var settings = new ProcessSettings()
        .SetRedirectStandardOutput(true)
        .SetRedirectStandardError(true)
        .WithArguments(a => a.Append($"run --project {testProj} -c={config} --no-build -- {cmd}"));

    using (var process = host.Context.ProcessRunner.Start("dotnet", settings))
    {
        var t = string.Join("\n", process.GetStandardOutput());
        var err = string.Join("\n", process.GetStandardError());
        return t + err;
    }
}

void Delete(params string[] globs)
{
    foreach (var glob in globs)
    {
        var dirs = host.Context.GetDirectories(glob);
        host.Context.DeleteDirectories(dirs, new DeleteDirectorySettings
        {
            Recursive = true
        });
    }
}