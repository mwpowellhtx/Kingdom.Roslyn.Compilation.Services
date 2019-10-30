using System;
using System.Collections.Generic;

namespace Kingdom.Roslyn.Compilation
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;
    using WorkspacePropertiesDictionary = Dictionary<string, string>;
    using IWorkspacePropertiesDictionary = IDictionary<string, string>;

    // TODO: TBD: could potentially even deliver this one as a separate package as well...
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// This Manager derives for purposes of supporting Microsoft Build centric Compilation.
    /// </summary>
    /// <inheritdoc />
    public class MSBuildCompilationManager : CompilationManager<MSBuildWorkspace>
    {
        /// <summary>
        /// Gets the <see cref="Lazy{T}"/> <see cref="MSBuildWorkspace"/> instance.
        /// </summary>
        /// <inheritdoc />
        protected override Lazy<MSBuildWorkspace> LazyWorkspace { get; }

        private readonly IWorkspacePropertiesDictionary _workspaceProperties;

        /// <summary>
        /// Override in order to add different <see cref="WorkspaceProperties"/> as needed.
        /// Adds the <see cref="CompilationManager.Configuration"/> property in the specified
        /// value by default.
        /// </summary>
        /// <param name="workspaceProperties"></param>
        /// <returns></returns>
        protected virtual IWorkspacePropertiesDictionary PrepareWorkspaceProperties(IWorkspacePropertiesDictionary workspaceProperties)
        {
            workspaceProperties[nameof(Configuration)] = Configuration;
            return workspaceProperties;
        }

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Override or Add <see cref="Workspace"/> Property items to the Manager.
        /// </summary>
        public IWorkspacePropertiesDictionary WorkspaceProperties => PrepareWorkspaceProperties(_workspaceProperties);

        //protected override Solution Solution => base.Solution;

        // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
        /// <summary>
        /// Default Public Constructor.
        /// </summary>
        /// <param name="configuration">A caller provided Configuration.
        /// The default is <see cref="CompilationManager.Release"/>.</param>
        public MSBuildCompilationManager(string configuration = Release)
            : this(new WorkspacePropertiesDictionary { }, configuration)
        {
        }

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="workspaceProperties">Subscribers may furnish their own set of Workspace
        /// Properties for use throughout the build cycle.</param>
        /// <param name="configuration">A caller provided Configuration.
        /// The default is <see cref="CompilationManager.Release"/>.</param>
        public MSBuildCompilationManager(IWorkspacePropertiesDictionary workspaceProperties
            , string configuration = Debug)
            : base(configuration)
        {
            // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
            _workspaceProperties = workspaceProperties ?? new WorkspacePropertiesDictionary { };

            LazyWorkspace = new Lazy<MSBuildWorkspace>(() => MSBuildWorkspace.Create(WorkspaceProperties));

            //// TODO: TBD: perhaps add test projects and member assets as embedded resources that we extrapolate and evaluate ...
            //// TODO: TBD: which can open solution, open projects, etc...
            //new MSBuildWorkspace()
            //new MSBuildWorkspace().OpenSolutionAsync("")

            // TODO: TBD: then what to do about "sources", never mind "solution", "projects", etc...
        }
    }
}
