using System;

namespace Cake.Console.HostBuilderBehaviours
{
    internal interface IHostBuilderBehaviour
    {
        void Run(IServiceProvider provider);
    }
}