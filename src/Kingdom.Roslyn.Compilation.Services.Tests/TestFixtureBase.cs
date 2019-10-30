using System.Collections.Generic;

namespace Kingdom.Roslyn.Compilation
{
    using Xunit.Abstractions;

    public abstract class TestFixtureBase : Disposable
    {
        /// <summary>
        /// Gets the OutputHelper.
        /// </summary>
        protected ITestOutputHelper OutputHelper { get; }

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        protected TestFixtureBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        /// <summary>
        /// Returns the Range of <see cref="IEnumerable{T}"/> corresponding with the
        /// <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        protected static IEnumerable<T> GetRange<T>(params T[] values)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var value in values)
            {
                yield return value;
            }
        }
    }
}
