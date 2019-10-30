using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Kingdom.Roslyn.Compilation
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Emit;
    using static Microsoft.CodeAnalysis.OutputKind;
    using static CompilationDiagnosticFilter.OutputKindFileExtensions;
    using DiagnosticPredicate = Predicate<Microsoft.CodeAnalysis.Diagnostic>;

    // TODO: TBD: we are pretty confident this focuses solely on the Diagnostic issue...
    // TODO: TBD: how ever we arrived at the compilation itself, when we want diagnostics to occur, we can filter them...
    /// <summary>
    /// Compilation <see cref="Diagnostic"/> Filter screens the Compilation Diagnostic results.
    /// Unfortunately, we cannot identify type as a true <see cref="Compilation"/>, on account
    /// of API such as <see cref="DiagnosticAnalyzerExtensions"/> yields an entirely different
    /// kind of class. Literally, <see cref="CompilationWithAnalyzers"/> is a wrapper only, not
    /// a derived class. However, both yield a set of <see cref="Diagnostic"/> instances, via
    /// different API. So the best we can do at this level is to Adapt a Filter around that
    /// concern.
    /// </summary>
    /// <inheritdoc />
    public class CompilationDiagnosticFilter : ICompilationDiagnosticFilter
    {
        /// <inheritdoc />
        public Project Project { get; }

        /// <inheritdoc />
        public string Configuration { get; }

        /// <inheritdoc />
        public object Compilation { get; }

        /// <inheritdoc />
        public T GetCompilation<T>() where T : class => (T) Compilation;

        /// <summary>
        /// Returns the Strongly Typed <typeparamref name="T"/> Detail associated with the
        /// <see cref="Compilation"/>. Compilation may be either a
        /// <see cref="CompilationWithAnalyzers"/>, which has as one of its properties the
        /// actual Compilation we want, or may be the actual
        /// <see cref="Microsoft.CodeAnalysis.Compilation"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getter"></param>
        /// <returns></returns>
        protected T GetCompilationDetail<T>(Func<Compilation, T> getter)
            => getter.Invoke((Compilation as CompilationWithAnalyzers)
                             ?.Compilation ?? Compilation as Compilation);

        /// <inheritdoc />
        public Compilation ActualCompilation => GetCompilationDetail(x => x);

        /// <summary>
        /// Gets the associated CancellationToken.
        /// </summary>
        protected CancellationToken CancellationToken { get; }

        ///// <summary>
        ///// Override in order to yield the set of <see cref="Diagnostic"/> instances.
        ///// We do this as a <see cref="Task{TResult}"/> because this is how some of the
        ///// <see cref="Compilation"/> API work.
        ///// </summary>
        //protected abstract Task<ImmutableArray<Diagnostic>> DiagnosticsAsync { get; }

        /// <summary>
        /// Gets the Default <see cref="Predicate"/>.
        /// </summary>
        public static DiagnosticPredicate DefaultPredicate => _ => true;

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="configuration"></param>
        /// <param name="compilation"></param>
        public CompilationDiagnosticFilter(Project project, string configuration, object compilation)
            : this(project, configuration, compilation, default)
        {
        }

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="configuration"></param>
        /// <param name="compilation"></param>
        /// <param name="cancellationToken"></param>
        public CompilationDiagnosticFilter(Project project, string configuration, object compilation, CancellationToken cancellationToken)
        {
            Project = project;
            Configuration = configuration;
            Compilation = compilation;
            CancellationToken = cancellationToken;
            Predicate = DefaultPredicate;
        }

        // ReSharper disable InconsistentNaming
        internal static class OutputKindFileExtensions
        {
            internal const string bin = nameof(bin);

            private const string dot = ".";

            internal const string pdb = dot + nameof(pdb);

            internal const string exe = dot + nameof(exe);

            internal const string netmodule = dot + nameof(netmodule);

            internal const string winmdobj = dot + nameof(winmdobj);

            internal const string dll = dot + nameof(dll);
        }
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Specifies a mapping to Known Extensions. We intentionally do not specify
        /// DLL. In other words, if look up fails, then assume DLL as the default.
        /// </summary>
        private static IDictionary<OutputKind, string> OutputExtensions => new Dictionary<OutputKind, string>
        {
            {ConsoleApplication, exe},
            {WindowsApplication, exe},
            {WindowsRuntimeApplication, exe},
            {NetModule, netmodule},
            {WindowsRuntimeMetadata, winmdobj},
        };

        protected string OutputDirectory
        {
            get
            {
                var outputDirectory = Path.Combine($"{Project.Name}", bin, Configuration);
                Directory.CreateDirectory(outputDirectory);
                return outputDirectory;
            }
        }

        protected string ArtifactFileExtension
            => OutputExtensions.TryGetValue(GetCompilationDetail(x => x.Options.OutputKind), out var ext)
                ? ext
                : $"{dll}";

        protected string ArtifactOutputPath => Path.Combine(OutputDirectory, $"{Project.Name}{ArtifactFileExtension}");

        protected string PdbOutputPath => Path.Combine(OutputDirectory, $"{Project.Name}{pdb}");

        /// <inheritdoc />
        public bool TryAcceptResult(out EmitResult result, DiagnosticSeverity maximumAcceptableSeverity, out IEnumerable<Diagnostic> diagnostics)
        {
            result = null;

            diagnostics = Diagnostics.ToArray();

            var unacceptable = diagnostics.Where(x => x.Severity > maximumAcceptableSeverity).ToArray();

            if (unacceptable.Any())
            {
                return false;
            }

            var outputPath = ArtifactOutputPath;
            var pdbOutputPath = PdbOutputPath;

            result = GetCompilationDetail(x => x.Emit(outputPath, pdbOutputPath));

            return result?.Success == true;
        }

        /// <summary>
        /// Gets or Sets the <see cref="Predicate{T}"/>. Default yields <value>true</value>, we
        /// allow All <see cref="Diagnostic"/> instances to pass through. In some instances, you
        /// may want  to furnish your own Predicate, to filter, let us consider, errors only, for
        /// instance.
        /// </summary>
        /// <see cref="DiagnosticPredicate"/>
        /// <inheritdoc />
        public DiagnosticPredicate Predicate { get; set; }

        /// <summary>
        /// Gets the Diagnostics associated with the <see cref="Compilation"/>.
        /// </summary>
        protected virtual IEnumerable<Diagnostic> Diagnostics => GetCompilationDetail(
            x => x.GetDiagnostics(CancellationToken)
        );

        /// <inheritdoc />
        public virtual IEnumerator<Diagnostic> GetEnumerator() => Diagnostics.Where(Predicate.Invoke).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
