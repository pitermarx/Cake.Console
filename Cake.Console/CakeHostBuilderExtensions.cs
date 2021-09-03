using Cake.Core.Composition;
using Cake.Core.Scripting;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            builder.ConfigureServices(s => s.RegisterType<SetupDataAction<T>>().As<IPostBuildAction>().Singleton());
            return builder;
        }

        internal class SetupDataAction<T> : IPostBuildAction
            where T : class
        {
            private readonly IScriptHost host;

            public SetupDataAction(IScriptHost host) => this.host = host;

            public void Invoke(IServiceProvider provider) => host.Setup(_ => provider.GetRequiredService<T>());
        }
    }
}