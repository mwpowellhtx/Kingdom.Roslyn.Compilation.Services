using System;
using System.Linq;

namespace Kingdom.Roslyn.Compilation
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using static String;

    // TODO: TBD: may have a look at my `Kingdom.CodeAnalysis.Verifiers.Diagnostics' project from the Kingdom.Collections work space...
    // TODO: TBD: in particular leveraging methods such as GetMetadataReferenceFromType, GetTypeAssembly, etc...
    // TODO: TBD: could potentially be refactored, and further abstracted out at the same time, i.e. to not be as fit for purpose, i.e. concerning metadata references, etc.
    // TODO: TBD: additionally, strictly speaking, we do not necessarily need to connect such a package offering with the Xunit test framework. Strictly speaking.
    /// <summary>
    /// Establishes a basic CompilationManager for Roslyn based compilation services. We
    /// intentionally steer clear of introducing any in the way of things like test framework
    /// dependencies as these are really beyond the immediate purview of the compilation manager
    /// itself. We will allow sensible extensibility for this purpose if test framework level
    /// verification is so desired apart from the exposed
    /// <see cref="CompilationManager.ResolveMetadataReferences"/> and
    /// <see cref="CompilationManager.EvaluateCompilation"/> events themselves.
    /// </summary>
    /// <inheritdoc />
    public abstract class CompilationManager<TWorkspace> : CompilationManager
        where TWorkspace : Workspace
    {
        /// <inheritdoc />
        protected CompilationManager(string configuration = Release)
            : base(configuration)
        {
        }

        // TODO: TBD: do I need to save project(s)/solution from being "adhoc" in order for this to play nicely with CG?
        // TODO: TBD: conversely, how might it be possible to CG using an in-memory Roslyn compilation?
        // TODO: TBD: because dotnet-cgr tooling runs in a process apart from the actual compilation...
        // TODO: TBD: I'm not sure that a compilation such as this would be able discover that output, not without help...

        /// <summary>
        /// Override in order to provide the <typeparamref name="TWorkspace"/> Factory.
        /// </summary>
        protected abstract Lazy<TWorkspace> LazyWorkspace { get; }

        /// <summary>
        /// Gets the Workspace involved during the Manager lifecycle.
        /// </summary>
        public virtual TWorkspace Workspace => LazyWorkspace.Value;

        /// <summary>
        /// Solution backing field.
        /// </summary>
        private Solution _solution;

        /// <summary>
        /// Gets the Solution involved during the Manager lifecycle. Starts from, or resets
        /// to, the <see cref="Microsoft.CodeAnalysis.Workspace.CurrentSolution"/>, depending
        /// on usage.
        /// </summary>
        /// <remarks>Privately Sets the Solution, especially during mutating operations.
        /// Which also prohibits us from being able to abstractly declare Solution at
        /// the base class level, as this would require the setter to be Protected,
        /// which we do not need to unnecessarily expose it to the rest of the class
        /// hierarchy.</remarks>
        public virtual Solution Solution
        {
            get => _solution ?? (_solution = Workspace.CurrentSolution);
            private set => _solution = value;
        }

        /// <summary>
        /// Creates a New Project assuming <paramref name="projectName"/> and constituent member
        /// <paramref name="sources"/>.
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="sources"></param>
        /// <returns></returns>
        protected virtual Project CreateProjectFromSources(string projectName, params string[] sources)
        {
            projectName = IsNullOrEmpty(projectName) ? GetNewAssetName() : projectName;

            var projectId = ProjectId.CreateNewId(debugName: projectName);

            // We will assume C# as the language of choice for this purpose.
            Solution = Solution.AddProject(projectId, projectName, projectName, Language);

            // The flow is a bit inside-out, we need to have Added the Project first, which modifies the Solution.
            Solution = OnResolveMetadataReferences(Solution, Solution.GetProject(projectId))
                    .WithProjectCompilationOptions(projectId, CompilationOptions)
                    .WithProjectParseOptions(projectId, ParseOptions)
                ;

            sources.ToList().ForEach(src =>
            {
                var assetName = GetNewAssetName();
                var assetFileName = $"{assetName}{LanguageDocumentExtension}";
                // Adds the Document Connected with the Project to the Solution. Also about inside-out in my opinion.
                Solution = Solution.AddDocument(DocumentId.CreateNewId(projectId), assetFileName,
                    SourceText.From(src));
            });

            // Hold off getting the project because we need the most up to date state.
            return Solution.GetProject(projectId);
        }

        public virtual void CompileAllProjects()
        {
            // TODO: TBD: not counting build orders?
            var projects = Solution.Projects;

            projects.ToList().ForEach(
                p =>
                {
                    // TODO: TBD: is this right? resolve the references prior to Compilation resolution?
                    Solution = OnResolveMetadataReferences(p.Solution, p);

                    // Which ensures that the Project instance we have is the correct one.
                    p = Solution.GetProject(p.Id)
                            .WithCompilationOptions(CompilationOptions)
                            .WithParseOptions(ParseOptions)
                        ;

                    ResolveCompilation(p, p.GetCompilationAsync());
                }
            );
        }

        // TODO: TBD: expand upon the CancellationToken aspects...
        /// <summary>
        /// Compiles the <paramref name="projectName"/> given constituent
        /// <paramref name="sources"/>, assuming the Manager configuration.
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="sources"></param>
        public virtual void Compile(string projectName, params string[] sources)
        {
            var project = CreateProjectFromSources(projectName, sources);
            ResolveCompilation(project, project.GetCompilationAsync());
        }

        /// <summary>
        /// Disposes the Object. In this instance we have a <see cref="Workspace"/> to Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        /// <see cref="Workspace"/>
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed || !disposing)
            {
                return;
            }

            Workspace?.Dispose();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // True, while always True, makes it clearer, Follow the Chain.
            base.Dispose(disposing);
        }
    }
}
