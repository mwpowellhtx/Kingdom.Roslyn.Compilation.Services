using System;
using System.Collections.Generic;

namespace Kingdom.Roslyn.Compilation.Services
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Emit;
    using DiagnosticPredicate = Predicate<Microsoft.CodeAnalysis.Diagnostic>;

    /// <summary>
    /// Represents a <see cref="Diagnostic"/> oriented Compilation Filter. The only thing
    /// we provide here is the set of Diagnostics. It is up to you, the implementer, to fill
    /// in the blanks. We intentionally do not provide any means of identifying the actual
    /// Compilation at this level.
    /// </summary>
    /// <inheritdoc />
    public interface ICompilationDiagnosticFilter : IEnumerable<Diagnostic>
    {
        /// <summary>
        /// Gets the <see cref="object"/> Compilation. We must get the <see cref="object"/>
        /// because we cannot know whether this was a
        /// <see cref="Microsoft.CodeAnalysis.Compilation"/> or a
        /// <see cref="CompilationWithAnalyzers"/>.
        /// </summary>
        object Compilation { get; }

        /// <summary>
        /// Returns the <see cref="Compilation"/> in terms of the strongly typed
        /// <typeparamref name="T"/>. We cannot know precisely what this Type ought to be at
        /// this level. That is for the caller to make the correct determination. That said,
        /// expect that the Type could be a <see cref="Microsoft.CodeAnalysis.Compilation"/>
        /// or a <see cref="CompilationWithAnalyzers"/>, for instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetCompilation<T>() where T : class;

        /// <summary>
        /// Gets or Sets the Result.
        /// </summary>
        EmitResult Result { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="Predicate{T}"/> used to Filter the <see cref="Diagnostic"/> set.
        /// </summary>
        /// <see cref="DiagnosticPredicate"/>
        DiagnosticPredicate Predicate { get; set; }
    }
}
