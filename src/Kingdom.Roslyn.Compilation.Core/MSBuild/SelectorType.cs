namespace Kingdom.Roslyn.Compilation.MSBuild
{
    /// <summary>
    /// Provides a means of interfacing with the VisualStudioInstance in the form of Selectors.
    /// </summary>
    public enum SelectorType
    {
        /// <summary>
        /// Instance selection may occur by the VisualStudioInstance.DiscoveryType property.
        /// </summary>
        DiscoveryType,

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Instance selection may occur by the VisualStudioInstance.MSBuildPath property.
        /// </summary>
        MSBuildPath,

        /// <summary>
        /// Instance selection may occur by the VisualStudioInstance.Name property.
        /// </summary>
        Name,

        /// <summary>
        /// Instance selection may occur by the VisualStudioInstance.Version property.
        /// </summary>
        Version,

        /// <summary>
        /// Instance selection may occur by the VisualStudioInstance.VisualStudioRootPath property.
        /// </summary>
        VisualStudioRootPath
    }
}
