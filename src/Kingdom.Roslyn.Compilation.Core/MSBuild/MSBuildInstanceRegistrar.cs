using System.Collections.Generic;

namespace Kingdom.Roslyn.Compilation.MSBuild
{
    /// <inheritdoc />
    // ReSharper disable once InconsistentNaming
    public abstract class MSBuildInstanceRegistrar<T> : IMSBuildInstanceRegistrar<T>
        where T : class
    {
        /// <inheritdoc />
        public abstract T RegisteredInstance { get; }

        /// <inheritdoc />
        public abstract IEnumerable<T> EnumeratedInstances { get; }

        /// <summary>
        /// Gets whether IsDisposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Occurs OnDisposed for <paramref name="disposing"/>.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void OnDispose(bool disposing)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            OnDispose(true);
            IsDisposed = true;
        }
    }
}
