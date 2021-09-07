using Cake.Cli;
using Cake.Core;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;

namespace Cake.Console.Internals
{
    internal class CakeConsoleLog : ICakeLog
    {
        private readonly CakeBuildLog log;

        public Verbosity Verbosity
        {
            get => log.Verbosity;
            set => log.Verbosity = value;
        }

        public CakeConsoleLog(ICakeConfiguration config, IConsole console)
        {
            log = new CakeBuildLog(console, Verbosity.Normal);
            if ((config.GetValue("verbosity") ?? config.GetValue("v")) is string v)
            {
                log.Verbosity = (Verbosity)new VerbosityConverter().ConvertFrom(v);
            }
        }

        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
            => log.Write(verbosity, level, format, args);
    }
}