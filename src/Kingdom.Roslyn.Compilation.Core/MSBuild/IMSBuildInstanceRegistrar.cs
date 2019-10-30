using System;
using System.Collections.Generic;

namespace Kingdom.Roslyn.Compilation.MSBuild
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Represents a simple MSBuild Instance Registrar. There is literally no implementation we
    /// can sensible expose here. We leave it to the concrete implementation in order to furnish
    /// whatever it needs via constructors, their arguments, etc. Whether you need a Singleton
    /// instance, or a maintain an instance in a contextual life time, this provides flexibility
    /// how to engage with the MSBuild Instance Locator.
    /// </summary>
    /// <inheritdoc />
    public interface IMSBuildInstanceRegistrar : IDisposable
    {
    }

    /// <inheritdoc />
    /// <typeparam name="T"></typeparam>
    public interface IMSBuildInstanceRegistrar<out T> : IMSBuildInstanceRegistrar
        where T : class
    {
        /// <summary>
        /// Gets the RegisteredInstance.
        /// </summary>
        T RegisteredInstance { get; }

        /// <summary>
        /// Gets the EnumeratedInstances.
        /// </summary>
        IEnumerable<T> EnumeratedInstances { get; }
    }
}
