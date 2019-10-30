using System;

namespace Kingdom.Roslyn.Compilation.MSBuild
{
    /// <summary>
    /// Provides a Standard proxy allowing for standard engagement with the MSBuild SDK Core API.
    /// </summary>
    [Flags]
    public enum DiscoveryType
    {
        /// <summary>
        /// DeveloperConsole, or 0x1, or 0001b.
        /// </summary>
        DeveloperConsole = 1 << 0,

        /// <summary>
        /// DotNetSdk, or 0x2, or 0010b.
        /// </summary>
        DotNetSdk = 1 << 1,

        /// <summary>
        /// VisualStudioSetup, or 0x4, or 0100b.
        /// </summary>
        VisualStudioSetup = 1 << 2
    }
}
