using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Common.Diagnostics;
using Cake.Console.CommandApp;
using Cake.Console.HostBuilderBehaviours;
using Cake.Console.Internals;
using Cake.Core;
using Cake.Core.Composition;
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

        internal T BuildScriptHost<T>(ICakeArguments args)
            where T : class, IScriptHost
        {
            var provider = cakeContainer
                .AsServiceCollection()
                .AddSingleton(args)
                .AddSingleton<T, T>()
                .AddSingleton<IScriptHost, T>()
                .BuildServiceProvider();

            var host = provider.GetService<T>();

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

        public IScriptHost BuildHost(IEnumerable<string> args)
            => BuildScriptHost<CakeHost>(new CakeConsoleArguments(args));

        public int RunCakeCli(IEnumerable<string> args)
        {
            cakeContainer.RegisterInstance(this).AsSelf().Singleton();
            return new CommandApp<CakeCliCommand>(cakeContainer).Run(args);
        }
    }
}