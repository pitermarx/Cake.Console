using Cake.Core.Packaging;

namespace Cake.Console.Internals;

internal class CakeNugetTool(string id, string version) : ICakeToolReference
{
    public PackageReference Reference { get; } =
        new(new Uri($"nuget:?package={id}&version={version}"));
}
