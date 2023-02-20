using System;

namespace Cake.Console.CommandApp
{
    internal sealed class TypeResolver : Spectre.Console.Cli.ITypeResolver
    {
        private readonly IServiceProvider provider;

        public TypeResolver(IServiceProvider provider)
            => this.provider = provider;

        public object Resolve(Type? type)
            => (type is null ? null : provider.GetService(type)) ?? throw new NullReferenceException($"Type {type?.Name} not found");
    }
}