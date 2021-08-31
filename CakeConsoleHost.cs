using System;
using System.Linq;
using System.Threading.Tasks;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Scripting;
using Cake.Core.Tooling;

namespace Cake.Console
{
    public static class CakeConsoleAppHost
    {
        public static IScriptHost ScriptHost { get; } = GetScriptHost();

        private static IScriptHost GetScriptHost()
        {
            var fileSystem = new FileSystem();
            var data = new CakeDataService();
            var environment = new CakeEnvironment(new CakePlatform(), new CakeRuntime());
            var console = new CakeConsole(environment);
            var log = new CakeBuildLog(console);
            var globber = new Globber(fileSystem, environment);

            var dic = Environment.GetCommandLineArgs()
                .Select(a => a.Replace("-", string.Empty).Split("="))
                .ToDictionary(pair => pair[0], pair => pair.Length > 1 ? pair[1] : "true");
            var configuration = new CakeConfigurationProvider(fileSystem, environment).CreateConfiguration(".", dic);
            var arguments = new CakeArguments(dic.ToLookup(v => v.Key, v => v.Value));

            var tools = new ToolLocator(
                        environment,
                        new ToolRepository(environment),
                        new ToolResolutionStrategy(
                            fileSystem,
                            environment,
                            globber,
                            configuration,
                            log
                            )
                        );
            ICakeContext context = new CakeContext(
                    fileSystem,
                    environment,
                    globber,
                    log,
                    arguments,
                    new ProcessRunner(fileSystem, environment, log, tools, configuration),
                    new WindowsRegistry(),
                    tools,
                    data,
                    configuration
                );

            return new ConsoleScriptHost(new CakeEngine(data, log), context)
            {
                Strategy = new DefaultExecutionStrategy(log),
                Reporter = new CakeReportPrinter(console, context),
                Arguments = arguments
            };
        }

        internal class ConsoleScriptHost : ScriptHost
        {
            public IExecutionStrategy Strategy { get; init; }
            public ICakeReportPrinter Reporter { get; init; }
            public ICakeArguments Arguments { get; init; }

            public ConsoleScriptHost(ICakeEngine engine, ICakeContext context) : base(engine, context)
            {
            }

            public override async Task<CakeReport> RunTargetAsync(string target)
            {
                try
                {
                    if (Arguments.HasArgument("exclusive") && !StringComparer.OrdinalIgnoreCase.Equals("false", Arguments.GetArguments("exclusive").FirstOrDefault()))
                    {
                        Settings.UseExclusiveTarget();
                    }
                    Settings.SetTarget(target);
                    var report = await Engine.RunTargetAsync(Context, Strategy, Settings);
                    Reporter.Write(report);
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
}