using System;
using System.Collections.Generic;
using System.IO;

namespace Kingdom.Roslyn.Compilation.Services
{
    using Microsoft.CodeAnalysis;
    using static Path;
    using static MSBuildEmittingCompilationManager.KnownExtensionsAndSomeOtherConstants;
    using static String;
    using static Microsoft.CodeAnalysis.OutputKind;
    using IWorkspacePropertiesDictionary = IDictionary<string, string>;

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Does all that the <see cref="MSBuildCompilationManager"/> does, plus Emits Debugging
    /// Symbols as well as the Output Binary itself upon successful compilation.
    /// </summary>
    /// <inheritdoc />
    public class MSBuildEmittingCompilationManager : MSBuildCompilationManager
    {
        /// <summary>
        /// Default Public Constructor.
        /// </summary>
        public MSBuildEmittingCompilationManager()
        {
        }

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="workspaceProperties"></param>
        public MSBuildEmittingCompilationManager(IWorkspacePropertiesDictionary workspaceProperties)
            : base(workspaceProperties)
        {
        }

        //// TODO: TBD: options to save intermediate obj/ files and result bin/ files?
        //// TODO: TBD: ideally wanting to load the resulting Assembly itself for inspection...
        //// TODO: TBD: however, not like this, but we do need to ensure that a Project is `trained´ with the Options prior to requesting Compilation...
        //// TODO: TBD: which, to be honest, I'm not sure why that would not be the case to begin with...
        //protected override CompilationOptions CompilationOptions => base.CompilationOptions;

        //protected override ParseOptions ParseOptions => base.ParseOptions;

        // ReSharper disable InconsistentNaming, IdentifierTypo
        internal static class KnownExtensionsAndSomeOtherConstants
        {
            internal const string bin = "bin";

            internal const char dot = '.';

            internal const string pdb = nameof(pdb);

            internal const string exe = nameof(exe);

            internal const string netmodule = nameof(netmodule);

            internal const string winmdobj = nameof(winmdobj);

            internal const string dll = nameof(dll);
        }
        // ReSharper restore InconsistentNaming, IdentifierTypo

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

        protected override void OnEvaluateCompilation(Project project, ICompilationDiagnosticFilter diagnosticFilter)
        {
            string GetProjectOutputFilePath() => IsNullOrEmpty(project.OutputFilePath) ? null : project.OutputFilePath;

            // TODO: TBD: I am kind of surprised that the Roslyn API does not do more to help with this issue?
            string GetOutputExtension(OutputKind kind) => OutputExtensions.TryGetValue(kind, out var ext) ? ext : $"{dot}{dll}";

            // TODO: TBD: ditto Compilation... CompilationWithAnalyzers...
            var compilation = diagnosticFilter.GetCompilation<Compilation>();
            // TODO: TBD: possibly involving Compilation.Emit? pdb path? xml path? comprehension of `target framework´?

            string GetDesiredProjectOutputPath()
            {
                var outputDirectory = Combine($"{project.AssemblyName}", bin);

                Directory.CreateDirectory(outputDirectory);

                var outputFileName = $"{project.AssemblyName}{GetOutputExtension(compilation.Options.OutputKind)}";

                return Combine(outputDirectory, outputFileName);
            }

            // TODO: TBD: may further report Emit diagnostics ...
            var outputPath = GetProjectOutputFilePath() ?? GetDesiredProjectOutputPath();
            // ReSharper disable once AssignNullToNotNullAttribute
            var pdbPath = Combine(GetDirectoryName(outputPath), $"{GetFileNameWithoutExtension(outputPath)}{dot}{pdb}");

            diagnosticFilter.Result = compilation.Emit(outputPath, pdbPath);

            base.OnEvaluateCompilation(project, diagnosticFilter);
        }
    }
}
