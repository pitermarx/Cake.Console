using Cake.Core.Packaging;
using System;

namespace Cake.Console
{
    internal interface IHostBuilderBehaviour
    {
        void Run(IServiceProvider provider);
    }

    public interface IWorkingDirectory { string WorkingDirectory { get; } }
    public interface ICakeToolReference { PackageReference Reference { get; } }
    public interface ICakeTasks { }
}