using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sayer.Collections
{
    public static class ListExtensions
    {
        /// <summary>
        /// Sets elements in the list to a single value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="value">The value to set each element to</param>
        /// <param name="startIndex">The starting index within the list</param>
        /// <param name="count">The number of elements to set. It is safe to pass a value greater the the list count.</param>
        public static void Fill<T>(this IList<T> list, T value, int startIndex = 0, uint count = uint.MaxValue)
        {
            uint fillCount = Math.Min(count, (uint)(list.Count - startIndex));

            for (int i = 0; i < fillCount; ++i)
            {
                list[i + startIndex] = value;
            }
        }

        /// <summary>
        /// This returns a list consisting of a range of elements within this list. The returned list is a view into this list. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source list, which serves as the backing collection</param>
        /// <param name="start">The starting index into the source list</param>
        /// <param name="count">
        /// The number of elements in the sublist. This can be greater than the number of remaining elements in the list, in which case all the
        /// remaining elements in the list are included.
        /// </param>
        /// <returns></returns>
        public static IReadOnlyList<T> GetSubList<T>(this T[] source, int start, int count) => new SubList<T>(source, start, count);

        /// <summary>
        /// This returns a list consisting of a range of elements within this list. The returned list is
        /// a view into this list. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source list, which serves as the backing collection</param>
        /// <param name="start">The starting index into the source list</param>
        /// <param name="count">
        /// The number of elements in the sublist. This can be greater than the number of remaining elements in the list, in which case all the
        /// remaining elements in the list are included.
        /// </param>
        /// <returns></returns>
        public static IReadOnlyList<T> GetSubList<T>(this List<T> source, int start, int count) => new SubList<T>(source, start, count);

        /// <summary>
        /// This returns a list consisting of a range of elements within this list. The returned list is
        /// a view into this list. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source list, which serves as the backing collection</param>
        /// <param name="start">The starting index into the source list</param>
        /// <param name="count">
        /// The number of elements in the sublist. This can be greater than the number of remaining elements in the list, in which case all the
        /// remaining elements in the list are included.
        /// </param>
        /// <returns></returns>
        public static IReadOnlyList<T> GetSubList<T>(this IList<T> source, int start, int count) => new SubList<T>(source.AsReadOnly(), start, count);

        /// <summary>
        /// This returns a list consisting of a range of elements within this list. The returned list is
        /// a view into this list. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source list, which serves as the backing collection</param>
        /// <param name="start">The starting index into the source list</param>
        /// <param name="count">
        /// The number of elements in the sublist. This can be greater than the number of remaining elements in the list, in which case all the
        /// remaining elements in the list are included.
        /// </param>
        /// <returns></returns>
        public static IReadOnlyList<T> GetSubList<T>(this IReadOnlyList<T> source, int start, int count) => new SubList<T>(source, start, count);

        /// <summary>
        /// Returns the index of the first element that matches the predicate, or source.Count if there are no matches
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this T[] source, Predicate<T> match) => IndexOf(source, 0, match);

        /// <summary>
        /// Returns the index of the first element that matches the predicate, or source.Count if there are no matches
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this IReadOnlyList<T> source, Predicate<T> match) => IndexOf(source, 0, match);

        /// <summary>
        /// Returns the first index of the element that matches the predicate, or source.Count if there are no matches
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this List<T> source, Predicate<T> match) => ((IReadOnlyList<T>)source).IndexOf(match);

        /// <summary>
        /// Returns the first index of the element that matches the predicate, or source.Count if there are no matches
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this IList<T> source, Predicate<T> match) => source.AsReadOnly().IndexOf(match);

        /// <summary>
        /// Returns the index of the first element that matches the predicate, or source.Count if there are no matches
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="startIndex">The starting index. If this is >= to the count of source, this method returns the count of source.</param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this IReadOnlyList<T> source, int startIndex, Predicate<T> match)
        {
            int count = source.Count;

            for (int i = startIndex; i < count; ++i)
            {
                if (match(source[i]))
                {
                    return i;
                }
            }

            return count;
        }

        /// <summary>
        /// Returns the index of the minimum element in the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with minimum value</returns>
        public static int IndexOfMin<T>(this List<T> source) => IndexOfMin((IReadOnlyList<T>)source, 0, Comparer<T>.Default);

        /// <summary>
        /// Returns the index of the minimum element in the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns>The index of element with minimum value</returns>
        public static int IndexOfMin<T>(this IList<T> source) => IndexOfMin(source.AsReadOnly(), 0, Comparer<T>.Default);

        /// <summary>
        /// Returns the index of the minimum element in the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns>The index of element with minimum value</returns>
        public static int IndexOfMin<T>(this IReadOnlyList<T> source) => IndexOfMin(source, 0, Comparer<T>.Default);

        /// <summary>
        /// Returns the index of the minimum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with minimum value</returns>
        public static int IndexOfMin<T>(this List<T> source, IComparer<T> comparer) => IndexOfMin((IReadOnlyList<T>)source, 0, comparer);

        /// <summary>
        /// Returns the index of the minimum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with minimum value</returns>
        public static int IndexOfMin<T>(this IList<T> source, IComparer<T> comparer) => IndexOfMin(source.AsReadOnly(), 0, comparer);

        /// <summary>
        /// Returns the index of the minimum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with minimum value</returns>
        public static int IndexOfMin<T>(this IReadOnlyList<T> source, IComparer<T> comparer) => IndexOfMin(source, 0, comparer);

        /// <summary>
        /// Returns the index of the minimum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="startIndex">The index at which to begin searching. If greater than or equal to the list count, throws an ArgumentException</param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with minimum value</returns>
        public static int IndexOfMin<T>(this List<T> source, int startIndex, IComparer<T> comparer) => IndexOfMin((IReadOnlyList<T>)source, startIndex, comparer);

        /// <summary>
        /// Returns the index of the minimum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="startIndex">The index at which to begin searching. If greater than or equal to the list count, throws an ArgumentException</param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with minimum value</returns>
        public static int IndexOfMin<T>(this IList<T> source, int startIndex, IComparer<T> comparer) => IndexOfMin(source.AsReadOnly(), startIndex, comparer);

        /// <summary>
        /// Returns the index of the minimum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="startIndex">The index at which to begin searching. If greater than or equal to the list count, throws an ArgumentException</param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with minimum value</returns>
        public static int IndexOfMin<T>(this IReadOnlyList<T> source, int startIndex, IComparer<T> comparer)
        {
            int count = source.Count;
            int minIndex = startIndex;

            if (startIndex >= count)
            {
                throw new ArgumentException($"Start index {startIndex} is >= the list count {count}");
            }

            for (int i = startIndex + 1; i < count; ++i)
            {
                if (comparer.Compare(source[i], source[minIndex]) < 0)
                {
                    minIndex = i;
                }
            }

            return minIndex;
        }

        /// <summary>
        /// Returns the index of the maximum element in the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns>The index of element with maximum value..</returns>
        public static int IndexOfMax<T>(this List<T> source) => IndexOfMax((IReadOnlyList<T>)source, 0, Comparer<T>.Default);

        /// <summary>
        /// Returns the index of the maximum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns>The index of element with maximum value..</returns>
        public static int IndexOfMax<T>(this IList<T> source) => IndexOfMax(source.AsReadOnly(), 0, Comparer<T>.Default);

        /// <summary>
        /// Returns the index of the maximum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns>The index of element with maximum value</returns>
        public static int IndexOfMax<T>(this IReadOnlyList<T> source) => IndexOfMax(source, 0, Comparer<T>.Default);

        /// <summary>
        /// Returns the index of the maximum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with maximum value</returns>
        public static int IndexOfMax<T>(this List<T> source, IComparer<T> comparer) => IndexOfMax((IReadOnlyList<T>)source, 0, comparer);

        /// <summary>
        /// Returns the index of the maximum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with maximum value</returns>
        public static int IndexOfMax<T>(this IList<T> source, IComparer<T> comparer) => IndexOfMax(source.AsReadOnly(), 0, comparer);

        /// <summary>
        /// Returns the index of the maximum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with maximum value</returns>
        public static int IndexOfMax<T>(this IReadOnlyList<T> source, IComparer<T> comparer) => IndexOfMax(source, 0, comparer);

        /// <summary>
        /// Returns the index of the maximum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="startIndex">The index at which to begin searching. If greater than or equal to the list count, throws an ArgumentException</param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with maximum value</returns>
        public static int IndexOfMax<T>(this List<T> source, int startIndex, IComparer<T> comparer) => IndexOfMax((IReadOnlyList<T>)source, startIndex, comparer);

        /// <summary>
        /// Returns the index of the maximum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="startIndex">The index at which to begin searching. If greater than or equal to the list count, throws an ArgumentException</param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with maximum value</returns>
        public static int IndexOfMax<T>(this IList<T> source, int startIndex, IComparer<T> comparer) => IndexOfMax(source.AsReadOnly(), startIndex, comparer);

        /// <summary>
        /// Returns the index of the maximum element in the list, as determined by the provided comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="startIndex">The index at which to begin searching. If greater than or equal to the list count, throws an ArgumentException</param>
        /// <param name="comparer"></param>
        /// <returns>The index of element with maximum value</returns>
        public static int IndexOfMax<T>(this IReadOnlyList<T> source, int startIndex, IComparer<T> comparer)
        {
            int count = source.Count;
            int indexOfMax = startIndex;

            if (startIndex >= count)
            {
                throw new ArgumentException($"Start index {startIndex} is >= the list count {count}");
            }

            for (int i = startIndex + 1; i < count; ++i)
            {
                if (comparer.Compare(source[i], source[indexOfMax]) > 0)
                {
                    indexOfMax = i;
                }
            }

            return indexOfMax;
        }

        /// <summary>
        /// Splits the source array into a specified number of segments. If the size of the input array is not evenly divisible
        /// by the number of segments, the first N sublists will each have one more element. For example, if an array of 100 elements
        /// is split into 49 segments, the first sublist will have 3 elements and the rest will have 2 elements. If that array were
        /// instead split into 51 segments, the first 49 sublists will have 2 elements and the last two sublists will have 1 element.
        ///
        /// The returned sublists are "views" into the source array. This means that the split operation is instant and memory efficient.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source list</param>
        /// <param name="numSegments">
        /// The number of sub-lists to create. If this number is larger than the number of elements in the source list,
        /// it will be set to the number of elements in the source list.
        /// </param>
        /// <returns>An enumerable of lists</returns>
        public static IEnumerable<IReadOnlyList<T>> Split<T>(this T[] source, int numSegments) => Split((IReadOnlyList<T>)source, numSegments);

        /// <summary>
        /// Splits the source list into a specified number of segments. If the size of the input list is not evenly divisible
        /// by the number of segments, the first N sub lists will each have one more element. For example, if a list of 100 elements
        /// is split into 49 segments, the first sublist will have 3 elements and the rest will have 2 elements. If that list were
        /// instead split into 51 segments, the first 49 sublists will have 2 elements and the last two sublists will have 1 element.
        ///
        /// The returned sublists are "views" into the source collection. This means that the split operation is instant and memory efficient.
        /// It also means that if elements are removed from the source after this split method is called, but before the results are enumerated,
        /// an out of bounds violation will occur during enumeration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source list</param>
        /// <param name="numSegments">
        /// The number of sub-lists to create. If this number is larger than the number of elements in the source list,
        /// it will be set to the number of elements in the source list.
        /// </param>
        /// <returns>An enumerable of lists</returns>
        public static IEnumerable<IReadOnlyList<T>> Split<T>(this List<T> source, int numSegments) => Split((IReadOnlyList<T>)source, numSegments);

        /// <summary>
        /// Splits the source list into a specified number of segments. If the size of the input list is not evenly divisible
        /// by the number of segments, the first N sub lists will each have one more element. For example, if a list of 100 elements
        /// is split into 49 segments, the first sublist will have 3 elements and the rest will have 2 elements. If that list were
        /// instead split into 51 segments, the first 49 sublists will have 2 elements and the last two sublists will have 1 element.
        ///
        /// The returned sublists are "views" into the source collection. This means that the split operation is instant and memory efficient.
        /// It also means that if elements are removed from the source after this split method is called, but before the results are enumerated,
        /// an out of bounds violation will occur during enumeration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source list</param>
        /// <param name="numSegments">
        /// The number of sub-lists to create. If this number is larger than the number of elements in the source list,
        /// it will be set to the number of elements in the source list.
        /// </param>
        /// <returns>An enumerable of lists</returns>
        public static IEnumerable<IReadOnlyList<T>> Split<T>(this IList<T> source, int numSegments) => Split(source.GetSubList(0, source.Count), numSegments);

        /// <summary>
        /// Splits the source list into a specified number of segments. If the size of the input list is not evenly divisible
        /// by the number of segments, the first N sub lists will each have one more element. For example, if a list of 100 elements
        /// is split into 49 segments, the first sublist will have 3 elements and the rest will have 2 elements. If that list were
        /// instead split into 51 segments, the first 49 sublists will have 2 elements and the last two sublists will have 1 element.
        ///
        /// The returned sublists are "views" into the source collection. This means that the split operation is instant and memory efficient.
        /// It also means that if elements are removed from the source after this split method is called, but before the results are enumerated,
        /// an out of bounds violation will occur during enumeration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source list</param>
        /// <param name="numSegments">
        /// The number of sub-lists to create. If this number is larger than the number of elements in the source list,
        /// it will be set to the number of elements in the source list.
        /// </param>
        /// <returns>An enumerable of lists</returns>
        public static IEnumerable<IReadOnlyList<T>> Split<T>(this IReadOnlyList<T> source, int numSegments)
        {
            // Check parameters
            if (numSegments < 1)
            {
                throw new ArgumentException("Number of segments must be positive and non-zero");
            }

            numSegments = Math.Min(numSegments, source.Count);

            if (numSegments > 0)
            {
                int batchSize = source.Count / numSegments;
                int remainder = source.Count % numSegments;
                int index = 0;

                for (int i = 0; i < numSegments; ++i)
                {
                    int count = i < remainder ? batchSize + 1 : batchSize;
                    yield return new SubList<T>(source, index, count);
                    index += count;
                }
            }
        }

        /// <summary>
        /// Swaps two elements within the source list. This is a no-op if the indices are the same.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source list</param>
        /// <param name="firstIndex">The index of the one element to swap</param>
        /// <param name="secondIndex">The index of the other element to swap</param>
        public static void Swap<T>(this IList<T> source, int firstIndex, int secondIndex)
        {
            if (firstIndex != secondIndex)
            {
                T temp = source[firstIndex];
                source[firstIndex] = source[secondIndex];
                source[secondIndex] = temp;
            }
        }

        /// <summary>
        /// Reverses the order of elements in this list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        public static void Reverse<T>(this IList<T> source)
        {
            for (int i = 0, j = source.Count - 1; i < j; ++i, --j)
            {
                // Could call Swap instead, but that makes a comparison check which is not necessary in this case
                T temp = source[i];
                source[i] = source[j];
                source[j] = temp;
            }
        }

        /// <summary>Removes all the elements that match the condition defined by the specified predicate.</summary>
        /// <param name="source"></param>
        /// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements removed from the <see cref="T:System.Collections.Generic.List`1" /> .</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="match" /> is <see langword="null" />.</exception>
        public static int RemoveMatches<T>(this IList<T> source, Predicate<T> match)
        {
            if (source is List<T> list)
            {
                return list.RemoveAll(match);
            }

            int beforeCount = source.Count;
            for (int i = source.Count - 1; i >= 0; --i)
            {
                if (match(source[i]))
                {
                    source.RemoveAt(i);
                }
            }

            return beforeCount - source.Count;
        }

        /// <summary>Removes all the elements that match the condition defined by the specified predicate.</summary>
        /// <param name="source"></param>
        /// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements removed from the <see cref="T:System.Collections.Generic.List`1" /> .</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="match" /> is <see langword="null" />.</exception>
        public static async Task<int> RemoveMatchesAsync<T>(this IList<T> source, Func<T, Task<bool>> match)
        {
            int beforeCount = source.Count;
            for (int i = source.Count - 1; i >= 0; --i)
            {
                if (await match(source[i]).ConfigureAwait(false))
                {
                    source.RemoveAt(i);
                }
            }

            return beforeCount - source.Count;
        }

        /// <summary>Removes a range of elements from the <see cref="T:System.Collections.Generic.List`1" />.</summary>
        /// <param name="source"></param>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than 0.-or-
        /// <paramref name="count" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the <see cref="T:System.Collections.Generic.List`1" />.</exception>
        public static void RemoveSpan<T>(this IList<T> source, int index, int count)
        {
            if (source is List<T> list)
            {
                list.RemoveRange(index, count);
            }
            else
            {
                if (source.Count - index < count)
                {
                    throw new ArgumentException("Not enough elements", nameof(count));
                }

                for (int i = index + count; i > index; --i)
                {
                    source.RemoveAt(i - 1);
                }
            }
        }

        /// <summary>
        /// Adds all items in an enumerable to the end of this list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="elements"></param>
        public static void AddAll<T>(this IList<T> source, IEnumerable<T> elements)
        {
            if (source is List<T> list)
            {
                list.AddRange(elements);
            }
            else
            {
                foreach (T element in elements)
                {
                    source.Add(element);
                }
            }
        }

        /// <summary>
        /// Moves all elements matching a predicate to the beginning of the list, shifting the remaining elements back (not necessarily in their original order).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The source list</param>
        /// <param name="matcher">The predicate. The argument is the index of the element in the list being examined.</param>
        /// <param name="numConcurrent">The number of concurrent calls to the matcher callback that are permitted.</param>
        /// <returns>The number of elements that matched the predicate</returns>
        public static async Task<int> SiftAsync<T>(this IList<T> list, Func<int, Task<bool>> matcher, int numConcurrent)
        {
            int matches = 0;

            if (numConcurrent < 2)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    if (await matcher(i).ConfigureAwait(false))
                    {
                        list.Swap(i, matches++);
                    }
                }

                return matches;
            }

            var siftData = new SiftData(list.Count, matcher);
            var tasks = new Task<List<int>>[numConcurrent];

            try
            {
                for (int i = 0; i < tasks.Length; ++i)
                {
                    tasks[i] = Task.Run(() => GetMatches(siftData));
                }

                List<int>[] matchLists = await Task.WhenAll(tasks).ConfigureAwait(false);
                var enumerators = new List<IEnumerator<int>>(matchLists.Length);

                foreach (List<int> matchList in matchLists.Where(l => l.Count > 0))
                {
                    List<int>.Enumerator enumerator = matchList.GetEnumerator();
                    enumerator.MoveNext();
                    enumerators.Add(enumerator);
                }

                // preserve the order of the sifted elements
                while (enumerators.Count > 0)
                {
                    int minValue = int.MaxValue;
                    int index = -1;

                    for (int i = 0; i < enumerators.Count; ++i)
                    {
                        if (enumerators[i].Current <= minValue)
                        {
                            index = i;
                            minValue = enumerators[i].Current;
                        }
                    }

                    list.Swap(enumerators[index].Current, matches++);

                    if (!enumerators[index].MoveNext())
                    {
                        enumerators[index].Dispose();
                        enumerators.RemoveAt(index);
                    }
                }
            }
            finally
            {
                foreach (Task<List<int>> task in tasks)
                {
                    task.Dispose();
                }
            }

            return matches;
        }

        private class SiftData
        {
            internal SiftData(int count, Func<int, Task<bool>> matcher)
            {
                _count = count;
                _matcher = matcher;
            }

            internal readonly int _count;
            internal readonly Func<int, Task<bool>> _matcher;
            internal volatile int _index = -1;
        }

        private static async Task<List<int>> GetMatches(SiftData siftData)
        {
            var matches = new List<int>();

            for (int i = Interlocked.Increment(ref siftData._index); i < siftData._count; i = Interlocked.Increment(ref siftData._index))
            {
                if (await siftData._matcher(i))
                {
                    matches.Add(i);
                }
            }

            return matches;
        }

        /// <summary>
        /// Moves all elements matching a predicate to the beginning of the list, shifting the remaining elements back (not necessarily in their original order).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The source list</param>
        /// <param name="matcher">The predicate. It takes the index of the element in the list being examined as an argument.</param>
        /// <returns>The number of elements that matched the predicate</returns>
        public static int Sift<T>(this IList<T> list, Predicate<int> matcher)
        {
            int matches = 0;

            for (int i = 0; i < list.Count; ++i)
            {
                if (matcher(i))
                {
                    list.Swap(i, matches++);
                }
            }

            return matches;
        }

        private class SubList<T> : IReadOnlyList<T>
        {
            internal SubList(IReadOnlyList<T> source, int offset, int count)
            {
                if (source is SubList<T> subList)
                {
                    _list = subList._list;
                    _offset = subList._offset + offset;
                }
                else
                {
                    _list = source;
                    _offset = offset;
                }

                Count = Math.Min(source.Count - offset, count);
            }

            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count { get; }

            public T this[int index] => _list[_offset + index];

            private readonly IReadOnlyList<T> _list;
            private readonly int _offset;
        }

        /// <summary>
        /// Converts the input IList to an IReadOnlyList. No copying of elements are performed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source list, which serves as the backing collection</param>
        /// <returns></returns>
        public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> source)
        {
            if (source is IReadOnlyList<T> readOnly)
            {
                return readOnly;
            }

            return new ReadOnlyList<T>(source);
        }

        private class ReadOnlyList<T> : IReadOnlyList<T>
        {
            internal ReadOnlyList(IList<T> list) { _list = list; }

            public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public int Count => _list.Count;
            public T this[int index] => _list[index];

            private readonly IList<T> _list;
        }
    }
}
