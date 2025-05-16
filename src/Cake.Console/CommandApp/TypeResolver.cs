using Spectre.Console.Cli;

namespace Cake.Console.CommandApp;

internal sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    public object Resolve(Type? type) =>
        (type is null ? null : provider.GetService(type))
        ?? throw new NullReferenceException($"Type {type?.Name} not found");
}
