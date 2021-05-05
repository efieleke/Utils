using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sayer.Collections
{
    /// <summary>
    /// A Dictionary intended to be used as a cache. It has a maximum size. When a new entry is added, the oldest entry
    /// is removed if the maximum size has been reached. Accessing an entry refreshes it, marking it as the most recent
    /// entry.
    ///
    /// If an expiration period is defined, entries will be expired if not accessed within that period. Expired entries
    /// will be removed before new items are added, or when PruneExpired() is called.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ConcurrentCache<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Grab this lock if performing a succession of operations upon this ConcurrentCache that you need to be atomic.
        /// </summary>
        public object Lock { get; }

        public uint MaxSize => Contents.MaxSize;

        public TimeSpan? Expiration => Contents.Expiration;

        public ConcurrentCache(uint maxSize) : this(maxSize, null)
        {
        }

        public ConcurrentCache(uint maxSize, TimeSpan? expiration)
        {
            Contents = new Cache<TKey, TValue>(maxSize, expiration);
            Lock = new object();
        }

        public void Add(TKey key, TValue value)
        {
            lock (Lock) { Contents.Add(key, value); }
        }

        public bool Remove(TKey key)
        {
            lock (Lock) { return Contents.Remove(key); }
        }

        public void Clear()
        {
            lock (Lock) { Contents.Clear(); }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (Lock) { Contents.CopyTo(array, arrayIndex); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (Lock) { return Contents.Remove(item); }
        }

        public TValue this[TKey key]
        {
            get { lock (Lock) { return Contents[key]; } }
            set { lock (Lock) { Contents[key] = value; } }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public bool ContainsKey(TKey key)
        {
            lock (Lock) { return Contents.ContainsKey(key); }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (Lock) { return Contents.Contains(item); }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (Lock) { return Contents.TryGetValue(key, out value); }
        }

        /// <summary>
        /// Similar to the Keys property, except that this method does not create a copy of the underlying collection.
        /// This ConcurrentCache is locked by this thread until the return value is disposed. That makes enumeration
        /// thread safe. But it is incumbent upon the caller to dispose of the return value (foreach disposes of the
        /// enumerator under the covers).
        /// </summary>
        /// <returns>An enumerable of all the keys in this dictionary</returns>
        public IEnumerable<TKey> GetKeys()
        {
            return GetLockedEnumerable().Select(o => o.Key);
        }

        /// <summary>
        /// Similar to the Values property, except that this method does not create a copy of the underlying collection.
        /// This ConcurrentCache is locked by this thread until the return value is disposed. That makes enumeration
        /// thread safe. But it is incumbent upon the caller to dispose of the return value (foreach disposes of the
        /// enumerator under the covers).
        /// </summary>
        /// <returns>An enumerable of all the values in this dictionary</returns>
        public IEnumerable<TValue> GetValues()
        {
            return GetLockedEnumerable().Select(o => o.Value);
        }

        public ICollection<TKey> Keys
        {
            // This is inefficient because we must make a copy in order to be thread safe.
            get { lock (Lock) { return new List<TKey>(Contents.Keys); } }
        }

        public ICollection<TValue> Values
        {
            // This is inefficient because we must make a copy in order to be thread safe.
            get { lock (Lock) { return new List<TValue>(Contents.Values); } }
        }

        public void Add(KeyValuePair<TKey, TValue> item) { lock (Lock) { Contents.Add(item); } }

        public int Count { get { lock (Lock) { return Contents.Count; } } }

        public bool IsReadOnly => false;

        // Slow. Use GetLockedEnumerator instead, if it's safe to do so.
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            Dictionary<TKey, TValue> copy;

            lock (Lock)
            {
                copy = new Dictionary<TKey, TValue>(Contents);
            }

            return copy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Note that the IEnumerator that the returned Enumerable provides holds a lock on this ConcurrentCache
        // until the enumerator is disposed. That makes enumeration thread safe, and much faster than GetEnumerator()
        // because the underlying dictionary does not need to be copied. But it is incumbent upon the caller to dispose of the
        // enumerator (foreach disposes of the enumerator under the covers).
        //
        // Also be aware that because a lock is held during enumeration, if the processing that occurs
        // for each iteration is expensive, this thread may block other threads for a significant period.
        //
        // Do not use this method if, during enumeration, you wait upon an asynchronous call, and that asynchronous
        // code could also attempt to access this dictionary. That would cause deadlock.
        public IEnumerable<KeyValuePair<TKey, TValue>> GetLockedEnumerable() => new LockedEnumerable(this);

        /// <summary>
        /// Adds the key and its associated value to the dictionary if there is not already a mapping for the given key.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="value">the value</param>
        /// <returns>true if the key was successfully added, false if the dictionary already contained the key</returns>
        public bool TryAdd(TKey key, TValue value)
        {
            lock (Lock) { return Contents.TryAdd(key, value); }
        }

        /// <summary>
        /// Gets an entry for the given key. If no entry for the key exists, it will be created using
        /// the supplied creation routine.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="addRoutine">
        /// A delegate that will be invoked only if there is no value associated with the key. Implementations
        /// should return the value to add.
        /// </param>
        /// <returns>the matching item, which may have been newly created</returns>
        public TValue GetOrAdd(TKey key, Func<TValue> addRoutine)
        {
            lock (Lock) { return Contents.GetOrAdd(key, addRoutine); }
        }

        /// <summary>
        /// Adds an entry or updates an existing entry
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="addRoutine">
        /// A delegate that will be invoked only if there is no value associated with the key. Implementations
        /// should return the value to add.
        /// </param>
        /// <param name="updateRoutine">
        /// A delegate that will be invoked only if there is a value already associated with the key.
        /// The argument to the delegate will be the existing value. Implementations should return the updated
        /// value (which can be the same instance that is passed to the delegate).
        /// </param>
        /// <returns>the newly added or updated item</returns>
        public TValue AddOrUpdate(TKey key, Func<TValue> addRoutine, Func<TValue, TValue> updateRoutine)
        {
            lock (Lock) { return Contents.AddOrUpdate(key, addRoutine, updateRoutine); }
        }

        /// <summary>
        /// Accesses a value associated with a key in a thread-safe manner. This can be used to call methods on the existing value,
        /// and also to return a conversion of the value rather than the original (for example, returning a deep copy
        /// of the value rather than the original value). The converted value is stored in the out parameter 'converted'.
        /// The value associated with the key in this dictionary is *not* replaced with the converted value.
        /// </summary>
        /// <typeparam name="TResult">The type to convert to (for the out parameter)</typeparam>
        /// <param name="key">the key</param>
        /// <param name="accessRoutine">
        /// The access routine. Note that if the sole purpose is to call methods on the existing value,
        /// you can just return null from this routine. An exception will be thrown if the existing value
        /// is returned by accessRoutine, the point being to protect the value from being accessed in
        /// a non-thread-safe manner.
        /// </param>
        /// <param name="converted">If the key exists, this will be set to the result of this routine.</param>
        /// <returns>true if the key exists, false otherwise</returns>
        public bool SafeAccessValue<TResult>(TKey key, Func<TValue, TResult> accessRoutine, out TResult converted)
        {
            lock (Lock)
            {
                if (Contents.TryGetValue(key, out TValue value))
                {
                    converted = accessRoutine(value);

                    if (converted != null && ReferenceEquals(converted, value))
                    {
                        throw new InvalidOperationException("The access routine passed to ConvertValue must return an instance that is different from the value passed as its argument");
                    }

                    return true;
                }

                converted = default;
                return false;
            }
        }

        /// <summary>
        /// If an expiration timespan was defined for this ConcurrentCache, removes all elements that have not been accessed
        /// within that expiration period. This happens automatically before new items are added, but this method can be invoked
        /// directly to force it to happen (for example, if there is a situation where you want to free memory and you know you
        /// will not be adding items to the ConcurrentCache any time soon).
        /// </summary>
        public void PruneExpired()
        {
            lock (Lock) { Contents.PruneExpired(); }
        }

        /// <summary>
        /// This method calls the supplied predicate for each element in this ConcurrentCache. Each time the
        /// predicate returns true, the item supplied to the predicate will be removed from this ConcurrentCache.
        /// If the predicate returns false, the element will not be removed, but iteration will continue.
        /// </summary>
        /// <param name="shouldRemove">Return true to remove the item, false if it should not be removed.</param>
        /// <returns>the number of elements removed</returns>
        public int RemoveIf(Predicate<KeyValuePair<TKey, TValue>> shouldRemove)
        {
            lock (Lock) { return Contents.RemoveIf(shouldRemove); }
        }

        /// <summary>
        /// Refreshes the expiration for the value associated with the given key, if present.
        /// </summary>
        /// <param name="key">The key to refresh</param>
        /// <returns>true, if a value exists for the key</returns>
        public bool Touch(TKey key)
        {
            lock (Lock) { return Contents.Touch(key); }
        }

        private class LockedEnumerable : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            internal LockedEnumerable(ConcurrentCache<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return new LockedEnumerator(_dictionary.Contents, _dictionary.Lock);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private class LockedEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
            {
                internal LockedEnumerator(Cache<TKey, TValue> dictionary, object dictionaryLock)
                {
                    _dictionaryLock = dictionaryLock;
                    System.Threading.Monitor.Enter(_dictionaryLock, ref _lockWasTaken);
                    _enumerator = dictionary.GetEnumerator();
                }

                public void Dispose()
                {
                    _enumerator.Dispose();

                    if (_lockWasTaken)
                    {
                        System.Threading.Monitor.Exit(_dictionaryLock);
                    }
                }

                public bool MoveNext()
                {
                    return _enumerator.MoveNext();
                }

                public void Reset()
                {
                    _enumerator.Reset();
                }

                public KeyValuePair<TKey, TValue> Current => _enumerator.Current;

                object IEnumerator.Current => Current;

                private readonly object _dictionaryLock;
                private readonly bool _lockWasTaken;
                private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;
            }

            private readonly ConcurrentCache<TKey, TValue> _dictionary;
        }

        private Cache<TKey, TValue> Contents { get; }
    }
}
