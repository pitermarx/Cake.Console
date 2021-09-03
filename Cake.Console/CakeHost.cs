using System;
using System.Threading.Tasks;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.Scripting;

namespace Cake.Console
{
    internal class CakeHost : ScriptHost
    {
        private readonly IExecutionStrategy strategy;
        private readonly ICakeReportPrinter printer;

        public CakeHost(ICakeEngine engine, ICakeContext context, ICakeReportPrinter printer, IExecutionStrategy strategy)
            : base(engine, context)
        {
            this.printer = printer;
            this.strategy = strategy;
        }

        public override async Task<CakeReport> RunTargetAsync(string target)
        {
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