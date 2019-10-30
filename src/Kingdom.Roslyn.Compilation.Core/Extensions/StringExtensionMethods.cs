using System.Runtime.InteropServices;

namespace Kingdom.Roslyn.Compilation.MSBuild
{
    using static OSPlatform;
    using static RuntimeInformation;

    public static class StringExtensionMethods
    {
        /// <summary>
        /// Gets whether the OS Platform is <see cref="Linux"/>.
        /// </summary>
        /// <see cref="IsOSPlatform"/>
        /// <see cref="Linux"/>
        private static bool IsLinuxPlatform => IsOSPlatform(Linux);

        /// <summary>
        /// Returns <paramref name="s"/> either as-is or as lowercase depending on the nature of
        /// <see cref="IsLinuxPlatform"/>.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string AllowCaseSensitivity(this string s) => IsLinuxPlatform ? s : s.ToLower();
    }
}
