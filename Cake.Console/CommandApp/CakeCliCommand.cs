using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Cake.Cli;
using Cake.Console.Internals;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Scripting;
using Spectre.Console.Cli;

namespace Cake.Console.CommandApp
{
    internal sealed class CakeCliCommand : Command<CakeCliCommand.Settings>
    {
        private readonly CakeHostBuilder builder;
        private readonly ICakeVersionFeature versionFeature;
        private readonly ICakeInfoFeature infoFeature;
        private readonly IConsole console;

        public CakeCliCommand(
            CakeHostBuilder builder,
            ICakeVersionFeature versionFeature,
            ICakeInfoFeature infoFeature,
            IConsole console)
        {
            this.builder = builder;
            this.versionFeature = versionFeature;
            this.infoFeature = infoFeature;
            this.console = console;
        }

        public sealed class Settings : CommandSettings
        {
            [CommandOption("--target|-t <TARGET>")]
            [DefaultValue("Default")]
            [Description("Target task to invoke.")]
            public string Target { get; set; }

            [CommandOption("--verbosity|-v <VERBOSITY>")]
            [Description("Specifies the amount of information to be displayed.\n(Quiet, Minimal, Normal, Verbose, Diagnostic)")]
            [TypeConverter(typeof(VerbosityConverter))]
            [DefaultValue(Verbosity.Normal)]
            public Verbosity Verbosity { get; set; }

            [CommandOption("--description|--descriptions|--showdescription|--showdescriptions")]
            [Description("Shows description for each task.")]
            public bool Description { get; set; }

            [CommandOption("--tree|--showtree")]
            [Description("Shows the task dependency tree.")]
            public bool Tree { get; set; }

            [CommandOption("--dryrun|--noop|--whatif")]
            [Description("Performs a dry run.")]
            public bool DryRun { get; set; }

            [CommandOption("--exclusive|-e")]
            [Description("Executes the target task without any dependencies.")]
            public bool Exclusive { get; set; }

            [CommandOption("--version|--ver")]
            [Description("Displays version information.")]
            public bool Version { get; set; }

            [CommandOption("--info")]
            [Description("Displays additional information about Cake.")]
            public bool Info { get; set; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            if (settings.Info)
            {
                infoFeature.Run(console);
                return 0;
            }

            if (settings.Version)
            {
                versionFeature.Run(console);
                return 0;
            }

            ICakeArguments args = new CakeConsoleArguments(context.Remaining.Parsed, settings.Verbosity);
            var host = settings switch
            {
                {DryRun: true} => builder.Build<DryRunScriptHost>(args),
                {Description: true} => builder.Build<DescriptionScriptHost>(args),
                {Tree: true} => builder.Build<TreeScriptHost>(args),
                _ => builder.Build<CakeHost>(args)
            };

            if (settings.Exclusive && host is ScriptHost h) h.Settings.UseExclusiveTarget();

            host.RunTarget(settings.Target);

            return 0;
        }
    }
}