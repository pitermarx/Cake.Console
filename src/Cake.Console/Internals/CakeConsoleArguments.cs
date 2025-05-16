using Cake.Core;

namespace Cake.Console.Internals;

internal class CakeConsoleArguments : ICakeArguments
{
    private readonly IDictionary<string, ICollection<string>> arguments;

    public CakeConsoleArguments(IEnumerable<string> args)
    {
        // very naive argument parsing.
        // send help
        arguments = args.Select(a => a.Split("="))
            .ToDictionary(
                pair => pair[0].ToLowerInvariant().TrimStart('-'),
                pair => (ICollection<string>)[pair.Length > 1 ? pair[1] : "true"]
            );
    }

    public CakeConsoleArguments(
        ILookup<string, string> parsed,
        Core.Diagnostics.Verbosity verbosity
    )
    {
        arguments = parsed.ToDictionary(p => p.Key, p => (ICollection<string>)p.ToArray());
        arguments["verbosity"] = [verbosity.ToString()];
    }

    public ICollection<string> GetArguments(string name) =>
        HasArgument(name) ? arguments[name.ToLowerInvariant()] : Array.Empty<string>();

    public IDictionary<string, ICollection<string>> GetArguments() => arguments;

    public bool HasArgument(string name) => arguments.ContainsKey(name.ToLowerInvariant());
}
