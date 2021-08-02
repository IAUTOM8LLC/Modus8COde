using System;
using System.Collections.Generic;
using System.Linq;

namespace IAutoM8.Global.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> WhereIf<TSource>(
           this IEnumerable<TSource> source,
           bool condition,
           Func<TSource, bool> predicate)
        {
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// Executes the given action on each element in the source sequence
        /// and yields it.
        /// </summary>
        public static IEnumerable<T> Pipe<T>(this IEnumerable<T> source, Action<T> action)
        {
            // https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/Pipe.cs
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var element in source)
            {
                action(element);
                yield return element;
            }
        }
    }
}
