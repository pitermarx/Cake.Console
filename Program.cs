using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Core;
using static Cake.Console.CakeConsoleAppHost;

ScriptHost
    .Setup(ctx => ctx.Information("Setup!"));
ScriptHost
    .Task("Hello")
    .Does(ctx => ctx.Information($"Hello {ctx.Configuration.GetValue("Name")}"));

ScriptHost.RunTarget(ScriptHost.Context.Argument("Target", ""));