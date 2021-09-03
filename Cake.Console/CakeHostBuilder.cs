using System;
using System.Threading.Tasks;
using Cake.Common;
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

        public Task Run(string defaultTarget = null)
        {
            var host = Build();
            if (host.Context.Argument("Target", defaultTarget) is string t)
            {
                return host.RunTargetAsync(t);
            }

            host.Context.Error("No target specified");
            return Task.CompletedTask;
        }

        public IScriptHost Build()
        {
            var provider = cakeContainer.Build(args);

            var host = provider.GetService<IScriptHost>();
            foreach (var behaviour in provider.GetServices<IHostBuilderBehaviour>())
            {
                host.Context.Debug($"Applying {behaviour.GetType().Name}");
                behaviour.Run(provider);
            }

            return host;
        }
    }
}