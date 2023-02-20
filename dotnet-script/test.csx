#!/usr/bin/env dotnet-script
#r "nuget: cake.console, 3.0.0.1"

using Cake.Core;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Console;

var host = new CakeHostBuilder()
    .InstallDotnetTool("GitVersion.Tool", "5.7.0")
    .BuildHost(Args);

host.Task("test")
    .Does(c => c.Information("Hello from dotnet-script"));

var target = host.Context.Argument("target", "test");

host.RunTarget(target);
