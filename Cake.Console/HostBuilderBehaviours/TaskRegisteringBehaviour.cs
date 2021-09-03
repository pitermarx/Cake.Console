using System;
using Microsoft.Extensions.DependencyInjection;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.Scripting;
using System.Reflection;
using System.Linq;

namespace Cake.Console.HostBuilderBehaviours
{
    internal class TaskRegisteringBehaviour : IHostBuilderBehaviour
    {
        private readonly IScriptHost host;

        public TaskRegisteringBehaviour(IScriptHost host) => this.host = host;

        public void Run(IServiceProvider provider)
        {
            foreach (var taskClass in provider.GetServices<ICakeTasks>())
            {
                var tasks = taskClass.GetType()
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
                    t.Invoke(taskClass, new[] { host.Task(t.Name) });
                }
            }
        }
    }
}
