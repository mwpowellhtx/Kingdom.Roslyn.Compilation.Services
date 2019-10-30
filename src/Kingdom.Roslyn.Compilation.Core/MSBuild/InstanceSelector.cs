using System;

namespace Kingdom.Roslyn.Compilation.MSBuild
{
    /// <summary>
    /// Provides the ability to Select a VisualStudioInstance based on the <see cref="Selector"/>.
    /// </summary>
    public class InstanceSelector
    {
        /// <summary>
        /// Gets or Sets the Selector which drives subsequent <see cref="Predicate"/> invocations.
        /// </summary>
        public SelectorType Selector { get; set; }

        /// <summary>
        /// Gets or Sets the Predicate. The <see cref="object"/> may be a <see cref="Version"/>,
        /// a <see cref="DiscoveryType"/>, or a <see cref="string"/>. When it is a
        /// <see cref="string"/>, will be lowercase when the OS is not Linux, that is,
        /// is not case sensitive.
        /// </summary>
        public ImageSelectorPredicate Predicate { get; set; }
    }
}
