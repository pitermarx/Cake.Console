# Cake.Console

An alternative to Cake.Frosting

## Usage
 1. Create a new console app
 2. copy the code into a directory side by side your app
 3. Add `<ProjectReference Include="..\Cake.Console\Cake.Console.csproj" />`
 4. Use it like this

```cs
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Console;
using Cake.Core;

var cake = new CakeHostBuilder(args)
    .InstallNugetTool("xunit.runner.console", "2.4.1")
    .Build();

cake.Task("Hello")
    .Description("This is just like a cake script")
    .IsDependeeOf("World")
    .Does(c => c.Information("but methods are on the 'cake' object"));

cake.Task("World")
    .Does(c => c.Information("Hello world"));

var target = cake.Context.Argument("target", "hello");
cake.RunTarget(target);
```

or like this

```cs
using Cake.Common.Diagnostics;
using Cake.Console;
using Cake.Core;

var cake = new CakeHostBuilder(args)
    .ContextData<Dependency>()
    .WorkingDirectory<Tasks>()
    .RegisterTasks<Tasks>()
    .Build();

cake.RunTarget(nameof(Tasks.TaskName));

public record Dependency(Text = "Inject stuff into the class!!");

public class Tasks : ICakeTasks, IWorkingDirectory
{
    public string WorkingDirectory => ".";
    private readonly ICakeContext ctx;
    private readonly Dependency dep;

    public Tasks(ICakeContext ctx, Dependency dep)
    {
        this.ctx = ctx;
        this.dep = dep;
    }

    public void TaskName(CakeTaskBuilder builder) => builder
        .Description("The method name will be the task name")
        .IsDependentOn(nameof(AnotherTask));

    public void AnotherTask(CakeTaskBuilder builder) => builder
        .Description("Tasks will be all instance methods with the signature void M(CakeTaskBuilder)")
        .Does(() => ctx.Information(dep.Text));
}
```

or mix and match!
