using System.Threading.Tasks;
using Cake.Cli;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Console.HostBuilderBehaviours;
using Cake.Core;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;
using Cake.Core.Scripting;

namespace Cake.Console
{
    public static class CakeHostBuilderExtensions
    {
        public static CakeHostBuilder InstallNugetTool(this CakeHostBuilder builder, string id, string version) => builder
            .ConfigureSingleton<CakeNugetTool, ICakeToolReference>(new CakeNugetTool(id, version));

        public static CakeHostBuilder RegisterTasks<T>(this CakeHostBuilder builder) where T : class, ICakeTasks => builder
            .ConfigureSingleton<T, ICakeTasks>();

        public static CakeHostBuilder WorkingDirectory<T>(this CakeHostBuilder builder) where T : class, IWorkingDirectory => builder
            .ConfigureSingleton<T, IWorkingDirectory>();

        public static CakeHostBuilder ContextData<T>(this CakeHostBuilder builder, T instance = null) where T : class => builder
            .ConfigureSingleton<T, T>(instance)
            .ConfigureSingleton<SetupContextDataBehaviour<T>, IHostBuilderBehaviour>();

        private static CakeHostBuilder ConfigureSingleton<TImpl, T>(this CakeHostBuilder builder, TImpl instance = null) where TImpl : class => builder
            .ConfigureServices(s =>
            {
                var registration = instance is null
                    ? s.RegisterType<TImpl>()
                    : s.RegisterInstance(instance);
                registration.As<T>().Singleton();
            });

        public static Task RunCakeCli(this CakeHostBuilder builder, string target = null)
            => builder.Build().RunCakeCli(target);

        public static Task RunCakeCli(this IScriptHost host, string target = null)
        {
            if (host.Context.HasArgument("version"))
            {
                new VersionFeature(new VersionResolver()).Run(new CakeConsole(host.Context.Environment));
                return Task.CompletedTask;
            }

            if (host.Context.HasArgument("info"))
            {
                new InfoFeature(new VersionResolver()).Run(new CakeConsole(host.Context.Environment));
                return Task.CompletedTask;
            }

            target ??= host.Context.Argument<string>("target");
            if (target is null)
            {
                host.Context.Error("No target specified");
                return Task.CompletedTask;
            }

            return host.RunTargetAsync(target);
        }
    }
}