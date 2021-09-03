using Cake.Core.Packaging;
using System;

namespace Cake.Console
{
    internal class CakeNugetTool : ICakeToolReference
    {
        public PackageReference Reference { get; }

        public CakeNugetTool(string id, string version)
        {
            Reference = new PackageReference(new Uri($"nuget:?package={id}&version={version}"));
        }
    }
}