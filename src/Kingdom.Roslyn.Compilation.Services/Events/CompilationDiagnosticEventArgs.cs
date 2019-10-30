using System;

namespace Kingdom.Roslyn.Compilation
{
    /// <summary>
    /// Signals when Compilation Diagnostics may be further Evaluated.
    /// </summary>
    /// <inheritdoc />
    public sealed class CompilationDiagnosticEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the Filter.
        /// </summary>
        public ICompilationDiagnosticFilter Filter { get; }

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="filter"></param>
        /// <inheritdoc />
        internal CompilationDiagnosticEventArgs(ICompilationDiagnosticFilter filter)
        {
            Filter = filter;
        }
    }
}
