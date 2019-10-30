//using System.Linq;

//// TODO: TBD: this was an interesting experiment to get a flavor for how we might work with the MSBuildLocator ...
//namespace Kingdom.Roslyn.Compilation
//{
//    using Microsoft.Build.Locator;
//    using Xunit;
//    using Xunit.Abstractions;
//    using static Microsoft.Build.Locator.DiscoveryType;

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <see cref="!:https://github.com/Microsoft/MSBuildLocator"/>
//    /// <see cref="!:https://docs.microsoft.com/en-us/visualstudio/msbuild/updating-an-existing-application#use-microsoftbuildlocator"/>
//    public class MicrosoftBuildTests : TestFixtureBase
//    {
//        public MicrosoftBuildTests(ITestOutputHelper outputHelper)
//            : base(outputHelper)
//        {
//        }

//        private const DiscoveryType DiscoveryTypes = DeveloperConsole | DotNetSdk | VisualStudioSetup;

//        private static VisualStudioInstanceQueryOptions Options => new VisualStudioInstanceQueryOptions {DiscoveryTypes = DiscoveryTypes};

//        [Fact]
//        public void Verify()
//        {
//            var instances = MSBuildLocator.QueryVisualStudioInstances(Options).ToArray();

//            foreach (var x in instances)
//            {
//                var report = GetRange(
//                    $"{nameof(x.DiscoveryType)}: {x.DiscoveryType}"
//                    , $"{nameof(x.MSBuildPath)}: {x.MSBuildPath}"
//                    , $"{nameof(x.Name)}: {x.Name}"
//                    , $"{nameof(x.Version)}: {x.Version}"
//                    , $"{nameof(x.VisualStudioRootPath)}: {x.VisualStudioRootPath}"
//                );

//                report.ToList().ForEach(OutputHelper.WriteLine);
//            }

//            MSBuildLocator.RegisterInstance(instances.ElementAt(0));
//        }

//        protected override void OnDispose(bool disposing)
//        {
//            if (disposing && !IsDisposed)
//            {
//                MSBuildLocator.Unregister();
//            }

//            base.OnDispose(disposing);
//        }
//    }
//}
