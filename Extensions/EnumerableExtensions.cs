using System;
using System.Collections.Generic;
using System.Linq;

namespace EZms.Core.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determines whether the collection is null or contains no elements.
        /// </summary>
        /// <typeparam name="T">The IEnumerable type.</typeparam>
        /// <param name="enumerable">The enumerable, which may be null or empty.</param>
        /// <returns>
        ///     <c>true</c> if the IEnumerable is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return true;
            }

            /* If this is a list, use the Count property for efficiency. 
             * The Count property is O(1) while IEnumerable.Count() is O(N). */
            if (enumerable is ICollection<T> collection)
            {
                return collection.Count < 1;
            }
            return !enumerable.GetEnumerator().MoveNext();
        }

        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> enumerable) => enumerable ?? Enumerable.Empty<T>();

        public static IEnumerable<T> OrderByKeys<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, params TKey[] sortOrderKeys)
        {
            if (sortOrderKeys == null || !sortOrderKeys.Any()) return source;
            if (source == null) return default(IEnumerable<T>);

            var enumerable = source as T[] ?? source.ToArray();

            var ordered = (from i in sortOrderKeys
                join o in enumerable
                    on i equals keySelector(o)
                select o).ToList();

            ordered.AddRange(enumerable.Except(ordered).OrderBy(keySelector));

            return ordered;
        }
    }
}
