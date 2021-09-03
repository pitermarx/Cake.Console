using Cake.Core.Packaging;
using System;

namespace Cake.Console
{
    public class CakeNugetTool : ICakeToolReference
    {
        public PackageReference Reference { get; }

        public CakeNugetTool(string id, string version)
        {
            Reference = new PackageReference(new Uri($"nuget:?package={id}&version={version}"));
        }
    }
}