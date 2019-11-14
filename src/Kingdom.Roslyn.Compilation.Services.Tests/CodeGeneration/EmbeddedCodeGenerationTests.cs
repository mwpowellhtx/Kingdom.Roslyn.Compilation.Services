using System.Collections.Generic;
using System.Linq;

namespace Kingdom.Roslyn.Compilation.CodeGeneration
{
    using Microsoft.CodeAnalysis;
    using Xunit;
    using Xunit.Abstractions;
    using static Microsoft.CodeAnalysis.WorkspaceDiagnosticKind;

    public class EmbeddedCodeGenerationTests : MSBuildCompilationManagerTestFixtureBase
    {
        public EmbeddedCodeGenerationTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        private ICollection<ProjectContext> ProjectContexts { get; } = new List<ProjectContext>();

        private ProjectContext AppendContext(ProjectContext context)
        {
            ProjectContexts.Add(context);
            return context;
        }

        [Fact]
        public void Test_Case_Obviously_Malformed()
        {
            // TODO: TBD: we are satisfied that the reasons we were here pursuing Roslyn Compilation Services has been completed.
            // TODO: TBD: what we can stand to do more of next is to work on better arrangement, action, and assertion.
            // TODO: TBD: but for now we will commit and put it to some use, then apply whatever else we learn on the other side of that.
            var context = AppendContext(
                new ProjectContext("namespace Foo { public class Bar { public Bar() }".ToRange())
            );

            context.RefreshContents();

            var projectPath = context.ProjectPath.AssertNotNull().AssertFileExists();

            CompilationManager.Workspace.OpenProjectAsync(projectPath).Wait();

            EvaluateRoslynWorkspaceDiagnostics(context, CompilationManager.Workspace.Diagnostics.ToArray());

            CompilationManager.EvaluateCompilation += Private_OnEvaluateCompilation;

            CompilationManager.CompileAllProjects();

            CompilationManager.EvaluateCompilation -= Private_OnEvaluateCompilation;
        }

        [Fact]
        public void Test_Case_A()
        {
            var context = AppendContext(new ProjectContext(UseCases.A));

            context.RefreshContents();

            CompilationManager.Workspace.OpenProjectAsync(context.ProjectPath).Wait();

            EvaluateRoslynWorkspaceDiagnostics(context, CompilationManager.Workspace.Diagnostics.ToArray());

            CompilationManager.EvaluateCompilation += Private_OnEvaluateCompilation;

            CompilationManager.CompileAllProjects();

            CompilationManager.EvaluateCompilation -= Private_OnEvaluateCompilation;
        }

        // TODO: TBD: could expose these as core API, etc...
        private void EvaluateRoslynWorkspaceDiagnostics(ProjectContext _, IEnumerable<WorkspaceDiagnostic> diagnostics)
        {
            /* TODO: TBD: there is no clear path here yet.
             * https://gitter.im/dotnet/roslyn
             * https://github.com/microsoft/msbuild/issues/4319 / ToolsVersion should be ignored by MSBuild task
             * https://github.com/microsoft/msbuild/issues/4848 / SDK 'Microsoft.NET.Sdk' not found loading CSharp project via MSBuildWorkspace.OpenProjectAsync
             * https://github.com/dotnet/roslyn/issues/39471 / SDK 'Microsoft.NET.Sdk' not found loading CSharp project via MSBuildWorkspace.OpenProjectAsync
             * https://github.com/mwpowellhtx/Kingdom.Roslyn.Compilation.Services
             * https://github.com/mwpowellhtx/Code.Generation.Roslyn
             * https://github.com/mwpowellhtx/Kingdom.Roslyn.Compilation.Services/blob/master/src/Kingdom.Roslyn.Compilation.Services.Tests/CodeGeneration/EmbeddedCodeGenerationTests.cs#L10
             */
            var i = 0;

            // ReSharper disable PossibleMultipleEnumeration
            foreach (var x in diagnostics.AssertNotNull())
            {
                OutputHelper.WriteLine($"{++i}: {x}");
            }

            diagnostics.AssertFalse(x => x.Any(y => y.Kind == Failure));
            // ReSharper restore PossibleMultipleEnumeration
        }

        private void EvaluateRoslynDiagnostics(ProjectContext context, IEnumerable<Diagnostic> diagnostics)
        {
            var i = 0;

            foreach (var x in diagnostics)
            {
                context.DiagnosticResults.Add(DiagnosticResult.Create(x));
                OutputHelper.WriteLine($"{++i}: {context.DiagnosticResults.Last().Summary}");
            }
        }

        // TODO: TBD: we could potentially plant this in a base class...
        private void Private_OnEvaluateCompilation(object sender, CompilationDiagnosticEventArgs e)
        {
            const DiagnosticSeverity warning = DiagnosticSeverity.Warning;

            WorkspaceDiagnostic d;
            var context = ProjectContexts.LastOrDefault().AssertNotNull();

            var accepted = e.Filter.TryAcceptResult(out var result, warning, out var diagnostics);

            /* We could get fancier and condense it further, but there is some benefit
             * from a troubleshooting perspective in leaving it open such as it is. */

            EvaluateRoslynDiagnostics(context, diagnostics);

            result.AssertTrue(x => x.Success);
            accepted.AssertTrue();
        }

        // TODO: TBD: load the project and save it out...
        // TODO: TBD: obtain the use case(s) and save those out...
        // TODO: TBD: ensure dispose disposes of all
        // TODO: TBD: unless there is a problem, then maybe we postpone

        protected override void OnDispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                CompilationManager = null;

                foreach (var x in ProjectContexts)
                {
                    //x?.Dispose();
                }
            }

            base.OnDispose(disposing);
        }
    }
}
