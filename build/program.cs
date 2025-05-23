﻿using System.Diagnostics;
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
host.Task("Test")
    .IsDependentOn("Build")
    .Does(async c =>
    {
        string[] tests =
        [
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
        ];
        
        List<Exception> aggException = [];
        foreach (var t in tests)
        {
            try
            {
                var s = new VerifySettings();
                if (c.GitHubActions().IsRunningOnGitHubActions)
                {
                    s.DisableDiff();
                }
                s.ScrubLinesContaining(StringComparison.OrdinalIgnoreCase, "00:00:0");
                s.ScrubLinesWithReplace(l => l.Replace(pwd, "{CurrentDirectory}"));
                s.ScrubLinesWithReplace(l => l.Replace("\u001b[1m","").Replace("\u001b[0m", ""));
                s.ScrubLinesWithReplace(l =>
                    new Regex(@"Details: 1\.2\.3\+.*").Replace(l, "Details: 1.2.3+{Hash}")
                );
                var result = Run(t);
                await new InnerVerifier("build\\snapshots", $"Test_{t.Replace(" ", "_")}", s).Verify(result);
            }
            catch (Exception e)
            {
                aggException.Add(e);
                c.Error(e.Message);
            }
        }
        //
        // if (aggException.Count > 0)
        // {
        //     throw new AggregateException("Test failed", aggException);
        // }
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
        c.GitHubActions().IsRunningOnGitHubActions && 
        !c.GitHubActions().Environment.PullRequest.IsPullRequest && 
        c.HasEnvironmentVariable("NUGET_API_KEY")
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
    
    // run cmd and get stdout/stderr
    using Process process = new();
    process.StartInfo = new ("dotnet", $"{dll} {cmd}")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        WorkingDirectory = Environment.CurrentDirectory,
        Environment = { {"NO_COLOR", "true"} }
    };

    process.Start();
    
    var output = process.StandardOutput.ReadToEnd().Trim();
    output += process.StandardError.ReadToEnd();

    return output;
}

void Delete(params string[] globs)
{
    foreach (var glob in globs)
    {
        var dirs = host.Context.GetDirectories(glob);
        host.Context.DeleteDirectories(dirs, new DeleteDirectorySettings { Recursive = true });
    }
}
