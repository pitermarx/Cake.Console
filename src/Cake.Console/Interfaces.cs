using Cake.Core.Packaging;

namespace Cake.Console
{
    public interface IWorkingDirectory { string WorkingDirectory { get; } }
    public interface ICakeToolReference { PackageReference Reference { get; } }
    public interface ICakeTasks { }
}