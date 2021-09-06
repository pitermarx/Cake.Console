using Cake.Common.Diagnostics;
using Cake.Core.Scripting;

namespace Cake.Console.HostBuilderBehaviours
{
    internal class SetupContextDataBehaviour<T> : IHostBuilderBehaviour
        where T : class
    {
        private readonly IScriptHost host;
        private readonly T data;

        public SetupContextDataBehaviour(IScriptHost host, T data)
        {
            this.host = host;
            this.data = data;
        }

        public void Run() => host.Setup(_ =>
        {
            host.Context.Information($"Setting up context data <{typeof(T).Name}>");
            return data;
        });
    }
}
