using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sayer.FileBackedCollections
{
    /// <summary>
    /// Represents a set backed by a file. Every change to the set is immediately written to file, and every lookup
    /// is done from file. This class uses very little memory, but of course is slower than a memory-based set.
    /// 
    /// Due to the file-backed nature of this set, modifications to retrieved values will not be reflected in the set
    /// unless they are stored again via a remove and an add.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class FileBackedSet<TKey> : FileBackedSet<TKey, string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">The name of the backing file</param>
        /// <param name="mode">The mode in which to create or open the backing file.</param>
        /// <param name="capacity">The initial capacity of the dictionary. This is ignored if the input file has a file size greater than zero.</param>
        /// <param name="keyIO">Defines how the key is written and read from a stream. Implementations for common types exist in this assembly and namespace.</param>
        public FileBackedSet(string fileName, FileMode mode, int capacity, IReadWrite<TKey> keyIO) :
            base(fileName, mode, capacity, keyIO, new StringIO())
        {
        }
    }

    /// <summary>
    /// Represents a set backed by a file. Every change to the set is immediately written to file, and every lookup
    /// is done from file. This class uses very little memory, but of course is slower than a memory-based set.
    /// 
    /// Due to the file-backed nature of this set, modifications to retrieved values will not be reflected in the set
    /// unless they are stored again via a remove and an add.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TMetaData"></typeparam>
    public class FileBackedSet<TKey, TMetaData> : ISet<TKey>, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">The name of the backing file</param>
        /// <param name="mode">The mode in which to create or open the backing file.</param>
        /// <param name="capacity">The initial capacity of the dictionary. This is ignored if the input file has a file size greater than zero.</param>
        /// <param name="keyIO">Defines how the key is written and read from a stream. Implementations for common types exist in this assembly and namespace.</param>
        /// <param name="metaDataIO">Defines how metadata is written and read from a stream. Implementations for common types exist in this assembly and namespace.</param>
        public FileBackedSet(
            string fileName,
            FileMode mode,
            int capacity,
            IReadWrite<TKey> keyIO,
            IReadWrite<TMetaData> metaDataIO)
        {
            FileName = fileName;
            _stream = new FileStream(fileName, mode, FileAccess.ReadWrite, FileShare.None, BUFFER_SIZE, FileOptions.RandomAccess);
            _keyIO = keyIO;
            _metaDataIO = metaDataIO;

            if (_stream.Length == 0)
            {
                try
                {
                    using (BinaryWriter writer = GetWriter())
                    {
                        long bucketCount = Primes.GetNextPrime((long)capacity * 2L); // Two times as many buckets as elements to avoid collisions.
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
                            throw new Exception($"Unexpected version {version} for FileBackedSet");
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
        }

        /// <summary>
        /// The path of the backing file
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Saves metadata for this FileBackedList
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
        /// Reads metadata for this FileBackedList.
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
                using (var replacement = new FileBackedSet<TKey, TMetaData>(tempFileName, FileMode.Create, capacity, _keyIO, _metaDataIO))
                {
                    replacement.SaveMetaData(LoadMetaData());

                    foreach (TKey entry in this)
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
        public void Dispose() => _stream.Dispose();

        /// <inheritdoc />
        public IEnumerator<TKey> GetEnumerator()
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

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public bool Add(TKey key) => AddImpl(key);

        /// <inheritdoc />
        public void UnionWith(IEnumerable<TKey> other)
        {
            foreach (TKey obj in other)
            {
                AddImpl(obj);
            }
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<TKey> other)
        {
            if (!ReferenceEquals(other, this))
            {
                if (!(other is ISet<TKey> otherAsSet))
                {
                    otherAsSet = new HashSet<TKey>();

                    foreach (TKey item in other)
                    {
                        otherAsSet.Add(item);
                    }
                }

                string tempFileName = Path.GetTempFileName();

                using (var replacement = new FileBackedSet<TKey, TMetaData>(tempFileName, FileMode.Create, GetCapacity(), _keyIO, _metaDataIO))
                {
                    replacement.SaveMetaData(LoadMetaData());

                    foreach (TKey entry in this)
                    {
                        if (otherAsSet.Contains(entry))
                        {
                            replacement.Add(entry);
                        }
                    }
                }

                _stream.Dispose();
                File.Delete(FileName);
                File.Move(tempFileName, FileName);
                _stream = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None, BUFFER_SIZE, FileOptions.RandomAccess);
            }
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<TKey> other)
        {
            if (Count == 0)
            {
                return;
            }

            if (ReferenceEquals(other, this))
            {
                Clear();
            }
            else
            {
                foreach (TKey obj in other)
                {
                    Remove(obj);
                }
            }
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<TKey> other)
        {
            if (!ReferenceEquals(other, this))
            {
                if (!(other is ISet<TKey> otherAsSet))
                {
                    otherAsSet = new HashSet<TKey>();

                    foreach (TKey item in other)
                    {
                        otherAsSet.Add(item);
                    }
                }

                string tempFileName = Path.GetTempFileName();

                using (var replacement = new FileBackedSet<TKey, TMetaData>(tempFileName, FileMode.Create, GetCapacity(), _keyIO, _metaDataIO))
                {
                    replacement.SaveMetaData(LoadMetaData());

                    foreach (TKey entry in this)
                    {
                        if (!otherAsSet.Contains(entry))
                        {
                            replacement.Add(entry);
                        }
                    }

                    foreach (TKey entry in otherAsSet)
                    {
                        if (!Contains(entry))
                        {
                            replacement.Add(entry);
                        }
                    }
                }

                _stream.Dispose();
                File.Delete(FileName);
                File.Move(tempFileName, FileName);
                _stream = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None, BUFFER_SIZE, FileOptions.RandomAccess);
            }
        }


        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<TKey> other) => IsSubsetOf(other, false);

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<TKey> other) => IsSupersetOf(other, false);

        private bool IsSupersetOf(IEnumerable<TKey> other, bool proper)
        {
            if (other is IReadOnlyCollection<TKey> coll && coll.Count > Count - (proper ? 1 : 0))
            {
                return false;
            }

            return ReferenceEquals(other, this) || other.All(Contains);
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<TKey> other) => IsSupersetOf(other, true);

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<TKey> other) => IsSubsetOf(other, true);

        private bool IsSubsetOf(IEnumerable<TKey> other, bool proper)
        {
            if (ReferenceEquals(other, this))
            {
                return true;
            }

            if (other is IReadOnlyCollection<TKey> coll && coll.Count < Count + (proper ? 1 : 0))
            {
                return false;
            }

            if (!(other is ISet<TKey> otherAsSet))
            {
                otherAsSet = new HashSet<TKey>();

                foreach (TKey item in other)
                {
                    otherAsSet.Add(item);
                }
            }

            return Count <= otherAsSet.Count && this.All(k => otherAsSet.Contains(k));
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<TKey> other) => Count != 0 && other.Any(Contains);

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<TKey> other)
        {
            if (other is IReadOnlyCollection<TKey> coll && coll.Count != Count)
            {
                return false;
            }

            int count = 0;

            foreach (TKey item in other)
            {
                ++count;

                if (!Contains(item))
                {
                    return false;
                }
            }

            return Count == count;
        }

        void ICollection<TKey>.Add(TKey item) => AddImpl(item);

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
            }

            SaveMetaData(metaData);
        }

        /// <inheritdoc />
        public bool Contains(TKey key)
        {
            if (key == null)
            {
                return false;
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
        public void CopyTo(TKey[] array, int arrayIndex)
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

            foreach (TKey entry in this)
            {
                array[arrayIndex++] = entry;
            }
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            if (key == null)
            {
                return false;
            }

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
                        _stream.Seek(previousNode, SeekOrigin.Begin);
                        writer.Write(keyOffset);
                        AdjustCount(-1, writer);
                        return true;
                    }

                    previousNode = _stream.Position;
                    keyOffset = reader.ReadInt64();
                }

                return false;
            }
        }

        /// <inheritdoc />
        public int Count { get; private set; }

        /// <inheritdoc />
        public bool IsReadOnly => false;

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

        private bool AddImpl(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

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
                        return false;
                    }

                    previousNode = _stream.Position;
                    keyOffset = reader.ReadInt64();
                }

                keyOffset = _stream.Seek(0L, SeekOrigin.End);
                _keyIO.Write(key, writer);
                writer.Write(0L);
                _stream.Seek(previousNode, SeekOrigin.Begin);
                writer.Write(keyOffset);

                AdjustCount(1, writer);
                return true;
            }
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

                    for (int i = 0; i < batchSize; ++i)
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
            private readonly long[] _keyOffsetsBuffer = new long[BUFFER_SIZE / sizeof(long)];
            private int _currentIndex = -1;
        }

        private FileStream _stream;
        private readonly IReadWrite<TKey> _keyIO;
        private readonly IReadWrite<TMetaData> _metaDataIO;
        private readonly Encoding _encoding = new UTF8Encoding(false, true);
        private const int VERSION = 2;
        private const int BUFFER_SIZE = 4096;
    }
}
