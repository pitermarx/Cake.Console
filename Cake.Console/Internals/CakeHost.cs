using System;
using System.Threading.Tasks;
using Cake.Cli;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.Scripting;

namespace Cake.Console.Internals
{
    internal class CakeHost : ScriptHost
    {
        private readonly IExecutionStrategy strategy;
        private readonly IConsole console;
        private readonly ICakeInfoFeature infoFeature;
        private readonly ICakeVersionFeature versionFeature;
        private readonly ICakeReportPrinter printer;

        public CakeHost(
            ICakeEngine engine,
            IConsole console,
            ICakeInfoFeature infoFeature,
            ICakeVersionFeature versionFeature,
            ICakeContext context,
            ICakeReportPrinter printer,
            IExecutionStrategy strategy)
            : base(engine, context)
        {
            this.console = console;
            this.infoFeature = infoFeature;
            this.versionFeature = versionFeature;
            this.printer = printer;
            this.strategy = strategy;
        }

        public override async Task<CakeReport> RunTargetAsync(string target)
        {
            var args = Context.Arguments;
            if (args.HasArgument("version")) versionFeature.Run(console);
            if (args.HasArgument("info")) infoFeature.Run(console);

            if (target is null)
            {
                Context.Information("No target specified");
                return null;
            }

            try
            {
                Settings.SetTarget(target);
                var report = await Engine.RunTargetAsync(Context, strategy, Settings);
                printer.Write(report);

                return report;
            }
            catch (Exception ex)
            {
                Context.Error(ex);
                Environment.Exit(1);
                return null;
            }
        }
    }
}