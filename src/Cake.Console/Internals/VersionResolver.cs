using System.Diagnostics;
using Cake.Cli;

namespace Cake.Console.Internals
{
    /// <summary>
    /// The Cake version resolver.
    /// </summary>
    public sealed class CakeConsoleVersionResolver : IVersionResolver
    {
        /// <inheritdoc/>
        public string GetVersion()
        {
            var assembly = typeof(CakeHost).Assembly;
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;

            if (string.IsNullOrWhiteSpace(version))
            {
                version = "Unknown";
            }

            return version;
        }

        /// <inheritdoc/>
        public string GetProductVersion()
        {
            var assembly = typeof(CakeHost).Assembly;
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

            if (string.IsNullOrWhiteSpace(version))
            {
                version = "Unknown";
            }

            return version;
        }
    }
}