using System.Collections.Generic;
using System.Linq;

namespace Kingdom.Roslyn.Compilation.Services.CodeGeneration
{
    using Microsoft.CodeAnalysis.MSBuild;
    using Xunit;
    using Xunit.Abstractions;

    public class EmbeddedCodeGenerationTests : CompilationCodeGenerationTestFixtureBase<MSBuildWorkspace, MSBuildCompilationManager>
    {
        protected override MSBuildCompilationManager CreateCompilationManager() => new MSBuildCompilationManager(Debug);

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
            var context = AppendContext(
                new ProjectContext(GetRange(
                    "namespace Foo { public class Bar { public Bar() }"
                ))
            );

            context.RefreshContents();
            CompilationManager.Workspace.OpenProjectAsync(context.ProjectPath).Wait();

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

            CompilationManager.EvaluateCompilation += Private_OnEvaluateCompilation;

            CompilationManager.CompileAllProjects();

            CompilationManager.EvaluateCompilation -= Private_OnEvaluateCompilation;
        }


        // TODO: TBD: we could potentially plant this in a base class...
        private void Private_OnEvaluateCompilation(object sender, CompilationDiagnosticEventArgs e)
        {
            var i = 0;

            var context = ProjectContexts.LastOrDefault().AssertNotNull();

            var accepted = e.Filter.TryAcceptResult(out var result, out var diagnostics);

            /* We could get fancier and condense it further, but there is some benefit
             * from a troubleshooting perspective in leaving it open such as it is. */

            foreach (var x in diagnostics)
            {
                context.DiagnosticResults.Add(DiagnosticResult.Create(x));
                OutputHelper.WriteLine($"{++i}: {context.DiagnosticResults.Last().Summary}");
            }

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
