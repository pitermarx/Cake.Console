using Cake.Console;
using Cake.Core;
using Cake.Common.Diagnostics;
using System;
using System.Linq;

var message = args.FirstOrDefault() switch
{
    "host" => BuildHost(),
    "cli" => RunCli(),
    var action => $"action '{action ?? "null"}' not defined"
};

Console.WriteLine(message);

string BuildHost()
{
    var host = new CakeHostBuilder().BuildHost(args.Skip(1));
    var tasks = new Tasks();
    tasks.Task1(host.Task("Task1"));
    host.RunTarget("Task1");

    return "OK";
}

string RunCli()
{
    new CakeHostBuilder().RegisterTasks<Tasks>().RunCakeCli(args.Skip(1));

    return "OK";
}

class Tasks : ICakeTasks
{
    public void PrintArgs(CakeTaskBuilder b) => b.Does(c =>
    {
        foreach (var a in c.Arguments.GetArguments()) c.Information($"--{a.Key}={string.Join(",", a.Value)}");
    });

    public void Task1(CakeTaskBuilder b) => b.Does(c => c.Information("Task1 executed"));
    public void Task2(CakeTaskBuilder b) => b.Does(c => c.Information("Task2 executed"));
    public void TaskA(CakeTaskBuilder b) => b.Does(c => c.Information("TaskA executed"));
    public void TaskB(CakeTaskBuilder b) => b.IsDependentOn("TaskA").Does(c => c.Information("TaskB executed"));
}