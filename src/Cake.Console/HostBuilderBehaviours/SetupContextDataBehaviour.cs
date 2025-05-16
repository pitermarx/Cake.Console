using Cake.Common.Diagnostics;
using Cake.Core.Scripting;

namespace Cake.Console.HostBuilderBehaviours;

// ReSharper disable once ClassNeverInstantiated.Global
internal class SetupContextDataBehaviour<T>(IScriptHost host, T data) : IHostBuilderBehaviour
    where T : class
{
    public void Run() =>
        host.Setup(_ =>
        {
            host.Context.Information($"Setting up context data <{typeof(T).Name}>");
            return data;
        });
}
