using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.Scripting;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Cake.Console.HostBuilderBehaviours
{
    internal class TaskRegisteringBehaviour : IHostBuilderBehaviour
    {
        private readonly IScriptHost host;
        private readonly IEnumerable<ICakeTasks> tasks;

        public TaskRegisteringBehaviour(IScriptHost host, IEnumerable<ICakeTasks> tasks)
        {
            this.host = host;
            this.tasks = tasks;
        }

        public void Run()
        {
            foreach (var taskClass in tasks ?? Enumerable.Empty<ICakeTasks>())
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
