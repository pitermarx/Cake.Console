using System.Threading.Tasks;
using Cake.Common;
using Cake.Console.HostBuilderBehaviours;
using Cake.Core.Composition;

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

        public static Task Run(this CakeHostBuilder builder, string defaultTarget = null)
        {
            var host = builder.Build();
            var target = defaultTarget ?? host.Context.Argument<string>("target");
            return host.RunTargetAsync(target);
        }
    }
}