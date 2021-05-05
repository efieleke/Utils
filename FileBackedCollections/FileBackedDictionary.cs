using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sayer.Collections;

namespace Sayer.FileBackedCollections
{
    /// <summary>
    /// Represents a dictionary backed by a file. Every change to the dictionary is immediately written to file, and every lookup
    /// is done from file. This class uses very little memory, but of course is slower than a memory-based dictionary.
    /// 
    /// Due to the file-backed nature of this dictionary, modifications to retrieved values will not be reflected in the dictionary
    /// unless they are stored again via the index operator or some other method(s).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class FileBackedDictionary<TKey, TValue> : FileBackedDictionary<TKey, TValue, string>
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
        /// before new items are added. This value is ignored if cacheSize is 0. Note that access during enumeration is not considered as a lookup. 
        /// </param>
        public FileBackedDictionary(string fileName, FileMode mode, int capacity, IReadWrite<TKey> keyIO, IReadWrite<TValue> valueIO, uint cacheSize = 0, TimeSpan? cacheExpiration = null) :
            base(fileName, mode, capacity, keyIO, valueIO, new StringIO(), cacheSize, cacheExpiration)
        {
        }
    }

    /// <summary>
    /// Represents a dictionary backed by a file. Every change to the dictionary is immediately written to file, and every lookup
    /// is done from file. This class uses very little memory, but of course is slower than a memory-based dictionary.
    /// 
    /// Due to the file-backed nature of this dictionary, modifications to retrieved values will not be reflected in the dictionary
    /// unless they are stored again via the index operator or some other method(s).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TMetaData"></typeparam>
    public class FileBackedDictionary<TKey, TValue, TMetaData> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDisposable
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
        /// before new items are added. This value is ignored if cacheSize is 0. Note that access during enumeration is not considered as a lookup. 
        /// </param>
        public FileBackedDictionary(
            string fileName,
            FileMode mode,
            int capacity,
            IReadWrite<TKey> keyIO,
            IReadWrite<TValue> valueIO,
            IReadWrite<TMetaData> metaDataIO,
            uint cacheSize = 0,
            TimeSpan? cacheExpiration = null)
        {
            FileName = fileName;
            _stream = new FileStream(fileName, mode, FileAccess.ReadWrite, FileShare.None, BUFFER_SIZE, FileOptions.RandomAccess);
            _keyIO = keyIO;
            _valueIO = valueIO;
            _metaDataIO = metaDataIO;

            if (_stream.Length == 0)
            {
                try
                {
                    using (BinaryWriter writer = GetWriter())
                    {
                        long bucketCount = Primes.GetNextPrime(capacity * 2L); // Two times as many buckets as normal to avoid collisions.
                        writer.Write(VERSION);
                        writer.Write(0L); // space holder for metadata
                        writer.Write(Count);
                        writer.Write(bucketCount);

                        for (int i = 0; i < bucketCount; ++i)
                        {
                            writer.Write(0L);
                        }
                    }
                }
                catch (Exception)
                {
                    _stream.Dispose();
                    File.Delete(fileName);
                    throw;
                }
            }
            else
            {
                try
                {
                    using (BinaryReader reader = GetReader())
                    {
                        _stream.Seek(0L, SeekOrigin.Begin);
                        int version = reader.ReadInt32();

                        if (version != VERSION)
                        {
                            throw new Exception($"Unexpected version {version} for FileBackedDictionary");
                        }

                        _stream.Seek(sizeof(long), SeekOrigin.Current); // skip metadata
                        Count = reader.ReadInt32();
                    }
                }
                catch (Exception)
                {
                    _stream.Dispose();
                    throw;
                }
            }

            if (cacheSize != 0)
            {
                _cache = new Cache<TKey, TValue>(cacheSize, cacheExpiration);
            }

            _memoryStream = new MemoryStream();
        }

        /// <summary>
        /// The path of the backing file
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Saves metadata for this FileBackedDictionary
        /// </summary>
        /// <param name="metaData">The metadata to save. null is a permissible value.</param>
        public void SaveMetaData(TMetaData metaData)
        {
            using (BinaryWriter writer = GetWriter())
            {
                long position = 0;

                if (metaData != null)
                {
                    position = _stream.Seek(0L, SeekOrigin.End);
                    _metaDataIO.Write(metaData, writer);
                }

                _stream.Seek(sizeof(int), SeekOrigin.Begin);
                writer.Write(position);
            }
        }

        /// <summary>
        /// Reads metadata for this FileBackedDictionary.
        /// </summary>
        /// <returns>The metadata, or default(TMetaData) if no metadata was ever saved</returns>
        public TMetaData LoadMetaData()
        {
            using (BinaryReader reader = GetReader())
            {
                _stream.Seek(sizeof(int), SeekOrigin.Begin);
                long offset = reader.ReadInt64();

                if (offset == 0)
                {
                    return default;
                }

                _stream.Seek(offset, SeekOrigin.Begin);
                return _metaDataIO.Read(reader);
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
            if (!TryGetValue(key, out TValue value))
            {
                value = getter(key);
                Add(key, value);
            }

            return value;
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
            if (!TryGetValue(key, out TValue value))
            {
                value = await getter(key);
                Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// Returns the maximum optimal capacity for this dictionary. If the current count is greater than this
        /// number, consider calling Rebuild to improve performance.
        /// </summary>
        public int GetCapacity()
        {
            using (BinaryReader reader = GetReader())
            {
                _stream.Seek(sizeof(int) * 2 + sizeof(long), SeekOrigin.Begin);
                // Number of buckets divided by 2 represents max optimal capacity. This is a generous number of buckets,
                // because collisions means more file seeks and reads.
                return (int)(reader.ReadInt64() / 2);
            }
        }

        /// <summary>
        /// Completely rewrites and compacts the dictionary. Removed items lead to dead space in the file.
        /// This method also ensures capacity. It is very expensive, so it should only be called deliberately.
        /// </summary>
        /// <param name="capacity">The capacity of the rebuilt dictionary. If this value is less than Count, it will be set to the count.</param>
        public void Rebuild(int capacity)
        {
            capacity = Math.Max(capacity, Count);
            string tempFileName = Path.GetTempFileName();

            try
            {
                using (var replacement = new FileBackedDictionary<TKey, TValue, TMetaData>(tempFileName, FileMode.Create, capacity, _keyIO, _valueIO, _metaDataIO))
                {
                    replacement.SaveMetaData(LoadMetaData());

                    foreach (KeyValuePair<TKey, TValue> entry in this)
                    {
                        replacement.Add(entry);
                    }
                }

                _stream.Dispose();
                File.Delete(FileName);
            }
            catch (Exception)
            {
                File.Delete(tempFileName);
                throw;
            }

            File.Move(tempFileName, FileName);
            _stream = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None, BUFFER_SIZE, FileOptions.RandomAccess);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _stream.Dispose();
            _memoryStream.Dispose();
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            using (BinaryReader reader = GetReader())
            {
                _stream.Seek(sizeof(int) * 2 + sizeof(long), SeekOrigin.Begin);
                long numBuckets = reader.ReadInt64();

                using (var bucketsEnumerator = new BucketEnumerator(reader, numBuckets))
                {
                    while (bucketsEnumerator.MoveNext())
                    {
                        long nodeOffset = bucketsEnumerator.Current;

                        while (nodeOffset != 0L)
                        {
                            _stream.Seek(nodeOffset, SeekOrigin.Begin);
                            TKey key = _keyIO.Read(reader);
                            nodeOffset = reader.ReadInt64();
                            TValue value = _valueIO.Read(reader);
                            yield return new KeyValuePair<TKey, TValue>(key, value);
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        /// <inheritdoc />
        public void Clear()
        {
            TMetaData metaData = LoadMetaData();

            using (BinaryReader reader = GetReader())
            using (BinaryWriter writer = GetWriter())
            {
                _stream.Seek(sizeof(int) * 2 + sizeof(long), SeekOrigin.Begin);
                long bucketCount = reader.ReadInt64();
                _stream.Seek(sizeof(int) + sizeof(long), SeekOrigin.Begin);
                Count = 0;
                writer.Write(Count);
                writer.Write(bucketCount); // not necessary, but doesn't hurt

                for (int i = 0; i < bucketCount; ++i)
                {
                    writer.Write(0L);
                }

                _stream.SetLength(_stream.Position);
                _cache?.Clear();
            }

            SaveMetaData(metaData);
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item) => TryGetValue(item.Key, out TValue value) && Equals(value, item.Value);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (Count > array.Length - arrayIndex)
            {
                throw new ArgumentException("Input destination not large enough");
            }

            foreach (KeyValuePair<TKey, TValue> entry in this)
            {
                array[arrayIndex++] = entry;
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key, item.Value, valueMustMatch: true);

        public int Count { get; private set; }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
            if (_cache != null && _cache.ContainsKey(key))
            {
                return true;
            }

            using (BinaryReader reader = GetReader())
            {
                _stream.Seek(GetBucketOffset(key, reader), SeekOrigin.Begin);
                long keyOffset = reader.ReadInt64();

                while (keyOffset != 0L)
                {
                    _stream.Seek(keyOffset, SeekOrigin.Begin);

                    if (key.Equals(_keyIO.Read(reader)))
                    {
                        return true;
                    }

                    keyOffset = reader.ReadInt64();
                }

                return false;
            }
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value) => Insert(key, value, failIfExists: true);

        /// <inheritdoc />
        public bool Remove(TKey key) => Remove(key, default, valueMustMatch: false);

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_cache != null && _cache.TryGetValue(key, out value))
            {
                return true;
            }

            using (BinaryReader reader = GetReader())
            {
                _stream.Seek(GetBucketOffset(key, reader), SeekOrigin.Begin);
                long keyOffset = reader.ReadInt64();

                while (keyOffset != 0L)
                {
                    _stream.Seek(keyOffset, SeekOrigin.Begin);

                    if (key.Equals(_keyIO.Read(reader)))
                    {
                        _stream.Seek(sizeof(long), SeekOrigin.Current);
                        value = _valueIO.Read(reader);
                        _cache?.Add(key, value);
                        return true;
                    }

                    keyOffset = reader.ReadInt64();
                }

                value = default;
                return false;
            }
        }

        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out TValue value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }

            set => Insert(key, value, failIfExists: false);
        }

        /// <inheritdoc />
        [DebuggerDisplay("Count = {Count}")]
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        /// <inheritdoc />
        [DebuggerDisplay("Count = {Count}")]
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
        /// <inheritdoc />
        [DebuggerDisplay("Count = {Count}")]
        public ICollection<TKey> Keys
        {
            get
            {
                var result = new List<TKey>(Count);
                result.AddRange(GetKeysEnumerable());
                return result;
            }
        }

        /// <inheritdoc />
        [DebuggerDisplay("Count = {Count}")]
        public ICollection<TValue> Values
        {
            get
            {
                var result = new List<TValue>(Count);
                result.AddRange(this.Select(o => o.Value));
                return result;
            }
        }

        /// <summary>
        /// Enumerates all the keys in this collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TKey> GetKeysEnumerable()
        {
            using (BinaryReader reader = GetReader())
            {
                _stream.Seek(sizeof(int) * 2 + sizeof(long), SeekOrigin.Begin);
                long numBuckets = reader.ReadInt64();

                using (var bucketsEnumerator = new BucketEnumerator(reader, numBuckets))
                {
                    while (bucketsEnumerator.MoveNext())
                    {
                        long nodeOffset = bucketsEnumerator.Current;

                        while (nodeOffset != 0L)
                        {
                            _stream.Seek(nodeOffset, SeekOrigin.Begin);
                            TKey key = _keyIO.Read(reader);
                            nodeOffset = reader.ReadInt64();
                            yield return key;
                        }
                    }
                }
            }
        }

        private bool Remove(TKey key, TValue value, bool valueMustMatch)
        {
            using (BinaryReader reader = GetReader())
            using (BinaryWriter writer = GetWriter())
            {
                long previousNode = GetBucketOffset(key, reader);
                _stream.Seek(previousNode, SeekOrigin.Begin);
                long keyOffset = reader.ReadInt64();

                while (keyOffset != 0L)
                {
                    _stream.Seek(keyOffset, SeekOrigin.Begin);

                    if (key.Equals(_keyIO.Read(reader)))
                    {
                        keyOffset = reader.ReadInt64();
                        bool matches = true;

                        if (valueMustMatch)
                        {
                            matches = Equals(value, _valueIO.Read(reader));
                        }

                        if (matches)
                        {
                            _stream.Seek(previousNode, SeekOrigin.Begin);
                            writer.Write(keyOffset);
                            AdjustCount(-1, writer);
                            _cache?.Remove(key);
                            return true;
                        }

                        return false;
                    }

                    previousNode = _stream.Position;
                    keyOffset = reader.ReadInt64();
                }

                return false;
            }
        }

        private void Insert(TKey key, TValue value, bool failIfExists)
        {
            using (BinaryReader reader = GetReader())
            using (BinaryWriter writer = GetWriter())
            {
                long previousNode = GetBucketOffset(key, reader);
                _stream.Seek(previousNode, SeekOrigin.Begin);
                long keyOffset = reader.ReadInt64();

                while (keyOffset != 0)
                {
                    _stream.Seek(keyOffset, SeekOrigin.Begin);

                    if (key.Equals(_keyIO.Read(reader)))
                    {
                        if (failIfExists)
                        {
                            throw new ArgumentException($"Key {key} already exists in dictionary", nameof(key));
                        }

                        _stream.Seek(sizeof(long), SeekOrigin.Current);
                        long valueStartPos = _stream.Position;
                        _valueIO.Read(reader); // will result in moving the file pointer
                        long bytesUsedForExistingValue = _stream.Position - valueStartPos;

                        using (var memStreamWriter = new BinaryWriter(_memoryStream, _encoding, leaveOpen: true))
                        {
                            _memoryStream.SetLength(0);
                            _valueIO.Write(value, memStreamWriter);

                            if (_memoryStream.Length > bytesUsedForExistingValue)
                            {
                                // Not enough room at the existing file location. This will leave dead space in
                                // the file, which can be reclaimed via the Rebuild() method.
                                Remove(key);
                                Insert(key, value, failIfExists: true);
                            }
                            else
                            {
                                _stream.Seek(valueStartPos, SeekOrigin.Begin);
                                writer.Write(_memoryStream.GetBuffer(), 0, (int)_memoryStream.Length);

                                if (_cache != null)
                                {
                                    _cache[key] = value;
                                }
                            }
                        }

                        return;
                    }

                    previousNode = _stream.Position;
                    keyOffset = reader.ReadInt64();
                }

                keyOffset = _stream.Seek(0L, SeekOrigin.End);
                _keyIO.Write(key, writer);
                writer.Write(0L);
                _valueIO.Write(value, writer);
                _stream.Seek(previousNode, SeekOrigin.Begin);
                writer.Write(keyOffset);

                AdjustCount(1, writer);
                _cache?.Add(key, value);
            }
        }

        private BinaryReader GetReader() => new BinaryReader(_stream, _encoding, leaveOpen: true);
        private BinaryWriter GetWriter() => new BinaryWriter(_stream, _encoding, leaveOpen: true);

        private void AdjustCount(int increment, BinaryWriter writer)
        {
            Count += increment;
            _stream.Seek(sizeof(int) + sizeof(long), SeekOrigin.Begin);
            writer.Write(Count);
        }

        private long GetBucketOffset(TKey key, BinaryReader reader)
        {
            _stream.Seek(sizeof(int) * 2 + sizeof(long), SeekOrigin.Begin);
            long bucketCount = reader.ReadInt64();
            long bucketIndex = (key.GetHashCode() & int.MaxValue) % bucketCount;
            return sizeof(int) * 2 + sizeof(long) * 2 + bucketIndex * sizeof(long);
        }

        private class BucketEnumerator : IEnumerator<long>
        {
            internal BucketEnumerator(BinaryReader reader, long count)
            {
                _reader = reader;
                _count = count;
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                if (_currentIndex == _count - 1)
                {
                    return false;
                }

                if (++_currentIndex % _keyOffsetsBuffer.Length == 0)
                {
                    long batchSize = Math.Min(_keyOffsetsBuffer.Length, _count - _currentIndex);
                    _reader.BaseStream.Seek(sizeof(int) * 2 + sizeof(long) * 2 + sizeof(long) * _currentIndex, SeekOrigin.Begin);

                    for (long i = 0; i < batchSize; ++i)
                    {
                        _keyOffsetsBuffer[i] = _reader.ReadInt64();
                    }
                }

                return true;
            }

            public void Reset() => _currentIndex = -1;

            public long Current => _keyOffsetsBuffer[_currentIndex % _keyOffsetsBuffer.Length];

            object IEnumerator.Current => Current;

            private readonly BinaryReader _reader;
            private readonly long _count;
            private readonly long[] _keyOffsetsBuffer = new long[BUFFER_SIZE];
            private long _currentIndex = -1;
        }

        private FileStream _stream;
        private readonly MemoryStream _memoryStream;
        private readonly IReadWrite<TKey> _keyIO;
        private readonly IReadWrite<TValue> _valueIO;
        private readonly IReadWrite<TMetaData> _metaDataIO;
        private readonly Encoding _encoding = new UTF8Encoding(false, true);
        private const int VERSION = 2;
        private const int BUFFER_SIZE = 4096;
        private readonly Cache<TKey, TValue> _cache;
    }
}
