using System;
using System.Linq;
using Cake.Common.Diagnostics;
using Cake.Console;
using Cake.Core;

var message = args.FirstOrDefault() switch
{
    "host" => BuildHost(),
    "cli" => RunCli(),
    var action => $"action '{action ?? "null"}' not defined",
};

Console.WriteLine(message);

string BuildHost()
{
    var host = new CakeHostBuilder()
        .ContextData<ContextData>()
        .InstallNugetTool("xunit.runner.console", "2.9.3")
        .BuildHost(args.Skip(1));

    var tasks = new Tasks(host.Context);
    tasks.Task1(host.Task("Task1"));
    host.RunTarget("Task1");

    return "OK";
}

string RunCli()
{
    var host = new CakeHostBuilder().RegisterTasks<Tasks>();

    if (args.Any(a => a.ToLowerInvariant() == "--target=printargs"))
    {
        host = host.ContextData<ContextData>();
    }

    host.RunCakeCli(args.Skip(1));

    return "OK";
}

internal class ContextData(ICakeArguments args)
{
    public string SomeVeryImportantData { get; set; } =
        args.HasArgument("tone-down") ? "Cake is pretty good..." : "Cake is awesome!";
}

internal class Tasks(ICakeContext ctx) : ICakeTasks
{
    public void PrintArgs(CakeTaskBuilder b) =>
        b.Does<ContextData>(data =>
            {
                foreach (var a in ctx.Arguments.GetArguments())
                    ctx.Information($"{a.Key}={string.Join(",", a.Value)}");

                ctx.Information($"--SomeVeryImportantData={data.SomeVeryImportantData}");
            }
        );

    public void Task1(CakeTaskBuilder b) => b.Does(c => c.Information("Task1 executed"));

    public static void Task2(CakeTaskBuilder b) => b.Does(c => c.Information("Task2 executed"));

    public static void TaskA(CakeTaskBuilder b) => b.Does(c => c.Information("TaskA executed"));

    public static void TaskB(CakeTaskBuilder b) =>
        b.IsDependentOn("TaskA").Does(c => c.Information("TaskB executed"));

    public static void TaskC(CakeTaskBuilder b) => b.IsDependentOn("TaskB").Description("hello");

    public static void TaskD(CakeTaskBuilder b) =>
        b.IsDependentOn("TaskB").Description("Some random text (&($/# /hda");
}
