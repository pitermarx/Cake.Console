using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Core;

namespace Cake.Console.Internals
{
    internal class CakeConsoleArguments : ICakeArguments
    {
        private readonly IDictionary<string, ICollection<string>> arguments;

        public CakeConsoleArguments(IEnumerable<string> args)
        {
            // very naive argument parsing.
            // send help
            arguments = args
                .Select(a => Trim(a))
                .Select(a => a.Replace("-", string.Empty).Split("="))
                .ToDictionary(
                    pair => pair[0].ToLowerInvariant(),
                    pair => (ICollection<string>)new[]{pair.Length > 1 ? pair[1] : "true"});
        }

        public CakeConsoleArguments(ILookup<string, string> parsed, Core.Diagnostics.Verbosity verbosity)
        {
            arguments = parsed.ToDictionary(p => p.Key, p => (ICollection<string>)p.ToArray());
            arguments["verbosity"] = new []{ verbosity.ToString() };
        }

        public ICollection<string> GetArguments(string name)
            => HasArgument(name) ? arguments[name.ToLowerInvariant()] : Array.Empty<string>();

        public IDictionary<string, ICollection<string>> GetArguments()
            => arguments;

        public bool HasArgument(string name)
            => arguments.ContainsKey(name.ToLowerInvariant());

        private static string Trim(string arg)
        {
            arg = arg.Trim();
            var idx = 0;
            while (arg.StartsWith("-")) idx++;
            return arg[idx..];
        }
    }
}