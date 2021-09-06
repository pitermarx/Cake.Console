using System.Threading.Tasks;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Console.HostBuilderBehaviours;
using Cake.Core.Composition;

namespace Cake.Console
{
    public static class CakeHostBuilderExtensions
    {
        public static CakeHostBuilder InstallNugetTool(this CakeHostBuilder builder, string id, string version)
        {
            builder.ConfigureServices(s => s.RegisterInstance(new CakeNugetTool(id, version)).As<ICakeToolReference>());
            return builder;
        }

        public static CakeHostBuilder RegisterTasks<T>(this CakeHostBuilder builder) where T : ICakeTasks
        {
            builder.ConfigureServices(s => s.RegisterType<T>().As<ICakeTasks>().Singleton());
            return builder;
        }

        public static CakeHostBuilder WorkingDirectory<T>(this CakeHostBuilder builder) where T : IWorkingDirectory
        {
            builder.ConfigureServices(s => s.RegisterType<T>().As<IWorkingDirectory>().Singleton());
            return builder;
        }

        public static CakeHostBuilder ContextData<T>(this CakeHostBuilder builder) where T : class
        {
            builder.ConfigureServices(s => s.RegisterType<T>().AsSelf().Singleton());
            builder.ConfigureServices(s => s.RegisterType<SetupContextDataBehaviour<T>>().As<IHostBuilderBehaviour>().Singleton());
            return builder;
        }

        public static CakeHostBuilder ContextData<T>(this CakeHostBuilder builder, T instance) where T : class
        {
            builder.ConfigureServices(s => s.RegisterInstance(instance).AsSelf().Singleton());
            builder.ConfigureServices(s => s.RegisterType<SetupContextDataBehaviour<T>>().As<IHostBuilderBehaviour>().Singleton());
            return builder;
        }

        public static Task Run(this CakeHostBuilder builder, string defaultTarget = null)
        {
            var host = builder.Build();
            if (host.Context.Argument("Target", defaultTarget) is string t)
            {
                return host.RunTargetAsync(t);
            }

            host.Context.Error("No target specified");
            return Task.CompletedTask;
        }
    }
}