using System.Collections.Generic;
using System.Linq;
using Sayer.Collections;

namespace Sayer.Sort
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Given a collection of sorted enumerables, enumerates through all of their combined elements in sorted order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerables">
        /// Each of the enumerables must have been sorted with the default comparer for this to properly return results in sorted order.
        /// </param>
        /// <returns>A sorted enumerable</returns>
        public static IEnumerable<T> GetSortedEnumerable<T>(this IEnumerable<IEnumerable<T>> enumerables) => GetSortedEnumerable(enumerables, Comparer<T>.Default);

        /// <summary>
        /// Given a collection of sorted enumerables and a comparer, enumerates through all of their combined elements in sorted order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerables">
        /// Each of the enumerables must have been sorted with the same criteria as the provided comparer for this to
        /// properly return results in sorted order.
        /// </param>
        /// <param name="comparer">Defines the sort order</param>
        /// <returns>A sorted enumerable</returns>
        public static IEnumerable<T> GetSortedEnumerable<T>(this IEnumerable<IEnumerable<T>> enumerables, IComparer<T> comparer)
        {
            var enumerators = new List<IEnumerator<T>>(enumerables.Select(e => e.GetEnumerator()));

            try
            {
                int numberInvalid = enumerators.Sift(i => !enumerators[i].MoveNext());

                for (int i = 0; i < numberInvalid; ++i)
                {
                    enumerators[i].Dispose();
                }

                enumerators.RemoveRange(0, numberInvalid);
                Comparer<IEnumerator<T>> compare = Comparer<IEnumerator<T>>.Create((a, b) => comparer.Compare(a.Current, b.Current));

                while (enumerators.Count > 0)
                {
                    int indexOfMin = enumerators.IndexOfMin(0, compare);
                    yield return enumerators[indexOfMin].Current;

                    if (!enumerators[indexOfMin].MoveNext())
                    {
                        enumerators[indexOfMin].Dispose();
                        enumerators.RemoveAt(indexOfMin);
                    }
                }
            }
            finally
            {
                foreach (IEnumerator<T> enumerator in enumerators)
                {
                    enumerator.Dispose();
                }
            }
        }
    }
}
