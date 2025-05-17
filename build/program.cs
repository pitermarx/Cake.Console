using System.Text.RegularExpressions;
using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.NuGet.Push;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Console;
using Cake.Core;
using Cake.Core.IO;

const string config = "Release";
const string proj = "src/Cake.Console/Cake.Console.csproj";
const string testProj = "src/Cake.Console.Tests/Cake.Console.Tests.csproj";
var version = Environment.GetEnvironmentVariable("VersionPrefix");
var cakeversion = Environment.GetEnvironmentVariable("CakeVersion");

var host = new CakeHostBuilder().BuildHost(args);

host.Task("Clean")
    .WithCriteria(c => c.HasArgument("rebuild"))
    .Does(() => Delete("src/**/obj", "src/**/bin"));

host.Task("Build")
    .IsDependentOn("Clean")
    .Does(c =>
    {
        var sett = new DotNetBuildSettings
        {
            Configuration = config,
            NoLogo = true,
            ArgumentCustomization = builder =>
                builder.Append($"/p:VersionPrefix={version}"),
        };
        c.DotNetBuild(testProj, sett);
    });

var pwd = Environment.CurrentDirectory;
var s = new VerifySettings();
if (host.Context.GitHubActions().IsRunningOnGitHubActions)
{
    s.DisableDiff();
}
s.ScrubLinesContaining(StringComparison.OrdinalIgnoreCase, "00:00:0");
s.ScrubLinesWithReplace(l => l.Replace(pwd, "{CurrentDirectory}"));
s.ScrubLinesWithReplace(l =>
    new Regex(@"Details: 1\.2\.3\+.*").Replace(l, "Details: 1.2.3+{Hash}")
);
host.Task("Test")
    .IsDependentOn("Build")
    .Does(async c =>
    {
        var tests = new[]
        {
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
            "cli --target=printargs --arg1=1 --arg2=x --super-long-arg=super-long-value,hello",
        };
        
        foreach (var t in tests)
        {
            var result = Run(t);
            await new InnerVerifier("build\\snapshots", $"Test_{t.Replace(" ", "_")}", s).Verify(result);
        }
    });

host.Task("Pack")
    .IsDependentOn("Test")
    .Does(c =>
        c.DotNetPack(
            proj,
            new DotNetPackSettings
            {
                Configuration = config,
                NoBuild = true,
                NoLogo = true,
                ArgumentCustomization = builder =>
                    builder.Append($"/p:CakeVersion={cakeversion} /p:Version={version}"),
            }
        )
    );

host.Task("Push")
    .WithCriteria(c =>
        c.GitHubActions().IsRunningOnGitHubActions && c.HasEnvironmentVariable("NUGET_API_KEY")
    )
    .IsDependentOn("Pack")
    .Does(c =>
    {
        c.DotNetNuGetPush(
            c.GetFiles($"**/Cake.Console.{version}.nupkg").First().FullPath,
            new DotNetNuGetPushSettings
            {
                ApiKey = c.Environment.GetEnvironmentVariable("NUGET_API_KEY"),
                SkipDuplicate = true,
                Source = " https://api.nuget.org/v3/index.json",
            }
        );
    });

host.Task("Default").IsDependentOn("Push");

host.RunTarget(host.Context.Argument("target", "default"));

string Run(string cmd)
{
    // find dll
    var dll = host
        .Context.GetFiles("src/Cake.Console.Tests/bin/Release/*/Cake.Console.Tests.dll")
        .First();
    host.Context.Information($"Running: dotnet {dll} {cmd}");
    var settings = new ProcessSettings()
        .SetRedirectStandardOutput(true)
        .SetRedirectStandardError(true)
        .WithArguments(a => a.Append($"{dll} {cmd}"));

    settings.EnvironmentVariables = new Dictionary<string, string>{ ["NO_COLOR"] = "true" };
    
    using var process = host.Context.ProcessRunner.Start("dotnet", settings);
    var t = string.Join("\n", process.GetStandardOutput());
    var err = string.Join("\n", process.GetStandardError());
    return t + err;
}

void Delete(params string[] globs)
{
    foreach (var glob in globs)
    {
        var dirs = host.Context.GetDirectories(glob);
        host.Context.DeleteDirectories(dirs, new DeleteDirectorySettings { Recursive = true });
    }
}
