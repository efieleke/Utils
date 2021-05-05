using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sayer.FileBackedCollections
{
    /// <summary>
    /// Represents a dictionary backed by a file. Every change to the dictionary is immediately written to file, and every lookup
    /// is done from file. This class uses very little memory, is slower than a memory-based dictionary, but much faster
    /// than typical databases.
    /// 
    /// Due to the file-backed nature of this dictionary, modifications to retrieved values will not be reflected in the dictionary
    /// unless they are stored again via the index operator or some other method(s).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ConcurrentFileBackedDictionary<TKey, TValue> : ConcurrentFileBackedDictionary<TKey, TValue, string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">The name of the backing file</param>
        /// <param name="mode">The mode in which to create or open the backing file.</param>
        /// <param name="capacity">The initial capacity of the dictionary. This is ignored if the input file has a file size greater than zero.</param>
        /// <param name="keyIO">Defines how the key is written and read from a stream. Implementations for common types exist in this assembly and namespace.</param>
        /// <param name="valueIO">Defines how the value is written and read from a stream. Implementations for common types exist in this assembly and namespace.</param>
        /// <param name="cacheSize">If non-zero, a memory-based cache will store the most recently looked up or added elements (up to cacheSize in number), for faster lookup.</param>
        /// <param name="cacheExpiration">
        /// If not null, cached entries will be expired if not looked up within this period since they were added or last looked up. Expired entries will be removed
        /// before new items are added. This value is ignored if cacheSize is 0. Note that access during enumeration is not considered a lookup. 
        /// </param>
        public ConcurrentFileBackedDictionary(string fileName, FileMode mode, int capacity, IReadWrite<TKey> keyIO, IReadWrite<TValue> valueIO, uint cacheSize = 0, TimeSpan? cacheExpiration = null) :
            base(fileName, mode, capacity, keyIO, valueIO, new StringIO(), cacheSize, cacheExpiration)
        {
        }
    }

    /// <summary>
    /// Represents a dictionary backed by a file. Every change to the dictionary is immediately written to file, and every lookup
    /// is done from file. This class uses very little memory, is slower than a memory-based dictionary, but much faster
    /// than typical databases.
    /// 
    /// Due to the file-backed nature of this dictionary, modifications to retrieved values will not be reflected in the dictionary
    /// unless they are stored again via the index operator or some other method(s).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TMetaData"></typeparam>
    public class ConcurrentFileBackedDictionary<TKey, TValue, TMetaData> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">The name of the backing file</param>
        /// <param name="mode">The mode in which to create or open the backing file.</param>
        /// <param name="capacity">The initial capacity of the dictionary. This is ignored if the input file has a file size greater than zero.</param>
        /// <param name="keyIO">Defines how the key is written and read from a stream. Implementations for common types exist in this assembly and namespace.</param>
        /// <param name="valueIO">Defines how the value is written and read from a stream. Implementations for common types exist in this assembly and namespace.</param>
        /// <param name="metaDataIO">Defines how metadata is written and read from a stream. Implementations for common types exist in this assembly and namespace.</param>
        /// <param name="cacheSize">If non-zero, a memory-based cache will store the most recently looked up or added elements (up to cacheSize in number), for faster lookup.</param>
        /// <param name="cacheExpiration">
        /// If not null, cached entries will be expired if not looked up within this period since they were added or last looked up. Expired entries will be removed
        /// before new items are added. This value is ignored if cacheSize is 0. Note that access during enumeration is not considered a lookup. 
        /// </param>
        public ConcurrentFileBackedDictionary(
            string fileName,
            FileMode mode,
            int capacity,
            IReadWrite<TKey> keyIO,
            IReadWrite<TValue> valueIO,
            IReadWrite<TMetaData> metaDataIO,
            uint cacheSize = 0,
            TimeSpan? cacheExpiration = null)
        {
            _impl = new FileBackedDictionary<TKey, TValue, TMetaData>(fileName, mode, capacity, keyIO, valueIO, metaDataIO, cacheSize, cacheExpiration);
        }
        
        /// <summary>
        /// The path of the backing file.
        /// </summary>
        public string FileName => _impl.FileName;

        /// <summary>
        /// Saves metadata for this file backed dictionary
        /// </summary>
        /// <param name="metaData">The metadata to save. null is a permissible value.</param>
        public void SaveMetaData(TMetaData metaData)
        {
            lock (Lock)
            {
                _impl.SaveMetaData(metaData);
            }
        }

        /// <summary>
        /// Reads metadata for this file backed dictionary
        /// </summary>
        /// <returns>The metadata, or default(TMetaData) if no metadata was ever saved</returns>
        public TMetaData LoadMetaData()
        {
            lock (Lock)
            {
                return _impl.LoadMetaData();
            }
        }

        /// <summary>
        /// Retrieves a value from this dictionary. If it is not present, calls the provided getter routine
        /// and adds the result to this dictionary before returning it.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="getter"></param>
        /// <returns></returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> getter)
        {
            lock (Lock)
            {
                return _impl.GetOrAdd(key, getter);
            }
        }

        /// <summary>
        /// Retrieves a value from this dictionary. If it is not present, calls the provided getter routine
        /// and adds the result to this dictionary before returning it.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="getter"></param>
        /// <returns></returns>
        public async Task<TValue> GetOrAddAsync(TKey key, Func<TKey, Task<TValue>> getter)
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }

            value = await getter(key).ConfigureAwait(false);
            return GetOrAdd(key, k => value);
        }

        /// <summary>
        /// Returns the maximum optimal capacity for this dictionary. If the current count is greater than this
        /// number, consider calling Rebuild to improve performance.
        /// </summary>
        public int GetCapacity()
        {
            lock (Lock)
            {
                return _impl.GetCapacity();
            }
        }

        /// <summary>
        /// Completely rewrites and compacts the dictionary. Removed items lead to dead space in the file.
        /// This method also ensures capacity. It is very expensive, so it should only be called deliberately.
        /// </summary>
        /// <param name="capacity">The capacity of the rebuilt dictionary. If this value is less than Count, it will be set to the count.</param>
        public void Rebuild(int capacity)
        {
            lock (Lock)
            {
                _impl.Rebuild(capacity);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _impl.Dispose();
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new LockedEnumerator<KeyValuePair<TKey, TValue>>(Lock, () => _impl.GetEnumerator());
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (Lock)
            {
                _impl.Add(item);
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            lock (Lock)
            {
                _impl.Clear();
            }
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (Lock)
            {
                return _impl.Contains(item);
            }
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (Lock)
            {
                _impl.CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (Lock)
            {
                return _impl.Remove(item);
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                lock (Lock)
                {
                    return _impl.Count;
                }
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
            lock (Lock)
            {
                return _impl.ContainsKey(key);
            }
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            lock (Lock)
            {
                _impl.Add(key, value);
            }
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            lock (Lock)
            {
                return _impl.Remove(key);
            }
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (Lock)
            {
                return _impl.TryGetValue(key, out value);
            }
        }

        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get
            {
                lock (Lock)
                {
                    return _impl[key];
                }
            }

            set
            {
                lock (Lock)
                {
                    _impl[key] = value;
                }
            }
        }

        /// <inheritdoc />
        [DebuggerDisplay("Count = {Count}")]
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        /// <inheritdoc />
        [DebuggerDisplay("Count = {Count}")]
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
        /// <inheritdoc />
        [DebuggerDisplay("Count = {Count}")]
        public ICollection<TKey> Keys => GetKeysEnumerable().ToList();
        /// <inheritdoc />
        [DebuggerDisplay("Count = {Count}")]
        public ICollection<TValue> Values => this.Select(o => o.Value).ToList();

        /// <summary>
        /// Enumerates all the keys in this collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TKey> GetKeysEnumerable()
        {
            using (var enumerator = new LockedEnumerator<TKey>(Lock, () => _impl.GetKeysEnumerable().GetEnumerator()))
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        /// <summary>
        /// The recursive lock used internally to keep this instance thread-safe. If performing macro
        /// operations upon this instance that need to be atomic, you can take hold of this lock.
        /// </summary>
        public object Lock { get; } = new object();

        private class LockedEnumerator<T> : IEnumerator<T>
        {
            internal LockedEnumerator(object lockObj, Func<IEnumerator<T>> getEnum)
            {
                _lock = lockObj;
                bool lockWasTaken = false;
                System.Threading.Monitor.Enter(_lock, ref lockWasTaken);
                _impl = getEnum();
            }

            public void Dispose()
            {
                _impl.Dispose();
                System.Threading.Monitor.Exit(_lock);
            }

            public bool MoveNext() => _impl.MoveNext();
            public void Reset() => _impl.Reset();
            public T Current => _impl.Current;
            object IEnumerator.Current => Current;

            private readonly object _lock;
            private readonly IEnumerator<T> _impl;
        }

        private readonly FileBackedDictionary<TKey, TValue, TMetaData> _impl;
    }
}
