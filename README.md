# Cake.Console

An alternative to Cake.Frosting

![Nuget](https://img.shields.io/nuget/v/cake.console?label=cake.console&style=plastic)

https://blog.pitermarx.com/2021/09/presenting-cake-console/

## Usage

```cs
await new CakeHostBuilder()
    .WorkingDirectory<MyWorkingDirectory>()
    .ContextData<Data>()
    .RegisterTasks<Tasks>()
    .InstallNugetTool("xunit.runner.console", "2.4.1")
    .RunCakeCli(args);
```
