using Cake.Common.IO;
using Cake.Common.Diagnostics;
using Cake.Core;
using System.Collections.Generic;
using System.Linq;

namespace Cake.Console.HostBuilderBehaviours
{
    internal class WorkingDirectoryBehaviour : IHostBuilderBehaviour
    {
        private readonly ICakeContext ctx;
        private readonly IWorkingDirectory? workingDirectory;

        public WorkingDirectoryBehaviour(ICakeContext ctx, IEnumerable<IWorkingDirectory> workingDirectory)
        {
            this.ctx = ctx;
            this.workingDirectory = workingDirectory.SingleOrDefault();
        }

        public void Run()
        {
            if (workingDirectory is IWorkingDirectory wd)
            {
                var dir = ctx.Directory(wd.WorkingDirectory).Path.MakeAbsolute(ctx.Environment);
                ctx.Environment.WorkingDirectory = dir;
                ctx.Debug($"Working directory changed to '{dir}'");
            }
        }
    }
}
