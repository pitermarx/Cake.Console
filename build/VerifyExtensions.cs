using System.Runtime.CompilerServices;
using Cake.Core;

namespace build;

public static class VerifyExtensions
{
    private static readonly Type T = typeof(VerifyExtensions);

    public static Task<VerifyResult> Verify(
        this ICakeContext c,
        string name,
        Func<VerifySettings, string> action,
        [CallerFilePath] string? callerFile = null,
        [CallerMemberName] string? callerMember = null
    ) =>
        c.Verify(
            s =>
            {
                s.UseFileName(name);
                return action(s);
            },
            callerFile,
            callerMember
        );

    private static Task<VerifyResult> Verify(
        this ICakeContext c,
        Func<VerifySettings, string> action,
        string? callerFile,
        string? callerMember
    )
    {
        var settings = new VerifySettings();
        settings.DisableDiff();
        settings.UseDirectory("snapshots");
        if (c.Arguments.HasArgument("accept"))
            settings.AutoVerify();

        var received = action(settings);

        var verifier = new InnerVerifier(
            callerFile!,
            settings,
            T.Name,
            callerMember!,
            Array.Empty<string>(),
            new PathInfo(".")
        );
        return verifier.Verify(received);
    }
}
