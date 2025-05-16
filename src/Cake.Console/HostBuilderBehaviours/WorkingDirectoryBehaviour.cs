using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core;

namespace Cake.Console.HostBuilderBehaviours;

// ReSharper disable once ClassNeverInstantiated.Global
internal class WorkingDirectoryBehaviour(
    ICakeContext ctx,
    IEnumerable<IWorkingDirectory> workingDirectory
) : IHostBuilderBehaviour
{
    private readonly IWorkingDirectory? workingDirectory = workingDirectory.SingleOrDefault();

    public void Run()
    {
        if (workingDirectory == null)
            return;
        var dir = ctx.Directory(workingDirectory.WorkingDirectory)
            .Path.MakeAbsolute(ctx.Environment);
        ctx.Environment.WorkingDirectory = dir;
        ctx.Debug($"Working directory changed to '{dir}'");
    }
}
