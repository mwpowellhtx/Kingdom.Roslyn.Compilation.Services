using System;
using System.Linq;

namespace Kingdom.Roslyn.Compilation.CodeGeneration
{
    using Microsoft.Build.Locator;
    using Microsoft.CodeAnalysis.MSBuild;
    using MSBuild;
    using Xunit;
    using Xunit.Abstractions;
    using static String;

    // ReSharper disable once InconsistentNaming
    /// <inheritdoc cref="CompilationCodeGenerationTestFixtureBase{TWorkspace,TCompilationManager}" />
    public abstract class MSBuildCompilationManagerTestFixtureBase
        : CompilationCodeGenerationTestFixtureBase<MSBuildWorkspace, MSBuildCompilationManager>
    {
        protected override MSBuildCompilationManager CreateCompilationManager() => new MSBuildCompilationManager(Debug);

        /// <summary>
        /// Gets the Registrar.
        /// </summary>
        private static IVisualStudioInstanceRegistrar Registrar { get; }

        static MSBuildCompilationManagerTestFixtureBase()
        {
            // TODO: TBD: not sure there is a great way to watch for this state?
            // TODO: TBD: in particular what to do when disposing, when to legitimately dispose of the Registrar...
            Registrar = new VisualStudioInstanceRegistrar();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        protected MSBuildCompilationManagerTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var registrar = Registrar.AssertNotNull();
            ReportLocatedBuildInstances(registrar.RegisteredInstance.AssertNotNull(), true);

            foreach (var x in registrar.EnumeratedInstances.AssertNotNull().AssertNotEmpty())
            {
                ReportLocatedBuildInstances(x.AssertNotNull());
            }
        }

        /// <summary>
        /// Reports the <paramref name="instance"/> and whether <paramref name="registered"/>.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="registered"></param>
        protected void ReportLocatedBuildInstances(VisualStudioInstance instance, bool registered = false)
        {
            const string curlyBraces = "{}";

            var which = registered ? "Registered" : "Available";

            string RenderInstance(params Func<VisualStudioInstance, string>[] parts)
                => parts.Any()
                    ? Join(Join(", ", parts.Select(x => x.Invoke(instance)))
                        , $"{curlyBraces.First()} ", $" {curlyBraces.Last()}")
                    : Join(" ", $"{curlyBraces.First()}", $"{curlyBraces.Last()}");

            var rendered = RenderInstance(
                x => $"'{nameof(x.DiscoveryType)}': '{x.DiscoveryType}'"
                , x => $"'{nameof(x.Name)}': '{x.Name}'"
                , x => $"'{nameof(x.Version)}': '{x.Version}'"
                , x => $"'{nameof(x.MSBuildPath)}': '{x.MSBuildPath}'"
                , x => $"'{nameof(x.VisualStudioRootPath)}': '{x.VisualStudioRootPath}'"
            );

            OutputHelper.WriteLine($"{which} instance: {rendered}");
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                //Registrar?.Dispose();
            }

            base.OnDispose(disposing);
        }
    }
}
