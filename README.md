# Cake.Console

An alternative to Cake.Frosting

https://blog.pitermarx.com/2021/09/presenting-cake-console/

## Usage

```cs
await new CakeHostBuilder(args)
    .WorkingDirectory<MyWorkingDirectory>()
    .ContextData<Data>()
    .RegisterTasks<Tasks>()
    .InstallNugetTool("xunit.runner.console", "2.4.1")
    .Run();
```
