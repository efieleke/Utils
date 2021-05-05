using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sayer.Collections
{
    /// <summary>
    /// Represents a set of keys, where each key is associated with one or more values. Enumerating through
    /// an IReadOnlyMultiMap will return a key/value pair for each element. The same key will be passed to successive
    /// iterations if more than one value is associated with that key.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IReadOnlyMultiMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Returns whether there are any value entries for the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsKey(TKey key);

        /// <summary>
        /// Returns whether a specific value is associated with a specific key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        bool ContainsValue(TKey key, TValue val);

        /// <summary>
        /// The collection of values associated with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IReadOnlyCollection<TValue> this[TKey key] { get; }

        /// <summary>
        /// Returns the collection of values associated with the given key, if there are any
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>true if there are any values associated with the given key, false otherwise</returns>
        bool TryGetValues(TKey key, out IReadOnlyCollection<TValue> values);

        /// <summary>
        /// Returns the key count. In some cases, this may be more performant than calling Keys.Count.
        /// </summary>
        int KeyCount { get; }

        /// <summary>
        /// Returns the collection of keys
        /// </summary>
        IReadOnlyCollection<TKey> Keys { get; }

        /// <summary>
        /// Returns the collection of all values
        /// </summary>
        IReadOnlyCollection<TValue> Values { get; }
    }

    /// <summary>
    /// Represents a set of keys, where each key is associated with one or more values. Enumerating through
    /// an IMultiMap will return a key/value pair for each element. The same key will be passed to successive
    /// iterations if more than one value is associated with that key.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IMultiMap<TKey, TValue> : IReadOnlyMultiMap<TKey, TValue>
    {
        /// <summary>
        /// The collection of values associated with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        new IReadOnlyCollection<TValue> this[TKey key] { get; set; }

        /// <summary>
        /// Adds an element to the multimap. Depending upon the type of underlying value collection,
        /// if the value is equal to another value already associated with the given key, this operation
        /// could be a no-op.
        /// </summary>
        /// <param name="key">The key with which to associate the value</param>
        /// <param name="val">The value</param>
        void Add(TKey key, TValue val);

        /// <summary>
        /// Removes a key (and therefore all of its associated values as well) from the collection.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true, iof the key existed and was removed</returns>
        bool Remove(TKey key);

        /// <summary>
        /// Removes a specific value that is associated with a specific key. Note that if the value
        /// being removed is the only value associated with the key, the key will also be removed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns>true, if the key/value pair existed, and the value was removed.</returns>
        bool Remove(TKey key, TValue val);

        /// <summary>
        /// Empties the collection
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Represents a set of keys, where each key is associated with one or more values. Enumerating through
    /// an IMultiMap will return a key/value pair for each element. The same key will be passed to successive
    /// iterations if more than one value is associated with that key.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TValueCollection">The collection type to use for the values associated with a key</typeparam>
    public class MultiMap<TKey, TValue, TValueCollection> : IMultiMap<TKey, TValue> where TValueCollection : ICollection<TValue>, new()
    {
        public MultiMap() : this(new Dictionary<TKey, TValueCollection>())
        {
        }

        public MultiMap(IEqualityComparer<TKey> keyComparer) : this(new Dictionary<TKey, TValueCollection>(keyComparer))
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">
        /// The number of unique keys this collection can initially hold without having to allocate more memory. Note that the
        /// number of keys is less than or equal to the number of elements, as a multimap can have multiple elements share
        /// the same key.
        /// </param>
        public MultiMap(int capacity) : this(new Dictionary<TKey, TValueCollection>(capacity))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">
        /// The number of unique keys this collection can initially hold without having to allocate more memory. Note that the
        /// number of keys is less than or equal to the number of elements, as a multimap can have multiple elements share
        /// the same key.
        /// </param>
        /// <param name="keyComparer" />
        public MultiMap(int capacity, IEqualityComparer<TKey> keyComparer) : this(new Dictionary<TKey, TValueCollection>(capacity, keyComparer))
        {
        }

        private MultiMap(IDictionary<TKey, TValueCollection> dictionary)
        {
            _dict = dictionary;
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue val)
        {
            if (_dict.TryGetValue(key, out TValueCollection values))
            {
                values.Add(val);
            }
            else
            {
                values = new TValueCollection { val };
                _dict.Add(key, values);
            }
        }

        /// <inheritdoc />
        public bool Remove(TKey key) => _dict.Remove(key);

        /// <inheritdoc />
        public bool Remove(TKey key, TValue value)
        {
            if (_dict.TryGetValue(key, out TValueCollection values) && values.Remove(value))
            {
                if (((ICollection<TValue>)values).Count == 0)
                {
                    _dict.Remove(key);
                }

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void Clear() => _dict.Clear();

        /// <inheritdoc />
        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

        /// <inheritdoc />
        public bool ContainsValue(TKey key, TValue val) => _dict.TryGetValue(key, out TValueCollection values) && values.Contains(val);

        /// <inheritdoc />
        public IReadOnlyCollection<TValue> this[TKey i]
        {
            get => _dict[i].AsReadOnly();

            set
            {
                var collection = new TValueCollection();

                foreach (TValue element in value)
                {
                    collection.Add(element);
                }

                _dict[i] = collection;
            }
        }

        /// <inheritdoc />
        public bool TryGetValues(TKey key, out IReadOnlyCollection<TValue> values)
        {
            if (_dict.TryGetValue(key, out TValueCollection collection))
            {
                values = collection.AsReadOnly();

                return true;
            }

            values = null;
            return false;
        }

        /// <inheritdoc />
        public int KeyCount => _dict.Count;

        /// <inheritdoc />
        public IReadOnlyCollection<TKey> Keys => _dict.Keys.AsReadOnly();  // In a later version of framework, KeyCollection implements IReadOnlyCollection

        /// <inheritdoc />
        public IReadOnlyCollection<TValue> Values => new ValuesCollection(_dict.AsReadOnly());

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (KeyValuePair<TKey, TValueCollection> entry in _dict)
            {
                foreach (TValue value in entry.Value)
                {
                    yield return new KeyValuePair<TKey, TValue>(entry.Key, value);
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class ValuesCollection : IReadOnlyCollection<TValue>
        {
            internal ValuesCollection(IReadOnlyDictionary<TKey, TValueCollection> dict)
            {
                _dict = dict;
            }

            public IEnumerator<TValue> GetEnumerator() =>
                _dict.Values.SelectMany(valueCollection => valueCollection).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => _dict.Values.Sum(valueList => valueList.Count);

            private readonly IReadOnlyDictionary<TKey, TValueCollection> _dict;
        }

        private readonly IDictionary<TKey, TValueCollection> _dict;
    }

    /// <summary>
    /// A MultiMap where the collection of values associated with each key is a List
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class MultiMap<TKey, TValue> : MultiMap<TKey, TValue, List<TValue>>
    {
        public MultiMap()
        {
        }

        public MultiMap(int capacity) : base(capacity)
        {
        }

        public MultiMap(IEqualityComparer<TKey> keyComparer) : base(keyComparer)
        {
        }

        public MultiMap(int capacity, IEqualityComparer<TKey> keyComparer) : base(capacity, keyComparer)
        {
        }
    }
}
