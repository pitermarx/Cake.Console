using System;
using Microsoft.Extensions.DependencyInjection;
using Cake.Common.IO;
using Cake.Common.Diagnostics;
using Cake.Core;

namespace Cake.Console.HostBuilderBehaviours
{
    internal class WorkingDirectoryBehaviour : IHostBuilderBehaviour
    {
        private readonly ICakeContext ctx;

        public WorkingDirectoryBehaviour(ICakeContext ctx) => this.ctx = ctx;

        public void Run(IServiceProvider provider)
        {
            if (provider.GetService<IWorkingDirectory>() is IWorkingDirectory wd)
            {
                var dir = ctx.Directory(wd.WorkingDirectory).Path.MakeAbsolute(ctx.Environment);
                ctx.Environment.WorkingDirectory = dir;
                ctx.Debug($"Working directory changed to '{dir}'");
            }
        }
    }
}
