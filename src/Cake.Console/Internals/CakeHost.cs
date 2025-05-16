using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.Scripting;

namespace Cake.Console.Internals;

// ReSharper disable once ClassNeverInstantiated.Global
internal class CakeHost(
    ICakeEngine engine,
    ICakeContext context,
    ICakeReportPrinter printer,
    IExecutionStrategy strategy
) : ScriptHost(engine, context)
{
    public override Task<CakeReport>? RunTargetAsync(string? target)
    {
        if (target is null)
        {
            Context.Information("No target specified");
            return null;
        }

        Settings.SetTarget(target);

        return Run();
    }

    public override Task<CakeReport>? RunTargetsAsync(IEnumerable<string>? targets)
    {
        var enumerable = targets as string[] ?? targets?.ToArray() ?? [];
        if (enumerable.Length == 0)
        {
            Context.Information("No targets specified");
            return null;
        }

        Settings.SetTargets(enumerable);

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
