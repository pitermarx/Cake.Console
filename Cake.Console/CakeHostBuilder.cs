using System;
using System.Linq;
using System.Reflection;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core;
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
            var provider = cakeContainer
                .RegisterServices(args)
                .Build();

            var host = provider.GetService<IScriptHost>();
            if (provider.GetService<IWorkingDirectory>() is IWorkingDirectory wd)
            {
                var dir = host.Context.Directory(wd.WorkingDirectory).Path.MakeAbsolute(host.Context.Environment);
                host.Context.Environment.WorkingDirectory = dir;
                host.Context.Debug($"Working directory changed to '{dir}'");
            }

            var toolInstaller = provider.GetService<ToolInstaller>();
            var tools = provider.GetServices<ICakeToolReference>();
            foreach (var t in tools) toolInstaller.Install(t.Reference);

            var tasks = provider.GetServices<ICakeTasks>();
            foreach (var s in tasks) RegisterTasks(host, s);

            var actions = provider.GetServices<IPostBuildAction>();
            foreach (var a in actions) a.Invoke(provider);

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
                host.Context.Debug($"Registering task {t.Name}");
                t.Invoke(service, new[] { host.Task(t.Name) });
            }
        }
    }
}