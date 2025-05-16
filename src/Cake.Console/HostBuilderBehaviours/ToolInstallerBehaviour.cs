using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.Packaging;

namespace Cake.Console.HostBuilderBehaviours;

// ReSharper disable once ClassNeverInstantiated.Global
internal class ToolInstallerBehaviour(
    ICakeContext ctx,
    IEnumerable<ICakeToolReference> tools,
    IEnumerable<IPackageInstaller> installers
) : IHostBuilderBehaviour
{
    private readonly IReadOnlyList<ICakeToolReference> tools = tools?.ToArray() ?? [];
    private readonly IReadOnlyList<IPackageInstaller> installers = installers?.ToArray() ?? [];

    public void Run()
    {
        var root = ctx.Configuration.GetToolPath(".", ctx.Environment);

        foreach (var tool in tools.Select(t => t.Reference))
        {
            ctx.Debug("Installing tool '{0}'...", tool.Package);
            var installer =
                installers.FirstOrDefault(i => i.CanInstall(tool, PackageType.Tool))
                ?? throw new Exception(
                    $"Could not find an installer for the '{tool.Scheme}' scheme."
                );
            var result = installer.Install(tool, PackageType.Tool, root);
            if (result.Count == 0)
            {
                throw new Exception($"Failed to install tool '{tool.Package}'.");
            }

            foreach (var item in result)
                ctx.Tools.RegisterFile(item.Path);
        }
    }
}
