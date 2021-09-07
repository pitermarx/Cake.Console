using System;
using System.Linq;
using Cake.Common.Diagnostics;
using Cake.Console.HostBuilderBehaviours;
using Cake.Console.Internals;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;
using Cake.Core.Scripting;
using Microsoft.Extensions.DependencyInjection;

namespace Cake.Console
{
    public class CakeHostBuilder
    {
        private readonly CakeContainer cakeContainer;

        public CakeHostBuilder(string[] args)
            => cakeContainer = new CakeContainer(new CakeConsoleArguments(args));

        public CakeHostBuilder ConfigureServices(Action<ICakeContainerRegistrar> action)
        {
            action(cakeContainer);
            return this;
        }

        public IScriptHost Build()
        {
            var provider = cakeContainer.Build();

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
    }
}