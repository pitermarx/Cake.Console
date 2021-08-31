using System.Linq;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Scripting;
using Cake.Core.Tooling;

var context = new MyContext(args); // Extends ICakeContext
var config = new BuildConfiguration(context);
var tasks = new Tasks(context, config);
var printer = new CakeReportPrinter(context.Console, context);

var host = new Cake.Cli.BuildScriptHost(
    new CakeEngine(context.DataService, context.Log),
    new DefaultExecutionStrategy(context.Log),
    context,
    printer,
    context.Log);

host.Setup(ctx => ctx.Information("Setup!"));

tasks.RegisterTasksOn(host);

var report = await host.RunTargetAsync(config.Target);

context.Information("bye");

public class MyContext : ICakeContext
{
    public CakeConsole Console { get; private set; }
    public CakeContext CakeContext { get; private set; }
    public CakeDataService DataService { get; private set; }

    public IFileSystem FileSystem => CakeContext.FileSystem;

    public ICakeEnvironment Environment => CakeContext.Environment;

    public IGlobber Globber => CakeContext.Globber;

    public ICakeLog Log => CakeContext.Log;

    public ICakeArguments Arguments => CakeContext.Arguments;

    public IProcessRunner ProcessRunner => CakeContext.ProcessRunner;

    public IRegistry Registry => CakeContext.Registry;

    public IToolLocator Tools => CakeContext.Tools;

    public ICakeDataResolver Data => CakeContext.Data;

    public ICakeConfiguration Configuration => CakeContext.Configuration;

    public MyContext(string[] args)
    {
        var env = new CakeEnvironment(new CakePlatform(), new CakeRuntime());
        var fs = new FileSystem();
        var globber = new Globber(fs, env);

        Console = new CakeConsole(env);
        var log = new CakeBuildLog(Console, Verbosity.Normal);

        var dic = args
            .Select(a => a.Replace("-", string.Empty).Split("="))
            .ToDictionary(pair => pair[0], pair => pair.Length > 1 ? pair[1] : "true");
        var config = new CakeConfigurationProvider(fs, env).CreateConfiguration(".", dic);
        var arguments = new CakeArguments(dic.ToLookup(v => v.Key, v => v.Value));

        var tools = new ToolLocator(
            env,
            new ToolRepository(env),
            new ToolResolutionStrategy(fs, env, globber, config, log));

        DataService = new CakeDataService();
        CakeContext = new CakeContext(
            fs, env, globber, log, arguments,
            new ProcessRunner(fs, env, log, tools, config),
            new WindowsRegistry(),
            tools, DataService, config);
    }
}

public class BuildConfiguration
{
    public BuildConfiguration(ICakeContext ctx)
    {
        Target = ctx.Argument("target", "");
        Name = ctx.Configuration.GetValue("Name");
    }

    public string Target { get; }
    public string Name { get; }
}

internal class Tasks
{
    private readonly MyContext ctx;
    private readonly BuildConfiguration config;

    public Tasks(MyContext ctx, BuildConfiguration config)
    {
        this.ctx = ctx;
        this.config = config;
    }

    public void RegisterTasksOn(IScriptHost host)
    {
        host
            .Task("Hello")
            .Does(() => ctx.Information($"Hello {config.Name}"));
    }
}
