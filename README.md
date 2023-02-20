# Cake.Console [![Nuget](https://img.shields.io/nuget/v/cake.console?label=cake.console&style=plastic)](https://www.nuget.org/packages/Cake.Console)

[Cake](https://cakebuild.net/) scripts, but in a Console app.

An alternative to [Cake.Frosting](https://cakebuild.net/docs/running-builds/runners/cake-frosting)

https://blog.pitermarx.com/2021/09/presenting-cake-console/

## Usage

Create a new project referencing Cake.Console. It will look something like this

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <OutputType>exe</OutputType>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Cake.Console" Version="3.0.0" />
  </ItemGroup>
</Project>
```

Add a single Program.cs file with the code. Take advantage of [top-level statements](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/top-level-statements).

There are 2 ways of using Cake.Console:

1. Building an IScriptHost. This is the implicit object in the .cake scripts, so we can use it to register tasks, perform setup, etc.

```cs
var host = new CakeHostBuilder().BuildHost(args);

host.Setup(() => { do something });
host.Task("TaskName").Does(c => c.Information("Hello"));
host.RunTarget(host.Context.Arguments.GetArgument("target"));
```

2. Using the Cake Cli, that includes arguments like --target, --version, --info, --tree, --description, --exclusive...
It's very similar to [frosting](https://cakebuild.net/docs/running-builds/runners/cake-frosting)

```cs
new CakeHostBuilder()
    .WorkingDirectory<WorkingDirectory>()
    .ContextData<BuildData>()
    .RegisterTasks<CakeTasks>()
    .InstallNugetTool("NuGet.CommandLine", "5.9.1")
    .InstallDotnetTool("GitVersion.Tool", "5.7.0")
    .RunCakeCli(args);
```

In this case, we dont have access to the host, so we need to define the build with the 4 extensions that come with Cake.Console:

- WorkingDirectory<>
- RegisterTasks<>
- ContextData<>
- InstallNugetTool

## WorkingDirectory<>
Here we can use a class that has the interface IWorkingDirectory and implements the string WorkingDirectory property.

The class can receive in the constructor any part of the cake infrastructure (ICakeContext, ICakeLog, ICakeArguments, ICakeConfiguration...)

## RegisterTasks<>
Here we can use a class that has the interface ICakeTasks.

The class can receive in the constructor any part of the cake infrastructure (ICakeContext, ICakeLog, ICakeArguments, ICakeConfiguration...)

All the methods that have the signature `void Name(CakeTaskBuilder builder)` will be called, and the name of the method will be the name of the task.

## ContextData<>
Here we can use any class that will then be available for use in the task's definitions.

## InstallNugetTool/InstallDotnetTool
Given a package name and a version, installs a nuget package or a dotnet tool as a [Cake tool](https://cakebuild.net/docs/writing-builds/tools/installing-tools)

# Summary
Putting it all together

```cs
using Cake.Common.Diagnostics;
using Cake.Console;
using Cake.Core;

new CakeHostBuilder()
    .WorkingDirectory<WorkingDir>()
    .ContextData<ContextData>()
    .RegisterTasks<CakeTasks>()
    .InstallNugetTool("xunit.runner.console", "2.4.1")
    .RunCakeCli(args);

record WorkingDir(string WorkingDirectory = ".") : IWorkingDirectory;

class ContextData
{
    public string SomeVeryImportantData { get; set; } = "Cake is awesome!";
    public ContextData(ICakeArguments args)
    {
        if (args.HasArgument("tone-down"))
        {
            SomeVeryImportantData = "Cake is pretty good...";
        }
    }
}


class CakeTasks : ICakeTasks
{
    private readonly ICakeContext ctx;

    public CakeTasks(ICakeContext ctx) => this.ctx = ctx;

    public void TaskName(CakeTaskBuilder b) => b
        .Description("Some task")
        .Does(() => ctx.Information("Something"));

    public void AnotherTask(CakeTaskBuilder b) => b
        .IsDependentOn(nameof(TaskName))
        .Does<ContextData>(data => ctx.Information(data.SomeVeryImportantData));
}
```

It is also possible to use [dotnet-script](https://github.com/filipw/dotnet-script).
[Thanks](https://github.com/pitermarx/Cake.Console/issues/11#issuecomment-1102590201) @badsim
See an example in ./dotnet-script/test.csx

```bash
dotnet script --isolated-load-context ./dotnet-script/test.csx --target=test
```
