using System;

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

        public void Run() => host.Setup(_ => data);
    }
}
