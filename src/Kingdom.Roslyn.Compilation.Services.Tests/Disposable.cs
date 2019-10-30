using System;

namespace Kingdom.Roslyn.Compilation
{
    /// <summary>
    /// Disposable base class.
    /// </summary>
    public abstract class Disposable : IDisposable
    {
        protected static object DummyObject => null;

        /// <summary>
        /// Gets whether IsDisposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <summary>
        /// Occurs On <see cref="IDisposable.Dispose"/>.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void OnDispose(bool disposing)
        {
        }

        /// <inheritdoc />
        /// <see cref="OnDispose"/>
        /// <see cref="IsDisposed"/>
        public void Dispose()
        {
            OnDispose(true);
            IsDisposed = true;
        }
    }
}
