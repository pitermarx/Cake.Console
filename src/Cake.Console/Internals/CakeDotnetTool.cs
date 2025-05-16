using Cake.Core.Packaging;

namespace Cake.Console.Internals;

internal class CakeDotnetTool(string id, string version) : ICakeToolReference
{
    public PackageReference Reference { get; } =
        new(new Uri($"dotnet:?package={id}&version={version}"));
}
