namespace Kingdom.Roslyn.Compilation.MSBuild
{
    /// <summary>
    /// Provides a set of <see cref="DiscoveryType"/> extension methods.
    /// </summary>
    internal static class DiscoverTypeExtensionMethods
    {
        /// <summary>
        /// Returns the <paramref name="value"/> as a <see cref="Microsoft.Build.Locator.DiscoveryType"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <see cref="DiscoveryType"/>
        /// <see cref="Microsoft.Build.Locator.DiscoveryType"/>
        public static Microsoft.Build.Locator.DiscoveryType ToDiscoveryType(this DiscoveryType value)
            => (Microsoft.Build.Locator.DiscoveryType) (int) value;

        /// <summary>
        /// Returns the <paramref name="value"/> in terms of a <see cref="DiscoveryType"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <see cref="Microsoft.Build.Locator.DiscoveryType"/>
        /// <see cref="DiscoveryType"/>
        public static DiscoveryType FromDiscoveryType(this Microsoft.Build.Locator.DiscoveryType value)
            => (DiscoveryType) (int) value;
    }
}
