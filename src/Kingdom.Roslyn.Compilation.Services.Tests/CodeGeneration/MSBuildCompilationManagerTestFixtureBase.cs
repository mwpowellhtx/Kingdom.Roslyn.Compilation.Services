using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingdom.Roslyn.Compilation.Services.CodeGeneration
{
    using Microsoft.Build.Locator;
    using Microsoft.CodeAnalysis.MSBuild;
    using Xunit.Abstractions;
    using static Microsoft.Build.Locator.DiscoveryType;
    using static String;

    /// <summary>
    /// 
    /// </summary>
    public class MSBuildCompilationManagerTestFixtureBase : CompilationCodeGenerationTestFixtureBase<MSBuildWorkspace, MSBuildCompilationManager>
    {
        private static VisualStudioInstance RegisteredInstance { get; }

        private static IEnumerable<VisualStudioInstance> EnumeratedInstances { get; }

        static MSBuildCompilationManagerTestFixtureBase()
        {
            const DiscoveryType discoveryTypes = DeveloperConsole | DotNetSdk | VisualStudioSetup;

            var options = new VisualStudioInstanceQueryOptions {DiscoveryTypes = discoveryTypes};

            EnumeratedInstances = MSBuildLocator.QueryVisualStudioInstances(options).ToArray();

            // TODO: TBD: are the defaults sufficient here?
            // TODO: TBD: is there a better way for us to register a specific runtime?
            RegisteredInstance = MSBuildLocator.RegisterDefaults();
        }

        protected override MSBuildCompilationManager CreateCompilationManager() => new MSBuildCompilationManager(Debug);

        public MSBuildCompilationManagerTestFixtureBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            foreach (var x in EnumeratedInstances)
            {
                ReportLocatedBuildInstances(x);
            }

            ReportLocatedBuildInstances(RegisteredInstance, true);
        }

        protected void ReportLocatedBuildInstances(VisualStudioInstance instance, bool registered = false)
        {
            const string curlyBraces = "{}";

            var which = registered ? "Registered" : "Available";

            string RenderInstance(params Func<VisualStudioInstance, string>[] parts)
                => Join(Join(", ", parts.Select(x => x.Invoke(instance)))
                    , $"{curlyBraces.First()} ", $" {curlyBraces.Last()}");

            var rendered = RenderInstance(
                x => $"'{nameof(x.DiscoveryType)}': '{x.DiscoveryType}'"
                , x => $"'{nameof(x.Name)}': '{x.Name}'"
                , x => $"'{nameof(x.Version)}': '{x.Version}'"
                , x => $"'{nameof(x.MSBuildPath)}': '{x.MSBuildPath}'"
                , x => $"'{nameof(x.VisualStudioRootPath)}': '{x.VisualStudioRootPath}'"
            );


            OutputHelper.WriteLine($"{which} instance: {rendered}");
        }
    }
}
