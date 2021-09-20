using Cake.Core;
using VerifyTests;
using System;

public static class VerifyExtensions
{
    public static void Verify(this CakeTaskBuilder tb, Func<ICakeContext, string> action)
    {
        tb.Does(async c =>
        {
            var received = action(c);
            var settings = new VerifySettings();
            settings.DisableDiff();
            settings.UseFileName(tb.Task.Name);
            settings.UseDirectory("snapshots");
            settings.ScrubLinesContaining(StringComparison.OrdinalIgnoreCase, "00:00:0");
            if (c.Arguments.HasArgument("accept")) settings.AutoVerify();
            var verifier = new InnerVerifier("build/Program.cs", typeof(VerifyExtensions), settings, typeof(VerifyExtensions).GetMethod(nameof(Verify)));
            await verifier.Verify(received);
        });
    }
}
