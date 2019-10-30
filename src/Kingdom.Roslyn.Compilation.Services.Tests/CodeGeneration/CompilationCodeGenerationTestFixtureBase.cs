using System.Collections.Generic;
using System.Linq;

namespace Kingdom.Roslyn.Compilation.CodeGeneration
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Xunit;
    using Xunit.Abstractions;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using static Microsoft.CodeAnalysis.DiagnosticSeverity;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

    public abstract class CompilationCodeGenerationTestFixtureBase : TestFixtureBase
    {
        protected const string Debug = CompilationManager.Debug;

        protected const string Release = CompilationManager.Release;

        protected CompilationCodeGenerationTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
    }

    /// <summary>
    /// The <see cref="Compilation"/> is the thing for which this Manager is driving. Everything
    /// here prepares for and initiates a Compilation then furnishes the <see cref="Diagnostic"/>
    /// results following said Compilation.
    /// </summary>
    /// <typeparam name="TWorkspace"></typeparam>
    /// <typeparam name="TCompilationManager"></typeparam>
    public abstract class CompilationCodeGenerationTestFixtureBase<TWorkspace, TCompilationManager>
        : CompilationCodeGenerationTestFixtureBase
        where TWorkspace : Workspace
        where TCompilationManager : CompilationManager<TWorkspace>
    {

#if DEBUG
        /// <summary>
        /// &quot;Debug&quot;
        /// </summary>
        protected const string Configuration = "Debug";
#else
        /// <summary>
        /// &quot;Release&quot;
        /// </summary>
        protected const string Configuration = "Release";
#endif

        private TCompilationManager _compilationManager;

        /// <summary>
        /// Creates a new CompilationManager instance.
        /// </summary>
        /// <returns></returns>
        protected abstract TCompilationManager CreateCompilationManager();

        /// <summary>
        /// Gets the CompilationManager instance.
        /// </summary>
        protected TCompilationManager CompilationManager
        {
            get
            {
                TCompilationManager InitializerCompilationManager(TCompilationManager compilationManager)
                {
                    compilationManager.ResolveMetadataReferences += CompilationManager_OnResolveMetadataReferences;
                    compilationManager.EvaluateCompilation += CompilationManager_OnEvaluateCompilation;
                    return compilationManager;
                }

                return _compilationManager ?? (_compilationManager
                           = InitializerCompilationManager(CreateCompilationManager())
                       );
            }
            set
            {
                if (value == null)
                {
                    _compilationManager?.Dispose();
                }

                _compilationManager = value;
            }
        }

        protected virtual void ReportDiagnostic(Diagnostic diagnostic)
        {
            // TODO: TBD: we need to know more than just Diagnostic in order to connect the dots with the ErrorMessage?
            var result = DiagnosticResult.Create(diagnostic);

            //// TODO: TBD: what sort of 'Location' are we talking about here?
            //result.Locations = new [] {diagnostic.Location}.Concat(diagnostic.AdditionalLocations.ToArray()).ToArray();

            OutputHelper.WriteLine(result.Summary);
        }

        /// <summary>
        /// Override to handle more than the basic <see cref="Compilation.GetDiagnostics"/>
        /// validation. <see cref="Diagnostic"/> validation is the first thing we must rule out.
        /// Because if there is any problem actually compiling, then there is not much point
        /// performing any further validation or analysis.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void CompilationManager_OnEvaluateCompilation(object sender, CompilationDiagnosticEventArgs e)
        {
            // TODO: TBD: which we might capture in some sort of options...
            const DiagnosticSeverity minimumSeverity = Error;

            bool FilterDiagnosticSeverity(Diagnostic diagnostic) => diagnostic.Severity >= minimumSeverity;

            var diagnostics = e.Filter.ToArray();

            diagnostics.Where(FilterDiagnosticSeverity).ToList().ForEach(ReportDiagnostic);

            // TODO: TBD: could filter more based on Severity ...
            diagnostics.AssertFalse(x => x.Any(y => y.Severity == Error));
        }

        // TODO: TBD: may do this on a more case-by-case, Fact-by-Fact, or Theory, basis...
        // TODO: TBD: or even establish a handful of different fit for purpose test fixtures...
        protected virtual void CompilationManager_OnResolveMetadataReferences(object sender, ResolveMetadataReferencesEventArgs e)
        {
            //var trusted = ((string) AppContext.GetData($"TRUSTED_PLATFORM_ASSEMBLIES")).Split(';');

            // TODO: TBD: refactored these references from the compilation manager as a next step...
            // TODO: TBD: I expect that we may refactor these even further from here, but this seems like the logical next step...
            var references = GetRange(
                "mscorlib.dll"
                , "netstandard.dll"
                , "System.dll"
                , "System.Core.dll"
                , "System.Runtime.dll"
            ).ToArray();

            // TODO: TBD: if NETCOREAPP? "System.Private.CoreLib.dll"
            // TODO: TBD: this needs to be loaded regardless whether this is a NETCOREAPP ...
            e.AddReferenceToTypeAssembly<object>();
            e.AddTypeAssemblyLocationBasedReferences<object>(references);
            e.AddReferenceToTypeAssembly<CSharpCompilation>();
        }

        protected CompilationCodeGenerationTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        public delegate CompilationUnitSyntax InitializeCompilationUnitCallback(CompilationUnitSyntax compilationUnit);

        internal static IEnumerable<CompilationUnitSyntax> GetCompilationUnits(params InitializeCompilationUnitCallback[] callbacks)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var x in callbacks)
            {
                yield return x.Invoke(CompilationUnit());
            }
        }

        protected virtual void VerifyCompilation(string projectName, params CompilationUnitSyntax[] compilationUnits)
        {
            OutputHelper.WriteLine($"Project Name: {projectName}");

            var renderedUnits = compilationUnits.Select(x => x.NormalizeWhitespace().ToFullString()).ToArray();

            foreach (var x in renderedUnits)
            {
                OutputHelper.WriteLine($"Rendered Unit: {x}");
            }

            CompilationManager.Compile(projectName, renderedUnits);
        }

        protected static class UseCases
        {
            internal static SyntaxToken PartialSyntaxToken => Token(PartialKeyword);

            internal static SyntaxToken PublicSyntaxToken => Token(PublicKeyword);


            // TODO: TBD: for lack of a better name at the moment...
            internal static IEnumerable<CompilationUnitSyntax> A => GetCompilationUnits(
                x => x.AddMembers(
                    NamespaceDeclaration(IdentifierName("Foo"))
                        .WithUsings(SingletonList(
                            UsingDirective(IdentifierName("Bar"))
                        ))
                        .AddMembers(
                            ClassDeclaration("Fiz")
                                .AddModifiers(PublicSyntaxToken, PartialSyntaxToken)
                        )
                )
                , x => x.AddMembers(
                    NamespaceDeclaration(IdentifierName("Bar"))
                )
            );
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                if (CompilationManager != null)
                {
                    CompilationManager.ResolveMetadataReferences -= CompilationManager_OnResolveMetadataReferences;
                    CompilationManager.EvaluateCompilation -= CompilationManager_OnEvaluateCompilation;
                }

                CompilationManager?.Dispose();
            }

            base.OnDispose(disposing);
        }
    }
}
