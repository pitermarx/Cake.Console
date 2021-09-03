using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Core;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Core.Packaging;
using Cake.Core.Tooling;

namespace Cake.Console
{
    internal sealed class ToolInstaller
    {
        private readonly ICakeEnvironment environment;
        private readonly IToolLocator locator;
        private readonly ICakeConfiguration configuration;
        private readonly ICakeLog log;
        private readonly List<IPackageInstaller> installers;

        public ToolInstaller(
            ICakeEnvironment environment,
            IToolLocator locator,
            ICakeConfiguration configuration,
            ICakeLog log,
            IEnumerable<IPackageInstaller> installers)
        {
            this.environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this.locator = locator ?? throw new ArgumentNullException(nameof(locator));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.installers = new List<IPackageInstaller>(installers ?? Enumerable.Empty<IPackageInstaller>());
        }

        public void Install(PackageReference tool)
        {
            // Get the tool path.
            var root = configuration.GetToolPath(".", environment);

            // Get the installer.
            var installer = installers.FirstOrDefault(i => i.CanInstall(tool, PackageType.Tool));
            if (installer == null)
            {
                throw new Exception($"Could not find an installer for the '{tool.Scheme}' scheme.");
            }

            // Install the tool.
            log.Debug("Installing tool '{0}'...", tool.Package);
            var result = installer.Install(tool, PackageType.Tool, root);
            if (result.Count == 0)
            {
                throw new Exception($"Failed to install tool '{tool.Package}'.");
            }

            // Register the tools.
            foreach (var item in result) locator.RegisterFile(item.Path);
        }
    }
}
