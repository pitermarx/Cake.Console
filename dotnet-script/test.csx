#!/usr/bin/env dotnet-script
#r "nuget: cake.console, 1.3.0"

using Cake.Core;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Console;

var host = new CakeHostBuilder().BuildHost(Args);

host.Task("test")
    .WithCriteria(c => c.HasArgument("skip"))
    .Does(c => c.Information("Hello from dotnet-script"));

var target = host.Context.Argument("target", "test");
host.RunTarget(target);
