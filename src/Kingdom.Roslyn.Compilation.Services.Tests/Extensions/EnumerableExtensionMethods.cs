using System.Collections.Generic;

namespace Kingdom.Roslyn.Compilation
{
    public static class EnumerableExtensionMethods
    {
        /// <summary>
        /// Generates a Range of Values based on <paramref name="anchor"/> followed by zero
        /// or more optional <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anchor"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToRange<T>(this T anchor, params T[] values)
        {
            yield return anchor;

            foreach (var x in values)
            {
                yield return x;
            }
        }
    }
}
