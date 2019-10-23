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
        /// &quot;Debug&quot;
        /// </summary>
        public const string Debug = nameof(Debug);

        /// <summary>
        /// &quot;Release&quot;
        /// </summary>
        public const string Release = nameof(Release);

        /// <summary>
        /// Gets or Sets the Build Configuration. The Default is <see cref="Release"/>.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="configuration">A caller provided value. The default is <see cref="Release"/>.</param>
        protected CompilationManager(string configuration = Release)
        {
            Configuration = configuration;
        }

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

        /// <summary>
        /// <see cref="PreprocessorSymbols"/> backing field.
        /// </summary>
        private readonly IList<string> _preprocessorSymbols = new List<string>();

        /// <summary>
        /// Prepares the <paramref name="symbols"/> for use during the Build cycle.
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
        /// <see cref="Configuration"/>
        /// <see cref="Debug"/>
        protected virtual IEnumerable<string> PreparePreprocessorSymbols(IList<string> symbols)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (Configuration)
            {
                case Debug:
                    symbols.Add(Debug.ToUpper());
                    break;
            }

            return symbols;
        }

        /// <summary>
        /// Gets the PreprocessorSymbols for use during the Build cycle.
        /// </summary>
        protected IEnumerable<string> PreprocessorSymbols => PreparePreprocessorSymbols(_preprocessorSymbols);

        /// <summary>
        /// Occurs when it is time to Resolve the <see cref="Project.MetadataReferences"/>
        /// given the <see cref="Solution"/>.
        /// </summary>
        public virtual event EventHandler<ResolveMetadataReferencesEventArgs> ResolveMetadataReferences;

        /// <summary>
        /// Resolve the <see cref="Project.MetadataReferences"/> as furnished by
        /// <see cref="ResolveMetadataReferencesEventArgs.MetadataReferences"/>.
        /// </summary>
        /// <param name="solution">The <see cref="Solution"/> upon which the resolution is based.</param>
        /// <param name="project">The <see cref="Project"/> for which the References may occur.</param>
        /// <returns>A potentially modified <see cref="Solution"/> instance based upon <paramref name="solution"/>.</returns>
        protected virtual Solution OnResolveMetadataReferences(Solution solution, Project project)
        {
            var e = new ResolveMetadataReferencesEventArgs {Solution = solution, Project = project};
            ResolveMetadataReferences?.Invoke(this, e);
            // TODO: TBD: may report those references unable to add...
            return solution.MergeAssets(e.MetadataReferences.ToArray()
                , (g, x) => g.AddMetadataReferences(e.Project.Id, x), x => x.Any());
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
            var e = new CompilationDiagnosticEventArgs(diagnosticFilter);
            EvaluateCompilation?.Invoke(this, e);
        }

        protected virtual ICompilationDiagnosticFilter CreateDiagnosticFilter(Project project, Compilation compilation)
            => new CompilationDiagnosticFilter(project, Configuration, compilation);

        // TODO: TBD: may need to reconsider Task<Compilation> in cases where Analyzers are involved, i.e. Task<CompilationWithAnalyzers> (?)
        /// <summary>
        /// Resolves the <see cref="Compilation"/> given <paramref name="project"/>, the ensuing
        /// <paramref name="compiling"/>, as well as constituent elements that informed the
        /// request.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="compiling"></param>
        /// <param name="cancellationToken"></param>
        protected virtual void ResolveCompilation(Project project, Task<Compilation> compiling, CancellationToken cancellationToken = default)
        {
            OnEvaluateCompilation(project, CreateDiagnosticFilter(project, compiling.Result));
        }

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
