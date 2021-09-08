using System;
using System.Linq;
using Cake.Cli;
using Cake.Common.Diagnostics;
using Cake.Console.CommandApp;
using Cake.Console.HostBuilderBehaviours;
using Cake.Console.Internals;
using Cake.Core;
using Cake.Core.Composition;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Core.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Cake.Console
{
    public class CakeHostBuilder
    {
        private readonly CakeContainer cakeContainer = new();

        public CakeHostBuilder ConfigureServices(Action<ICakeContainerRegistrar> action)
        {
            action(cakeContainer);
            return this;
        }

        public IScriptHost Build(string[] args)
            => Build<CakeHost>(new CakeConsoleArguments(args));

        internal IScriptHost Build<T>(ICakeArguments arguments)
            where T : class, IScriptHost
        {
            var provider = CreateServiceProvider<T>(arguments);

            var host = provider.GetService<IScriptHost>();
            foreach (var behaviour in provider.GetServices<IHostBuilderBehaviour>())
            {
                var type = behaviour.GetType();
                var name = type.Name;
                if (type.IsGenericType)
                {
                    name = name.Replace("`1", string.Empty);
                    name += $"<{string.Join(", ", type.GenericTypeArguments.Select(t => t.Name))}>";
                }
                host.Context.Debug($"Applying {name}");
                behaviour.Run();
            }
            return host;
        }

        internal IServiceProvider CreateServiceProvider<T>(ICakeArguments arguments)
            where T : class, IScriptHost
        {
            var collection = cakeContainer.BuildServiceCollection();

            // behaviours
            collection.AddSingleton<IHostBuilderBehaviour, WorkingDirectoryBehaviour>();
            collection.AddSingleton<IHostBuilderBehaviour, ToolInstallerBehaviour>();
            collection.AddSingleton<IHostBuilderBehaviour, TaskRegisteringBehaviour>();

            collection.AddSingleton<IScriptHost, T>();
            collection.AddSingleton<ICakeArguments>(arguments);
            collection.AddSingleton<ICakeConfiguration>(s => s
                .GetService<CakeConfigurationProvider>()
                .CreateConfiguration(".", arguments
                    .GetArguments()
                    .ToDictionary(a => a.Key, a => a.Value.First())));

            return collection.BuildServiceProvider();
        }

        public int RunCakeCli(string[] args)
        {
            var services = cakeContainer.BuildServiceCollection();
            services.AddSingleton(this);

            // features
            services.AddSingleton<IVersionResolver, VersionResolver>();
            services.AddSingleton<ICakeVersionFeature, VersionFeature>();
            services.AddSingleton<ICakeInfoFeature, InfoFeature>();

            return new CommandApp<CakeCliCommand>(new TypeRegistrar(services)).Run(args);
        }
    }
}