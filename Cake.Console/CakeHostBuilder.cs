using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cake.Core;
using Cake.Core.Composition;
using Cake.Core.Packaging;
using Cake.Core.Scripting;
using Microsoft.Extensions.DependencyInjection;

namespace Cake.Console
{
    public class CakeHostBuilder
    {
        private readonly string[] args;
        private readonly List<PackageReference> tools = new();
        private readonly CakeContainer cakeContainer = new();

        public CakeHostBuilder(string[] args)
            => this.args = args;

        public CakeHostBuilder WithTool(PackageReference toInstall)
        {
            tools.Add(toInstall);
            return this;
        }

        public CakeHostBuilder Configure(Action<ICakeContainerRegistrar> action)
        {
            action(cakeContainer);
            return this;
        }

        public IScriptHost Build()
        {
            var provider = cakeContainer
                .RegisterServices(args)
                .Build();

            var host = provider.GetService<IScriptHost>();
            var toolInstaller = provider.GetService<ToolInstaller>();
            var tasks = provider.GetServices<ICakeTasks>();

            foreach (var t in tools) toolInstaller.Install(t);
            foreach (var s in tasks) RegisterTasks(host, s);

            return host;
        }
        private static void RegisterTasks(IScriptHost host, ICakeTasks service)
        {
            var tasks = service.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m =>
                {
                    var parameters = m.GetParameters();

                    return parameters.Length == 1 &&
                        parameters[0].ParameterType == typeof(CakeTaskBuilder) &&
                        m.ReturnType == typeof(void);
                });

            foreach (var t in tasks)
            {
                t.Invoke(service, new[] { host.Task(t.Name) });
            }
        }
    }
}