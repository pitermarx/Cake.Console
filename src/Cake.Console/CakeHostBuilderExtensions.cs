using Cake.Console.HostBuilderBehaviours;
using Cake.Console.Internals;
using Cake.Core.Composition;

namespace Cake.Console;

public static class CakeHostBuilderExtensions
{
    public static CakeHostBuilder InstallDotnetTool(
        this CakeHostBuilder builder,
        string id,
        string version
    ) =>
        builder.ConfigureServices(s =>
            s.RegisterInstance(new CakeDotnetTool(id, version)).As<ICakeToolReference>().Singleton()
        );

    public static CakeHostBuilder InstallNugetTool(
        this CakeHostBuilder builder,
        string id,
        string version
    ) =>
        builder.ConfigureServices(s =>
            s.RegisterInstance(new CakeNugetTool(id, version)).As<ICakeToolReference>().Singleton()
        );

    public static CakeHostBuilder RegisterTasks<T>(this CakeHostBuilder builder)
        where T : class, ICakeTasks =>
        builder.ConfigureServices(s => s.RegisterType(typeof(T)).As<ICakeTasks>().Singleton());

    public static CakeHostBuilder WorkingDirectory<T>(this CakeHostBuilder builder)
        where T : class, IWorkingDirectory =>
        builder.ConfigureServices(s =>
            s.RegisterType(typeof(T)).As<IWorkingDirectory>().Singleton()
        );

    public static CakeHostBuilder ContextData<T>(this CakeHostBuilder builder, T? instance = null)
        where T : class =>
        builder
            .ConfigureServices(s => s.RegisterInstance(instance).As<T>().Singleton())
            .ConfigureServices(s =>
                s.RegisterType<SetupContextDataBehaviour<T>>()
                    .As<IHostBuilderBehaviour>()
                    .Singleton()
            );
}
