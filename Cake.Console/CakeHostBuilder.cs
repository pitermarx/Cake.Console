using System;
using Cake.Common.Diagnostics;
using Cake.Console.HostBuilderBehaviours;
using Cake.Console.Internals;
using Cake.Core.Composition;
using Cake.Core.Scripting;
using Microsoft.Extensions.DependencyInjection;

namespace Cake.Console
{
    public class CakeHostBuilder
    {
        private readonly string[] args;
        private readonly CakeContainer cakeContainer = new();

        public CakeHostBuilder(string[] args)
            => this.args = args;

        public CakeHostBuilder ConfigureServices(Action<ICakeContainerRegistrar> action)
        {
            action(cakeContainer);
            return this;
        }

        public IScriptHost Build()
        {
            var provider = cakeContainer.Build(args);

            var host = provider.GetService<IScriptHost>();
            foreach (var behaviour in provider.GetServices<IHostBuilderBehaviour>())
            {
                host.Context.Debug($"Applying {behaviour.GetType().Name}");
                behaviour.Run();
            }

            return host;
        }
    }
}