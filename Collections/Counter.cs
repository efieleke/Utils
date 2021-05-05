using System;
using System.Collections;
using System.Collections.Generic;

namespace Sayer.Collections
{
    /// <summary>
    /// Tracks counts of items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Counter<T> : IReadOnlyCollection<KeyValuePair<T, int>>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Counter() : this(null, 0, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">Used to determine if items are equal</param>
        public Counter(IEqualityComparer<T> comparer) : this(comparer, 0, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">The initial capacity of the collection</param>
        public Counter(int capacity) : this(null, capacity, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="values">The items for which to increment the count by 1</param>
        public Counter(IEnumerable<T> values) : this(null, 0, values)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">Used to determine if items are equal</param>
        /// <param name="values">Items to add to the collection. Ignored if null.</param>
        public Counter(IEqualityComparer<T> comparer, IEnumerable<T> values) : this(comparer, 0, values)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">Used to determine if items are equal</param>
        /// <param name="capacity"> The initial capacity of the collection</param>
        /// <param name="values">Items to add to the collection. Ignored if null.</param>
        public Counter(IEqualityComparer<T> comparer, int capacity, IEnumerable<T> values)
        {
            _dictionary = new Dictionary<T, int>(capacity, comparer);

            if (values != null)
            {
                Increment(values);
            }
        }

        /// <summary>
        /// Increments the count associated with an item by 1
        /// </summary>
        /// <param name="key">The item for which to increment the count</param>
        public void Increment(T key) => Increment(key, 1);

        /// <summary>
        /// Increments the count associated with each item by 1
        /// </summary>
        /// <param name="values">The items for which to increment the count</param>
        public void Increment(IEnumerable<T> values)
        {
            foreach (T value in values)
            {
                Increment(value, 1);
            }
        }

        /// <summary>
        /// Increments the count associated with an item
        /// </summary>
        /// <param name="key">The item for which to increment the count</param>
        /// <param name="increment">The amount to increment by. Must be greater than zero.</param>
        public void Increment(T key, int increment)
        {
            if (increment < 1)
            {
                throw new ArgumentException("Increment value must be > 0", nameof(increment));
            }

            _dictionary.TryGetValue(key, out int count);
            _dictionary[key] = count + increment;
        }

        public void Clear() => _dictionary.Clear();

        /// <summary>
        /// Returns the count of the specified key, which will be equal to the number of times Increment was called for the key.
        /// This will return 0 if Increment was never called for the key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int this[T key] => CountOf(key);

        /// <summary>
        /// Returns the count of the specified key, which will be equal to the number of times Increment was called for the key.
        /// This will return 0 if Increment was never called for the key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int CountOf(T key)
        {
            _dictionary.TryGetValue(key, out int count);
            return count;
        }

        public IReadOnlyCollection<T> Keys => _dictionary.Keys.AsReadOnly();

        public IReadOnlyCollection<int> Counts => _dictionary.Values.AsReadOnly();

        /// <summary>
        /// Returns the keys and their counts as a read-only dictionary. The returned dictionary is a view into this object, so if Increment() is called,
        /// a previously returned dictionary will reflect the change. Also note that the index operator of the returned dictionary will throw an exception
        /// if used to look up a key that was never incremented. This is different behavior than the index operator of Accumulator, which will
        /// return 0 in that case.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<T, int> AsReadOnlyDictionary() => _dictionary;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<T, int>> GetEnumerator() => _dictionary.GetEnumerator();
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => _dictionary.Count;

        private readonly Dictionary<T, int> _dictionary;
    }
}
