using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingdom.Roslyn.Compilation.MSBuild
{
    using Microsoft.Build.Locator;
    using static StringComparison;
    using static Microsoft.Build.Locator.MSBuildLocator;

    /// <summary>
    /// Provides a proxy managing the state of the <see cref="MSBuildLocator" />
    /// <see cref="VisualStudioInstance" /> registration. Allows for Default Registration,
    /// or if you happen to know the MSBuild SDK path, or you want to filter the
    /// <see cref="VisualStudioInstance"/> based on a set of selection criteria.
    /// </summary>
    /// <inheritdoc cref="MSBuildInstanceRegistrar{T}" />
    public class VisualStudioInstanceRegistrar : MSBuildInstanceRegistrar<VisualStudioInstance>, IVisualStudioInstanceRegistrar
    {
        /// <inheritdoc />
        public sealed override VisualStudioInstance RegisteredInstance { get; }

        private IEnumerable<VisualStudioInstance> _enumeratedInstances;

        /// <summary>
        /// Gets or Sets the Options. Default is Null, or we Query instances with no arguments.
        /// </summary>
        public static VisualStudioInstanceQueryOptions Options { get; set; }

        /// <inheritdoc />
        public sealed override IEnumerable<VisualStudioInstance> EnumeratedInstances
        {
            get
            {
                IEnumerable<VisualStudioInstance> QueryInstances()
                    => Options == null
                        ? QueryVisualStudioInstances()
                        : QueryVisualStudioInstances(Options);

                return _enumeratedInstances ?? (_enumeratedInstances = QueryInstances().ToArray());
            }
        }

        /// <summary>
        /// Evaluates whether we <see cref="CanRegister"/>.
        /// </summary>
        private static void EvaluateCanRegister()
        {
            if (!CanRegister)
            {
                throw new InvalidOperationException("Unable to register MSBuild.");
            }
        }

        /// <summary>
        /// Public Default Constructor.
        /// </summary>
        public VisualStudioInstanceRegistrar()
        {
            EvaluateCanRegister();

            if (!IsRegistered)
            {
                RegisteredInstance = RegisterDefaults();
            }
        }

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="msbuildPath"></param>
        public VisualStudioInstanceRegistrar(string msbuildPath)
        {
            EvaluateCanRegister();

            bool Equals(string a, string b) => string.Equals(a, b, InvariantCultureIgnoreCase);

            if (!IsRegistered)
            {
                RegisterMSBuildPath(msbuildPath);
            }

            RegisteredInstance = EnumeratedInstances.FirstOrDefault(x => Equals(x.MSBuildPath, msbuildPath));
        }

        /// <inheritdoc />
        public VisualStudioInstanceRegistrar(InstanceSelector instanceSelector) : this(new[] {instanceSelector})
        {
        }

        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="instanceSelectors">The Instance Selector instances. When dealing with
        /// string criteria, converts the operand to lower when the OS is not case-sensitive.
        /// Otherwise, leaves as-is. Converts the <see cref="DiscoveryType"/> from the underlying
        /// <see cref="Microsoft.Build.Locator.DiscoveryType"/> for comparison purposes.</param>
        /// <param name="all">When true, evaluates <paramref name="instanceSelectors"/> using All
        /// logic. When false, evaluates using Any logic.</param>
        /// <see cref="String"/>
        /// <see cref="Version"/>
        /// <see cref="DiscoveryType"/>
        public VisualStudioInstanceRegistrar(IEnumerable<InstanceSelector> instanceSelectors, bool all = true)
        {
            EvaluateCanRegister();

            if (!IsRegistered)
            {
                return;
            }

            bool SelectorPredicate(VisualStudioInstance instance, InstanceSelector selector)
            {
                var predicate = selector.Predicate;

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (selector.Selector)
                {
                    case SelectorType.DiscoveryType:
                        return true == predicate?.Invoke(instance.DiscoveryType.FromDiscoveryType());

                    case SelectorType.MSBuildPath:
                        return true == predicate?.Invoke(instance.MSBuildPath.AllowCaseSensitivity());

                    case SelectorType.Name:
                        return true == predicate?.Invoke(instance.Name.AllowCaseSensitivity());

                    case SelectorType.Version:
                        return true == predicate?.Invoke(instance.Version);

                    case SelectorType.VisualStudioRootPath:
                        return true == predicate?.Invoke(instance.VisualStudioRootPath.AllowCaseSensitivity());
                }

                throw new InvalidOperationException("The selector type was not found or is not supported.");
            }

            RegisteredInstance = EnumeratedInstances.FirstOrDefault(
                x => all
                    ? instanceSelectors.All(y => SelectorPredicate(x, y))
                    : instanceSelectors.Any(y => SelectorPredicate(x, y))
            );

            RegisterInstance(RegisteredInstance);
        }

        /// <inheritdoc />
        protected override void OnDispose(bool disposing)
        {
            // ReSharper disable once InvertIf
            if (disposing && !IsDisposed)
            {
                if (IsRegistered)
                {
                    Unregister();
                }
            }
        }
    }
}
