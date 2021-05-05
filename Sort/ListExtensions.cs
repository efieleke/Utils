using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sayer.Collections;

namespace Sayer.Sort
{
    public static class ListExtensions
    {
        /// <summary>Searches a one-dimensional sorted array for a value, using the default comparer.</summary>
        /// <param name="source">The sorted one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <typeparam name="T">The type of the elements of the array.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this T[] source, T item) => Array.BinarySearch(source, item);

        /// <summary>Searches a one-dimensional sorted array for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
        /// <param name="source">The sorted one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.</param>
        /// <typeparam name="T">The type of the elements of the array.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this T[] source, T item, IComparer<T> comparer) => Array.BinarySearch(source, item, comparer);

        /// <summary>Searches a range of elements in a one-dimensional sorted array for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
        /// <param name="source">The sorted one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
        /// <param name="start">The starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.</param>
        /// <typeparam name="T">The type of the elements of the array.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this T[] source, int start, int count, T item, IComparer<T> comparer) => Array.BinarySearch(source, start, count, item, comparer);

        /// <summary>Searches a sorted list for a value, using the default comparer.</summary>
        /// <param name="source">The sorted list to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this List<T> source, T item) => source.BinarySearch(item);

        /// <summary>Searches a sorted list for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
        /// <param name="source">The sorted list to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this List<T> source, T item, IComparer<T> comparer) => source.BinarySearch(item, comparer);

        /// <summary>Searches a sorted list for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
        /// <param name="source">The sorted list to search.</param>
        /// <param name="start">The starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this List<T> source, int start, int count, T item, IComparer<T> comparer) => source.BinarySearch(start, count, item, comparer);

        /// <summary>Searches a sorted list for a value, using the default comparer.</summary>
        /// <param name="source">The sorted list to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this IList<T> source, T item) => IndexOfSorted(source.AsReadOnly(), 0, source.Count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>Searches a sorted list for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
        /// <param name="source">The sorted list to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this IList<T> source, T item, IComparer<T> comparer) => IndexOfSorted(source.AsReadOnly(), 0, source.Count, item, comparer.Compare);

        /// <summary>Searches a sorted list for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
        /// <param name="source">The sorted list to search.</param>
        /// <param name="start">The starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this IList<T> source, int start, int count, T item, IComparer<T> comparer) => IndexOfSorted(source.AsReadOnly(), start, count, item, comparer.Compare);

        /// <summary>Searches a sorted list for a value, using the default comparer.</summary>
        /// <param name="source">The sorted list to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this IReadOnlyList<T> source, T item) => IndexOfSorted(source, 0, source.Count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>Searches a sorted list for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
        /// <param name="source">The sorted list to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this IReadOnlyList<T> source, T item, IComparer<T> comparer) => IndexOfSorted(source, 0, source.Count, item, comparer.Compare);

        /// <summary>Searches a sorted list for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
        /// <param name="source">The sorted list to search.</param>
        /// <param name="start">The starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T>(this IReadOnlyList<T> source, int start, int count, T item, IComparer<T> comparer) => IndexOfSorted(source, start, count, item, comparer.Compare);

        /// <summary>Searches a sorted list for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
        /// <param name="source">The sorted list to search.</param>
        /// <param name="start">The starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        /// <param name="item">The object to search for.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <typeparam name="V">The type of element to search for</typeparam>
        /// <returns>
        /// The index of the specified <paramref name="item" /> in the specified <paramref name="source" />, if <paramref name="item" /> is found; otherwise, a negative number. If <paramref name="item" /> is not found and
        /// <paramref name="item" /> is less than one or more elements in <paramref name="source" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="item" />.
        /// If <paramref name="item" /> is not found and <paramref name="item" /> is greater than all elements in <paramref name="source" />, the negative number returned is the bitwise complement of (the index of the last element plus 1).
        /// If this method is called with a non-sorted <paramref name="source" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="item" /> is present in <paramref name="source" />.
        /// </returns>
        public static int IndexOfSorted<T, V>(this IReadOnlyList<T> source, int start, int count, V item, Func<T, V, int> comparer)
        {
            int begin = start;
            int end = start + count - 1;

            while (begin <= end)
            {
                int middle = begin + ((end - begin) >> 1);
                int compare = comparer(source[middle], item);

                if (compare < 0)
                {
                    begin = middle + 1;
                }
                else if (compare > 0)
                {
                    end = middle - 1;
                }
                else
                {
                    return middle;
                }
            }

            return ~begin;
        }

        /// <summary>Returns the index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</returns>
        public static int UpperBound<T>(this T[] source, T item) => UpperBound(source.AsReadOnly(), 0, source.Length, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>
        /// Returns the index of the first element in the sorted collection that is greater than the specified item, using the provided comparer.
        /// If no elements are greater, returns the collection count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</returns>
        public static int UpperBound<T>(this T[] source, T item, IComparer<T> comparer) => UpperBound(source.AsReadOnly(), 0, source.Length, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</returns>
        public static int UpperBound<T>(this T[] source, int start, int count, T item) => UpperBound(source.AsReadOnly(), start, count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</returns>
        public static int UpperBound<T>(this T[] source, int start, int count, T item, IComparer<T> comparer) => UpperBound(source.AsReadOnly(), start, count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</returns>
        public static int UpperBound<T>(this List<T> source, T item) => UpperBound(source, 0, source.Count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>
        /// Returns the index of the first element in the sorted collection that is greater than the specified item, using the provided comparer.
        /// If no elements are greater, returns the collection count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</returns>
        public static int UpperBound<T>(this List<T> source, T item, IComparer<T> comparer) => UpperBound(source, 0, source.Count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</returns>
        public static int UpperBound<T>(this List<T> source, int start, int count, T item) => UpperBound(source, start, count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</returns>
        public static int UpperBound<T>(this List<T> source, int start, int count, T item, IComparer<T> comparer) => UpperBound(source, start, count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</returns>
        public static int UpperBound<T>(this IList<T> source, T item) => UpperBound(source.AsReadOnly(), 0, source.Count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>
        /// Returns the index of the first element in the sorted collection that is greater than the specified item, using the provided comparer.
        /// If no elements are greater, returns the collection count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</returns>
        public static int UpperBound<T>(this IList<T> source, T item, IComparer<T> comparer) => UpperBound(source.AsReadOnly(), 0, source.Count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</returns>
        public static int UpperBound<T>(this IList<T> source, int start, int count, T item) => UpperBound(source.AsReadOnly(), start, count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</returns>
        public static int UpperBound<T>(this IList<T> source, int start, int count, T item, IComparer<T> comparer) => UpperBound(source.AsReadOnly(), start, count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</returns>
        public static int UpperBound<T>(this IReadOnlyList<T> source, T item) => UpperBound(source, 0, source.Count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>
        /// Returns the index of the first element in the sorted collection that is greater than the specified item, using the provided comparer.
        /// If no elements are greater, returns the collection count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted collection that is greater than the specified item. If no elements are greater, returns the collection count.</returns>
        public static int UpperBound<T>(this IReadOnlyList<T> source, T item, IComparer<T> comparer) => UpperBound(source, 0, source.Count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</returns>
        public static int UpperBound<T>(this IReadOnlyList<T> source, int start, int count, T item) => UpperBound(source, start, count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</returns>
        public static int UpperBound<T>(this IReadOnlyList<T> source, int start, int count, T item, IComparer<T> comparer) => UpperBound(source, start, count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted range that is greater than the specified item. If no elements are greater, returns start + count.</returns>
        public static int UpperBound<T, V>(this IReadOnlyList<T> source, int start, int count, V item, Func<T, V, int> comparer)
        {
            int begin = start;
            int end = start + count - 1;

            while (begin <= end)
            {
                int middle = begin + ((end - begin) >> 1);
                int compare = comparer(source[middle], item);

                if (compare <= 0)
                {
                    begin = middle + 1;
                }
                else
                {
                    end = middle - 1;
                }
            }

            return begin;
        }

        /// <summary>Returns the index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</returns>
        public static int LowerBound<T>(this T[] source, T item) => LowerBound(source.AsReadOnly(), 0, source.Length, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>
        /// Returns the index of the first element in the sorted collection that is greater than or equal to the specified item, using the provided comparer.
        /// If no elements match that condition, returns the collection count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</returns>
        public static int LowerBound<T>(this T[] source, T item, IComparer<T> comparer) => LowerBound(source.AsReadOnly(), 0, source.Length, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than or equal to the specified item. If no elements match that condition, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted range that is greater than or equal to the specified item. If no elements match that condition, returns start + count.</returns>
        public static int LowerBound<T>(this T[] source, int start, int count, T item, IComparer<T> comparer) => LowerBound(source.AsReadOnly(), start, count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</returns>
        public static int LowerBound<T>(this List<T> source, T item) => LowerBound(source, 0, source.Count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>
        /// Returns the index of the first element in the sorted collection that is greater than or equal to the specified item, using the provided comparer.
        /// If no elements match that condition, returns the collection count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</returns>
        public static int LowerBound<T>(this List<T> source, T item, IComparer<T> comparer) => LowerBound(source, 0, source.Count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than or equal to the specified item. If no elements match that condition, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted range that is greater than or equal to the specified item. If no elements match that condition, returns start + count.</returns>
        public static int LowerBound<T>(this List<T> source, int start, int count, T item, IComparer<T> comparer) => LowerBound(source, start, count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</returns>
        public static int LowerBound<T>(this IList<T> source, T item) => LowerBound(source.AsReadOnly(), 0, source.Count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>
        /// Returns the index of the first element in the sorted collection that is greater than or equal to the specified item, using the provided comparer.
        /// If no elements match that condition, returns the collection count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</returns>
        public static int LowerBound<T>(this IList<T> source, T item, IComparer<T> comparer) => LowerBound(source.AsReadOnly(), 0, source.Count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than or equal to the specified item. If no elements match that condition, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted range that is greater than or equal to the specified item. If no elements match that condition, returns start + count.</returns>
        public static int LowerBound<T>(this IList<T> source, int start, int count, T item, IComparer<T> comparer) => LowerBound(source.AsReadOnly(), start, count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <returns>The index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</returns>
        public static int LowerBound<T>(this IReadOnlyList<T> source, T item) => LowerBound(source, 0, source.Count, item, System.Collections.Generic.Comparer<T>.Default.Compare);

        /// <summary>
        /// Returns the index of the first element in the sorted collection that is greater than or equal to the specified item, using the provided comparer.
        /// If no elements match that condition, returns the collection count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted collection that is greater than or equal to the specified item. If no elements match that condition, returns the collection count.</returns>
        public static int LowerBound<T>(this IReadOnlyList<T> source, T item, IComparer<T> comparer) => LowerBound(source, 0, source.Count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than or equal to the specified item. If no elements match that condition, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer.</param>
        /// <returns>The index of the first element in the sorted range that is greater than or equal to the specified item. If no elements match that condition, returns start + count.</returns>
        public static int LowerBound<T>(this IReadOnlyList<T> source, int start, int count, T item, IComparer<T> comparer) => LowerBound(source, start, count, item, comparer.Compare);

        /// <summary>Returns the index of the first element in the sorted range that is greater than or equal to the specified item. If no elements match that condition, returns start + count.</summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="source">The collection to search</param>
        /// <param name="start">The starting index</param>
        /// <param name="count">The length of the search range</param>
        /// <param name="item">The item used for comparison</param>
        /// <param name="comparer">A comparer that returns 0 if T equals V, a negative number if T is less than V, and a positive number if T is greater than V</param>
        /// <returns>The index of the first element in the sorted range that is greater than or equal to the specified item. If no elements match that condition, returns start + count.</returns>
        public static int LowerBound<T, V>(this IReadOnlyList<T> source, int start, int count, V item, Func<T, V, int> comparer)
        {
            int begin = start;
            int end = start + count - 1;

            while (begin <= end)
            {
                int middle = begin + ((end - begin) >> 1);
                int compare = comparer(source[middle], item);

                if (compare < 0)
                {
                    begin = middle + 1;
                }
                else
                {
                    end = middle - 1;
                }
            }

            return begin;
        }

        /// <summary>
        /// Sorts the list using a non-stable sort
        /// </summary>
        /// <typeparam name="T">Type of element in the list</typeparam>
        /// <param name="source">The list to sort</param>
        /// <param name="comparison">Compare delegate</param>
        public static void SortAll<T>(this IList<T> source, Comparison<T> comparison) => SortRange(source, 0, source.Count, comparison);

        /// <summary>
        /// Sorts a range of the list using a non-stable sort
        /// </summary>
        /// <typeparam name="T">Type of element in the list</typeparam>
        /// <param name="source">The list to sort</param>
        /// <param name="first">The starting element of the range to sort</param>
        /// <param name="count">The number of elements to sort</param>
        /// <param name="comparison">Compare delegate</param>
        public static void SortRange<T>(this IList<T> source, int first, int count, Comparison<T> comparison) =>
            ArrayList.Adapter((IList)source).Sort(first, count, new ComparisonWrapper<T>(comparison));

        private class ComparisonWrapper<T> : IComparer
        {
            public ComparisonWrapper(Comparison<T> comparison) { _comparison = comparison; }
            public int Compare(object o1, object o2) => _comparison((T)o1, (T)o2);
            private readonly Comparison<T> _comparison;
        }

        /// <summary>
        /// Sorts the list using MergeSort, which is a stable sort
        /// </summary>
        /// <typeparam name="T">Type of element in the list</typeparam>
        /// <param name="source">The list to sort</param>
        /// <param name="comparison">Compare delegate</param>
        public static void MergeSort<T>(this IList<T> source, Comparison<T> comparison) => MergeSort(source, 0, source.Count, comparison);

        /// <summary>
        /// Sorts a portion of the list using MergeSort, which is a stable sort
        /// </summary>
        /// <typeparam name="T">Type of element in the list</typeparam>
        /// <param name="list">The list to sort</param>
        /// <param name="first">The starting element of the range to sort</param>
        /// <param name="count">The number of elements to sort</param>
        /// <param name="comparison">Compare delegate</param>
        public static void MergeSort<T>(this IList<T> source, int first, int count, Comparison<T> comparison)
        {
            var buffer = new T[count];

            if (first > 0 || count < source.Count)
            {
                MergeSort(new WriteableSubList<T>(source, first, count), comparison, buffer);
            }
            else
            {
                MergeSort(source, comparison, buffer);
            }
        }

        private static void MergeSort<T>(IList<T> list, Comparison<T> comparison, T[] buffer)
        {
            if (list.Count < 2)
            {
                return;
            }

            var leftSubList = new WriteableSubList<T>(list, 0, list.Count / 2);
            var rightSubList = new WriteableSubList<T>(list, list.Count / 2, int.MaxValue);
            MergeSort(leftSubList, comparison, buffer);
            MergeSort(rightSubList, comparison, buffer);
            int leftIndex = 0;
            int rightIndex = 0;

            for (int i = 0; i < list.Count; ++i)
            {
                int compare = leftIndex == leftSubList.Count ? 1 : (rightIndex == rightSubList.Count ? -1 : comparison(leftSubList[leftIndex], rightSubList[rightIndex]));

                if (compare <= 0)
                {
                    buffer[i] = leftSubList[leftIndex++];
                }
                else
                {
                    buffer[i] = rightSubList[rightIndex++];
                }
            }

            for (int i = 0; i < list.Count; ++i)
            {
                list[i] = buffer[i];
            }
        }

        /// <summary>
        /// Represents a view into a list. The only aspect of this class which is not read-only is that an element in the
        /// list can be overwritten. Items cannot be added or removed.
        ///
        /// Behavior of this class is not defined if the parent list is modified. This class is solely for optimization purposes
        /// (avoiding collection copies). Because care is required when using it, it is private within this namespace.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class WriteableSubList<T> : IList<T>
        {
            internal WriteableSubList(IList<T> source, int offset, int count)
            {
                if (source is WriteableSubList<T> subList)
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
            public bool IsReadOnly => false;
            public void Add(T item) => throw new NotImplementedException("Unexpected call to WriteableSubList.Add");
            public void Clear() => throw new NotImplementedException("Unexpected call to WriteableSubList.Clear");
            public bool Contains(T item) => throw new NotImplementedException("Unexpected call to WriteableSubList.Contains");
            public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException("Unexpected call to WriteableSubList.CopyTo");
            public bool Remove(T item) => throw new NotImplementedException("Unexpected call to WriteableSubList.Remove");
            public int IndexOf(T item) => throw new NotImplementedException("Unexpected call to WriteableSubList.IndexOf");
            public void Insert(int index, T item) => throw new NotImplementedException("Unexpected call to WriteableSubList.Insert");
            public void RemoveAt(int index) => throw new NotImplementedException("Unexpected call to WriteableSubList.RemoveAt");

            public T this[int index]
            {
                get => _list[_offset + index];
                set => _list[_offset + index] = value;
            }

            private readonly IList<T> _list;
            private readonly int _offset;
        }
    }
}
