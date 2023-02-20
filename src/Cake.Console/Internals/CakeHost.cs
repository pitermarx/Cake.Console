using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.Scripting;

namespace Cake.Console.Internals
{
    internal class CakeHost : ScriptHost
    {
        private readonly IExecutionStrategy strategy;
        private readonly ICakeReportPrinter printer;

        public CakeHost(
            ICakeEngine engine,
            ICakeContext context,
            ICakeReportPrinter printer,
            IExecutionStrategy strategy)
            : base(engine, context)
        {
            this.printer = printer;
            this.strategy = strategy;
        }

        public override Task<CakeReport>? RunTargetAsync(string target)
        {
            if (target is null)
            {
                Context.Information("No target specified");
                return null;
            }

            Settings.SetTarget(target);

            return Run();
        }

        public override Task<CakeReport>? RunTargetsAsync(IEnumerable<string> targets)
        {
            if (targets?.Any() != true)
            {
                Context.Information("No targets specified");
                return null;
            }

            Settings.SetTargets(targets);

            return Run();
        }

        private async Task<CakeReport> Run()
        {
            try
            {
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