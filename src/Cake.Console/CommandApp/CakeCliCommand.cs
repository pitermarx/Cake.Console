using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Cake.Cli;
using Cake.Console.Internals;
using Cake.Core;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;
using Cake.Core.Scripting;
using Spectre.Console.Cli;

namespace Cake.Console.CommandApp;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class CakeCliCommand(
    CakeHostBuilder builder,
    ICakeVersionFeature versionFeature,
    ICakeInfoFeature infoFeature,
    IConsole console
) : Command<CakeCliCommand.Settings>
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--target|-t <TARGET>")]
        [DefaultValue("Default")]
        [Description("Target task to invoke.")]
        public string? Target { get; set; }

        [CommandOption("--verbosity|-v <VERBOSITY>")]
        [Description(
            "Specifies the amount of information to be displayed.\n(Quiet, Minimal, Normal, Verbose, Diagnostic)"
        )]
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

        ScriptHost host = settings switch
        {
            { DryRun: true } => ConfigureAndBuild<DryRunScriptHost>(),
            { Description: true } => ConfigureAndBuild<DescriptionScriptHost>(),
            { Tree: true } => ConfigureAndBuild<TreeScriptHost>(),
            _ => ConfigureAndBuild<CakeHost>(),
        };

        if (settings.Exclusive)
            host.Settings.UseExclusiveTarget();

        host.RunTarget(settings.Target);

        return 0;

        T ConfigureAndBuild<T>()
            where T : class, IScriptHost
        {
            var args = new CakeConsoleArguments(context.Remaining.Parsed!, settings.Verbosity);
            return builder
                .ConfigureServices(s => s.RegisterType<T>().AsSelf().Singleton())
                .BuildScriptHost<T>(args);
        }
    }
}
