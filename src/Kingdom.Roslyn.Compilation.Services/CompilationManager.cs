using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kingdom.Roslyn.Compilation.Services
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using static Microsoft.CodeAnalysis.LanguageNames;
    using static Microsoft.CodeAnalysis.OutputKind;
    using static Microsoft.CodeAnalysis.CSharp.LanguageVersion;

    public abstract class CompilationManager : IDisposable
    {
        /// <summary>
        /// Returns a New <see cref="Guid"/> basis for the Asset Name. In this case,
        /// &quot;Asset&quot; may be a <see cref="Project"/> name, <see cref="Document"/> name,
        /// even <see cref="Solution"/> name, etc.
        /// </summary>
        /// <returns></returns>
        protected static string GetNewAssetName() => $"{Guid.NewGuid():D}";

        /// <summary>
        /// Gets the <see cref="Compilation"/> Language. Default is <see cref="CSharp"/>.
        /// </summary>
        /// <see cref="CSharp"/>
        protected virtual string Language => CSharp;

        // TODO: TBD: may allow VB? Personally, I do not care about VB, but someone might ...
        /// <summary>
        /// Gets the <see cref="Document"/> File Name Extension,
        /// which should correspond with the specified <see cref="Language"/>.
        /// </summary>
        protected virtual string LanguageDocumentExtension => ".cs";

        /// <summary>
        /// Gets the <see cref="Microsoft.CodeAnalysis.CompilationOptions.OutputKind"/>.
        /// Defaults to <see cref="DynamicallyLinkedLibrary"/>.
        /// </summary>
        protected virtual OutputKind CompilationOptionsOutputKind => DynamicallyLinkedLibrary;

        // TODO: TBD: may allow for language-specific derivatives... i.e. CSharpCompilationOptions.
        /// <summary>
        /// Gets the CompilationOptions.
        /// </summary>
        protected virtual CompilationOptions CompilationOptions => new CSharpCompilationOptions(CompilationOptionsOutputKind);

        /// <summary>
        /// Gets the SpecificLanguageVersion. Defaults to <see cref="Default"/>.
        /// </summary>
        protected virtual LanguageVersion SpecificLanguageVersion => Default;

        // TODO: TBD: ditto CompilationOptions re: derivatives... i.e. CSharpParseOptions.
        /// <summary>
        /// Gets the ParseOptions.
        /// </summary>
        protected virtual ParseOptions ParseOptions => new CSharpParseOptions(SpecificLanguageVersion)
            .MergeAssets(PreprocessorSymbols.ToArray(), (o, x) => o.WithPreprocessorSymbols(x)
                , x => x.Any());

        // TODO: TBD: work of origin using this: "SOMETHING_ACTIVE";
        /// <summary>
        /// Gets any PreprocessorSymbols involved during the Compilation.
        /// </summary>
        protected virtual IEnumerable<string> PreprocessorSymbols
        {
            get { yield break; }
        }

        /// <summary>
        /// EvaluateCompilation event.
        /// </summary>
        public virtual event EventHandler<CompilationDiagnosticEventArgs> EvaluateCompilation;

        // TODO: TBD: may furnish Generic type derived from Compilation...
        // TODO: TBD: i.e. CSharpCompilation, but this must also align with the Language, etc...
        /// <summary>
        /// Event handler occurs when <see cref="EvaluateCompilation"/> is requested.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="diagnosticFilter"></param>
        protected virtual void OnEvaluateCompilation(Project project, ICompilationDiagnosticFilter diagnosticFilter)
        {
            var e = new CompilationDiagnosticEventArgs(project, diagnosticFilter);
            EvaluateCompilation?.Invoke(this, e);
        }

        protected virtual ICompilationDiagnosticFilter CreateDiagnosticFilter(Compilation compilation)
            => new BasicCompilationDiagnosticFilter(compilation);

        // TODO: TBD: may need to reconsider Task<Compilation> in cases where Analyzers are involved, i.e. Task<CompilationWithAnalyzers> (?)
        /// <summary>
        /// Resolves the <see cref="Compilation"/> given <paramref name="project"/>, the ensuing
        /// <paramref name="compiling"/>, as well as constituent elements that informed the
        /// request.
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="sources"></param>
        /// <param name="project"></param>
        /// <param name="compiling"></param>
        /// <param name="cancellationToken"></param>
        [Obsolete("is the projectName/sources version really that necessary after all?")]
        protected virtual void ResolveCompilation(string projectName, IReadOnlyList<string> sources, Project project
            , Task<Compilation> compiling, CancellationToken cancellationToken = default)
        {
            OnEvaluateCompilation(project, CreateDiagnosticFilter(compiling.Result));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="compiling"></param>
        /// <param name="cancellationToken"></param>
        protected virtual void ResolveCompilation(Project project, Task<Compilation> compiling, CancellationToken cancellationToken = default)
            => OnEvaluateCompilation(project, CreateDiagnosticFilter(compiling.Result));

        /// <summary>
        /// Disposes the Object.
        /// </summary>
        /// <param name="disposing"></param>
        /// <see cref="Workspace"/>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Gets whether the Fixture IsDisposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            IsDisposed = true;
        }
    }
}
