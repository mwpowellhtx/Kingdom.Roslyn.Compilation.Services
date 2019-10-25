using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kingdom.Roslyn.Compilation.Services.CodeGeneration
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ProjectContext : Disposable
    {

#if DEBUG
        internal const string Configuration = "Debug";
#else
        internal const string Configuration = "Release";
#endif

        internal string TargetFramework { get; } = "netcoreapp2.1";

        private Guid ProjectGuid { get; } = Guid.NewGuid();

        /// <summary>
        /// &quot;.csproj&quot;
        /// </summary>
        private const string ProjectFileExtension = ".csproj";

        /// <summary>
        /// &quot;.cs&quot;
        /// </summary>
        private const string CSharpFileExtension = ".cs";

        private string _projectName;

        private static string FullAssetName => Path.GetRandomFileName();

        private static string AssetName => Path.GetFileNameWithoutExtension(FullAssetName);

        internal string ProjectName => _projectName ?? (_projectName = AssetName);

        private IDictionary<Guid, string> _renderedCompilationUnits;

        internal IDictionary<Guid, string> RenderedCompilationUnits
            => _renderedCompilationUnits ?? (_renderedCompilationUnits
                   = CompilationUnits.ToDictionary(x => x.Key, x => x.Value.NormalizeWhitespace().ToFullString()));

        internal IDictionary<Guid, CompilationUnitSyntax> CompilationUnits { get; }
            = new Dictionary<Guid, CompilationUnitSyntax>();

        public ProjectContext(IEnumerable<string> renderedUnits)
        {
            _renderedCompilationUnits = renderedUnits.ToDictionary(_ => Guid.NewGuid());
        }

        public ProjectContext(IEnumerable<CompilationUnitSyntax> compilationUnits)
        {
            CompilationUnits = compilationUnits.ToDictionary(_ => Guid.NewGuid());
        }

        private string _projectDirectory;

        internal string ProjectDirectory => _projectDirectory ?? (_projectDirectory = ProjectName);

        internal string ProjectPath => Path.Combine(ProjectDirectory, $"{ProjectName}{ProjectFileExtension}");

        private static void RefreshFile(string path, string s)
        {
            var directory = Path.GetDirectoryName(path);

            Directory.CreateDirectory(directory);

            using (var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(s);
                }
            }
        }

        internal ProjectContext RefreshContents()
        {
            var type = GetType();

            const string projectTemplatePath = "ProjectTemplate.csproj.xml";

            using (var rs = type.Assembly.GetManifestResourceStream(type, projectTemplatePath))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                using (var sr = new StreamReader(rs))
                {
                    RefreshFile(ProjectPath, sr.ReadToEndAsync().Result);
                }
            }

            foreach (var (_, renderedUnit) in RenderedCompilationUnits)
            {
                var path = Path.Combine(ProjectDirectory, $"{AssetName}{CSharpFileExtension}");
                RefreshFile(path, renderedUnit);
            }

            return this;
        }

        /// <summary>
        /// Allows an opportunity to capture DiagnosticResults following Compilation.
        /// </summary>
        internal ICollection<DiagnosticResult> DiagnosticResults { get; } = new List<DiagnosticResult>();

        protected override void OnDispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                var projectDirectory = ProjectDirectory;

                if (!DiagnosticResults.Any() && Directory.Exists(projectDirectory))
                {
                    Directory.Delete(projectDirectory, true);
                }
            }

            base.OnDispose(disposing);
        }
    }
}
