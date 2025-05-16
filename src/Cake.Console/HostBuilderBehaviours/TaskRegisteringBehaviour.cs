using System.Reflection;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.Scripting;

namespace Cake.Console.HostBuilderBehaviours;

// ReSharper disable once ClassNeverInstantiated.Global
internal class TaskRegisteringBehaviour(IScriptHost host, IEnumerable<ICakeTasks> tasks)
    : IHostBuilderBehaviour
{
    public void Run()
    {
        foreach (var taskClass in tasks ?? [])
        {
            var classTasks = taskClass
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m =>
                {
                    var parameters = m.GetParameters();

                    return parameters.Length == 1
                        && parameters[0].ParameterType == typeof(CakeTaskBuilder)
                        && m.ReturnType == typeof(void);
                });

            foreach (var t in classTasks)
            {
                host.Context.Debug($"Registering task {t.Name}");
                t.Invoke(taskClass, [host.Task(t.Name)]);
            }
        }
    }
}
