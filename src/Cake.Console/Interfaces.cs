using Cake.Core.Composition;
using Cake.Core.Packaging;
using System;

namespace Cake.Console
{
    public interface IWorkingDirectory { string WorkingDirectory { get; } }
    public interface ICakeToolReference { PackageReference Reference { get; } }
    public interface ICakeTasks { }
}