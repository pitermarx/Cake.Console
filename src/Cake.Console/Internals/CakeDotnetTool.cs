using Cake.Core.Packaging;
using System;

namespace Cake.Console
{
    internal class CakeDotnetTool : ICakeToolReference
    {
        public PackageReference Reference { get; }

        public CakeDotnetTool(string id, string version)
        {
            Reference = new PackageReference(new Uri($"dotnet:?package={id}&version={version}"));
        }
    }
}