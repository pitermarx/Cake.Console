using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Core;

namespace Cake.Console.Internals
{
    internal class CakeConsoleArguments : ICakeArguments
    {
        private readonly IDictionary<string, ICollection<string>> arguments;

        public CakeConsoleArguments(string[] args)
        {
            // very naive argument parsing.
            // send help
            arguments = args
                .Select(a => a.Replace("-", string.Empty).Split("="))
                .ToDictionary(
                    pair => pair[0].ToLowerInvariant(),
                    pair => (ICollection<string>)new[]{pair.Length > 1 ? pair[1] : "true"});
        }

        public ICollection<string> GetArguments(string name)
            => HasArgument(name) ? arguments[name.ToLowerInvariant()] : Array.Empty<string>();

        public IDictionary<string, ICollection<string>> GetArguments()
            => arguments;

        public bool HasArgument(string name)
            => arguments.ContainsKey(name.ToLowerInvariant());
    }
}