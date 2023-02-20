using Cake.Core;
using VerifyTests;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;

public static class VerifyExtensions
{
    private static readonly Type t = typeof(VerifyExtensions);
    private static readonly MethodInfo mi = t.GetMethods().First();

    public static Task Verify(this ICakeContext c, string name, Func<string> action, [CallerFilePath] string? callerFile = null)
        => c.Verify(name, s => action(), callerFile);

    public static Task Verify(this ICakeContext c, string name, Func<VerifySettings, string> action, [CallerFilePath] string? callerFile = null, [CallerMemberName] string? callerMember = null)
        => c.Verify(s =>
        {
            s.UseFileName(name);
            return action(s);
        }, callerFile, callerMember);

    private static Task Verify(this ICakeContext c, Func<VerifySettings, string> action, string? callerFile, string? callerMember)
    {
        var settings = new VerifySettings();
        settings.DisableDiff();
        settings.UseDirectory("snapshots");
        if (c.Arguments.HasArgument("accept")) settings.AutoVerify();

        var received = action(settings);

        var verifier = new InnerVerifier(callerFile!, settings, t.Name, callerMember!, Array.Empty<string>(), new PathInfo("."));
        return verifier.Verify(received);
    }
}
