using System;

namespace Cake.Console.CommandApp
{
    internal sealed class TypeResolver : Spectre.Console.Cli.ITypeResolver
    {
        private readonly IServiceProvider provider;

        public TypeResolver(IServiceProvider provider)
            => this.provider = provider;

        public object Resolve(Type type)
            => provider.GetService(type) ?? new NullReferenceException($"Type {type.Name} not found");
    }
}