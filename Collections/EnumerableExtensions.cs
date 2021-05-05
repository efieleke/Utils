using System;
using System.Collections.Generic;
using System.Linq;

namespace Sayer.Collections
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns the minimum value in a sequence, as defined by a custom selector and default comparer
        /// </summary>
        /// <typeparam name="TSource">the type of element in the sequence</typeparam>
        /// <typeparam name="TCompare">the type of object being compared</typeparam>
        /// <param name="source">the sequence</param>
        /// <param name="selector">the selector. Given an object of type TSource, this should return the value you wish to compare.</param>
        /// <returns>
        /// The minimum value in the sequence, as defined by the custom selector and default comparer.
        /// If there is more than one element that equates to the minimum, returns the first match.
        /// </returns>
        public static TSource Minimum<TSource, TCompare>(this IEnumerable<TSource> source, Func<TSource, TCompare> selector)
        {
            return Minimum(source, selector, Comparer<TCompare>.Default);
        }

        /// <summary>
        /// Returns the minimum value in a sequence, as defined by a custom selector and comparer
        /// </summary>
        /// <typeparam name="TSource">the type of element in the sequence</typeparam>
        /// <typeparam name="TCompare">the type of object being compared</typeparam>
        /// <param name="source">the sequence</param>
        /// <param name="selector">the selector. Given an object of type TSource, this should return the value you wish to compare.</param>
        /// <param name="comparer">the comparer used to compare items returned by the selector</param>
        /// <returns>
        /// The minimum value in the sequence, as defined by the custom selector and comparer.
        /// If there is more than one element that equates to the minimum, returns the first match.
        /// </returns>
        public static TSource Minimum<TSource, TCompare>(this IEnumerable<TSource> source, Func<TSource, TCompare> selector, IComparer<TCompare> comparer)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }
            if (selector == null) { throw new ArgumentNullException(nameof(selector)); }
            if (comparer == null) { throw new ArgumentNullException(nameof(comparer)); }

            return source.Aggregate((o1, o2) =>
            {
                TCompare toCompare1 = selector(o1);
                TCompare toCompare2 = selector(o2);
                return comparer.Compare(toCompare1, toCompare2) <= 0 ? o1 : o2;
            });
        }

        /// <summary>
        /// Returns the maximum value in a sequence, as defined by a custom selector and default comparer
        /// </summary>
        /// <typeparam name="TSource">the type of element in the sequence</typeparam>
        /// <typeparam name="TCompare">the type of object being compared</typeparam>
        /// <param name="source">the sequence</param>
        /// <param name="selector">the selector. Given an object of type TSource, return the value you wish to compare</param>
        /// <returns>
        /// The maximum value in the sequence, as defined by the custom selector and default comparer.
        /// If there is more than one element that equates to the maximum, returns the first match.
        /// </returns>
        public static TSource Maximum<TSource, TCompare>(this IEnumerable<TSource> source, Func<TSource, TCompare> selector)
        {
            return Maximum(source, selector, Comparer<TCompare>.Default);
        }

        /// <summary>
        /// Returns the maximum value in a sequence, as defined by a custom selector and comparer
        /// </summary>
        /// <typeparam name="TSource">the type of element in the sequence</typeparam>
        /// <typeparam name="TCompare">the type of object being compared</typeparam>
        /// <param name="source">the sequence</param>
        /// <param name="selector">the selector. Given an object of type TSource, return the value you wish to compare</param>
        /// <param name="comparer">the comparer used to compare items returned by the selector</param>
        /// <returns>
        /// The maximum value in the sequence, as defined by the custom selector and comparer.
        /// If there is more than one element that equates to the maximum, returns the first match.
        /// </returns>
        public static TSource Maximum<TSource, TCompare>(this IEnumerable<TSource> source, Func<TSource, TCompare> selector, IComparer<TCompare> comparer)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }
            if (selector == null) { throw new ArgumentNullException(nameof(selector)); }
            if (comparer == null) { throw new ArgumentNullException(nameof(comparer)); }

            return source.Aggregate((o1, o2) =>
            {
                TCompare toCompare1 = selector(o1);
                TCompare toCompare2 = selector(o2);
                return comparer.Compare(toCompare1, toCompare2) >= 0 ? o1 : o2;
            });
        }

        /// <summary>
        /// Returns whether there are at least the provided number of elements in the enumerable
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="minCount"></param>
        /// <returns></returns>
        public static bool AtLeast<TSource>(this IEnumerable<TSource> source, int minCount)
        {
            // Arrays can be implicitly cast to an IReadOnlyCollection. Collection
            // classes normally implement this interface as well.
            if (source is IReadOnlyCollection<TSource> collection)
            {
                return collection.Count >= minCount;
            }

            return AtLeast(source, minCount, o => true);
        }

        /// <summary>
        /// Returns whether there are at least the provided number of elements in the enumerable that match a predicate
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="minCount"></param>
        /// <param name="predicate">The predicate that determines whether an elements matches</param>
        /// <returns></returns>
        public static bool AtLeast<TSource>(this IEnumerable<TSource> source, int minCount, Predicate<TSource> predicate)
        {
            if (minCount <= 0)
            {
                return true;
            }

            // Arrays can be implicitly cast to an IReadOnlyCollection. Collection
            // classes normally implement this interface as well.
            if (source is IReadOnlyCollection<TSource> collection && collection.Count < minCount)
            {
                return false;
            }

            int count = 0;
            foreach (TSource item in source) // Could use .Where, but that introduces some intermediary code, and speed might matter 
            {
                if (predicate(item) && ++count == minCount)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a MultiMap from an enumerable
        /// </summary>
        /// <typeparam name="TSource">The type of enumerated element, as well as the value type for the returned MultiMap</typeparam>
        /// <typeparam name="TKey">The key type for the returned MultiMap</typeparam>
        /// <param name="source">The enumerable</param>
        /// <param name="getKey">Delegate that returns a key given an instance of the enumerated type</param>
        /// <returns>The MultiMap</returns>
        public static MultiMap<TKey, TSource> ToMultiMap<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> getKey) =>
            ToMultiMap(source, getKey, getValue: o => o, initCapacity: 0, keyComparer: null);

        /// <summary>
        /// Creates a MultiMap from an enumerable
        /// </summary>
        /// <typeparam name="TSource">The type of enumerated element</typeparam>
        /// <typeparam name="TKey">The key type for the returned MultiMap</typeparam>
        /// <typeparam name="TValue">The value type for the returned MultiMap</typeparam>
        /// <param name="source">The enumerable</param>
        /// <param name="getKey">Delegate that returns a key given an instance of the enumerated type</param>
        /// <param name="getValue">Delegate that returns a value given an instance of the enumerated typ</param>
        /// <returns>The MultiMap</returns>
        public static MultiMap<TKey, TValue> ToMultiMap<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> getKey, Func<TSource, TValue> getValue) =>
            ToMultiMap(source, getKey, getValue, initCapacity: 0, keyComparer: null);

        /// <summary>
        /// Creates a MultiMap from an enumerable
        /// </summary>
        /// <typeparam name="TSource">The type of enumerated element, as well as the value type for the returned MultiMap</typeparam>
        /// <typeparam name="TKey">The key type for the returned MultiMap</typeparam>
        /// <param name="source">The enumerable</param>
        /// <param name="getKey">Delegate that returns a key given an instance of the enumerated type</param>
        /// <param name="initCapacity">The initial capacity of the MultiMap</param>
        /// <returns>The MultiMap</returns>
        public static MultiMap<TKey, TSource> ToMultiMap<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> getKey, int initCapacity) =>
            ToMultiMap(source, getKey, getValue: o => o, initCapacity: initCapacity, keyComparer: null);

        /// <summary>
        /// Creates a MultiMap from an enumerable
        /// </summary>
        /// <typeparam name="TSource">The type of enumerated element</typeparam>
        /// <typeparam name="TKey">The key type for the returned MultiMap</typeparam>
        /// <typeparam name="TValue">The value type for the returned MultiMap</typeparam>
        /// <param name="source">The enumerable</param>
        /// <param name="getKey">Delegate that returns a key given an instance of the enumerated type</param>
        /// <param name="getValue">Delegate that returns a value given an instance of the enumerated typ</param>
        /// <param name="initCapacity">The initial capacity of the MultiMap</param>
        /// <returns>The MultiMap</returns>
        public static MultiMap<TKey, TValue> ToMultiMap<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> getKey, Func<TSource, TValue> getValue, int initCapacity) =>
            ToMultiMap(source, getKey, getValue, initCapacity, keyComparer: null);

        /// <summary>
        /// Creates a MultiMap from an enumerable
        /// </summary>
        /// <typeparam name="TSource">The type of enumerated element</typeparam>
        /// <typeparam name="TKey">The key type for the returned MultiMap</typeparam>
        /// <typeparam name="TValue">The value type for the returned MultiMap</typeparam>
        /// <param name="source">The enumerable</param>
        /// <param name="getKey">Delegate that returns a key given an instance of the enumerated type</param>
        /// <param name="getValue">Delegate that returns a value given an instance of the enumerated typ</param>
        /// <param name="keyComparer">Used to determine if two keys are equal</param>
        /// <returns>The MultiMap</returns>
        public static MultiMap<TKey, TValue> ToMultiMap<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> getKey, Func<TSource, TValue> getValue, IEqualityComparer<TKey> keyComparer) =>
            ToMultiMap(source, getKey, getValue, initCapacity: 0, keyComparer: keyComparer);

        /// <summary>
        /// Creates a MultiMap from an enumerable
        /// </summary>
        /// <typeparam name="TSource">The type of enumerated element</typeparam>
        /// <typeparam name="TKey">The key type for the returned MultiMap</typeparam>
        /// <typeparam name="TValue">The value type for the returned MultiMap</typeparam>
        /// <param name="source">The enumerable</param>
        /// <param name="getKey">Delegate that returns a key given an instance of the enumerated type</param>
        /// <param name="getValue">Delegate that returns a value given an instance of the enumerated typ</param>
        /// <param name="initCapacity">The initial capacity of the MultiMap</param>
        /// <param name="keyComparer">Used to determine if two keys are equal</param>
        /// <returns>The MultiMap</returns>
        public static MultiMap<TKey, TValue> ToMultiMap<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> getKey, Func<TSource, TValue> getValue, int initCapacity, IEqualityComparer<TKey> keyComparer)
        {
            var result = new MultiMap<TKey, TValue>(initCapacity, keyComparer);
            AddToMultiMap(source, getKey, getValue, result);
            return result;
        }

        private static void AddToMultiMap<TSource, TKey, TValue>(IEnumerable<TSource> source, Func<TSource, TKey> getKey, Func<TSource, TValue> getValue, IMultiMap<TKey, TValue> multiMap)
        {
            foreach (TSource item in source)
            {
                multiMap.Add(getKey(item), getValue(item));
            }
        }
    }
}
