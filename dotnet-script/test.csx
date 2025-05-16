#!/usr/bin/env dotnet-script
#r "nuget: cake.console, 4.0.0"

using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Console;
using Cake.Core;

var host = new CakeHostBuilder()
    .InstallNugetTool("xunit.runner.console", "2.9.3")
    .InstallDotnetTool("GitVersion.Tool", "5.7.0")
    .BuildHost(Args);

host.Task("test").Does(c => c.Information("Hello from dotnet-script"));

var target = host.Context.Argument("target", "test");

host.RunTarget(target);
