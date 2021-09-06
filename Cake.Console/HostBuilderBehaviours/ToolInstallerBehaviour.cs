using System;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.Packaging;
using System.Collections.Generic;
using System.Linq;
using Cake.Common.Diagnostics;

namespace Cake.Console.HostBuilderBehaviours
{
    internal class ToolInstallerBehaviour : IHostBuilderBehaviour
    {
        private readonly ICakeContext ctx;
        private readonly IReadOnlyList<ICakeToolReference> tools;
        private readonly IReadOnlyList<IPackageInstaller> installers;

        public ToolInstallerBehaviour(ICakeContext ctx, IEnumerable<ICakeToolReference> tools, IEnumerable<IPackageInstaller> installers)
        {
            this.tools = tools?.ToArray() ?? Array.Empty<ICakeToolReference>();
            this.installers = installers?.ToArray() ?? Array.Empty<IPackageInstaller>();
            this.ctx = ctx;
        }

        public void Run()
        {
            var root = ctx.Configuration.GetToolPath(".", ctx.Environment);

            foreach (var tool in tools.Select(t => t.Reference))
            {
                ctx.Debug("Installing tool '{0}'...", tool.Package);
                var installer = installers.FirstOrDefault(i => i.CanInstall(tool, PackageType.Tool));
                if (installer == null)
                {
                    throw new Exception($"Could not find an installer for the '{tool.Scheme}' scheme.");
                }

                var result = installer.Install(tool, PackageType.Tool, root);
                if (result.Count == 0)
                {
                    throw new Exception($"Failed to install tool '{tool.Package}'.");
                }

                foreach (var item in result) ctx.Tools.RegisterFile(item.Path);
            }
        }
    }
}
