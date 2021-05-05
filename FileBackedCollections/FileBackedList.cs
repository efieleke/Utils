using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sayer.FileBackedCollections
{
    /// <summary>
    /// Represents a list backed by a file. Every change to the list is immediately written to file, and every lookup
    /// is done from file. This class uses very little memory, but of course is slower than a memory-based list.
    /// 
    /// Due to the file-backed nature of this list, modifications to retrieved values will not be reflected in the list
    /// unless they are stored again via the index operator or some other method(s).
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class FileBackedList<TValue> : FileBackedList<TValue, string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">The name of the backing file</param>
        /// <param name="mode">The mode in which to create or open the backing file.</param>
        /// <param name="capacity">The initial capacity of the list. This is ignored if the input file has a file size greater than zero.</param>
        /// <param name="valueIO">
        /// Defines how the value is written and read from a stream. Implementations for common types exist in this assembly and namespace.
        /// </param>
        public FileBackedList(string fileName, FileMode mode, int capacity, IReadWrite<TValue> valueIO) : base(fileName, mode, capacity, valueIO, new StringIO())
        {
        }
    }

    /// <summary>
    /// Represents a list backed by a file. Every change to the list is immediately written to file, and every lookup
    /// is done from file. This class uses very little memory, but of course is slower than a memory-based list.
    /// 
    /// Due to the file-backed nature of this list, modifications to retrieved values will not be reflected in the list
    /// unless they are stored again via the index operator or some other method(s).
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TMetaData"></typeparam>
    public class FileBackedList<TValue, TMetaData> : IList<TValue>, IReadOnlyList<TValue>, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">The name of the backing file</param>
        /// <param name="mode">The mode in which to create or open the backing file.</param>
        /// <param name="capacity">The initial capacity of the list. This is ignored if the input file has a file size greater than zero.</param>
        /// <param name="valueIO">
        /// Defines how the value is written and read from a stream. Implementations for common types exist in this assembly and namespace.
        /// </param>
        /// <param name="metaDataIO">Defines how metadata is written and read from a stream. Implementations for common types exist in this assembly and namespace.</param>
        public FileBackedList(
            string fileName,
            FileMode mode,
            int capacity,
            IReadWrite<TValue> valueIO,
            IReadWrite<TMetaData> metaDataIO)
        {
            FileName = fileName;
            _valueIO = valueIO;
            _metaDataIO = metaDataIO;
            _stream = new FileStream(fileName, mode, FileAccess.ReadWrite, FileShare.None, BUFFER_SIZE, FileOptions.RandomAccess);

            if (_stream.Length == 0)
            {
                try
                {
                    if (capacity < 1)
                    {
                        throw new ArgumentException("Capacity must be > 0", nameof(capacity));
                    }

                    _capacity = capacity;

                    using (BinaryWriter writer = GetWriter())
                    {
                        writer.Write(VERSION);
                        writer.Write(0L); // space holder for metadata
                        writer.Write(Count);
                        writer.Write(capacity);
                        _stream.SetLength(sizeof(int) * 3 + sizeof(long) + sizeof(long) * capacity);
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
                            throw new Exception($"Unexpected version {version} for FileBackedList");
                        }

                        _stream.Seek(sizeof(long), SeekOrigin.Current); // skip metadata
                        Count = reader.ReadInt32();
                        _capacity = reader.ReadInt32();
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
        /// If true, the underlying file will be deleted when this instance is disposed.
        /// </summary>
        public bool DeleteFileUponDisposal { get; set; }

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
        /// Completely rewrites and compacts the list. Removed items lead to dead space in the file.
        /// This method also ensures capacity. It is very expensive, so it should only be called deliberately.
        /// </summary>
        /// <param name="capacity">The capacity of the rebuilt list. If this value is less than Count, it will be set to the count.</param>
        public void Rebuild(int capacity)
        {
            capacity = Math.Max(capacity, Count);
            string tempFileName = Path.GetTempFileName();

            try
            {
                using (var replacement = new FileBackedList<TValue, TMetaData>(tempFileName, FileMode.Create, capacity, _valueIO, _metaDataIO))
                {
                    replacement.SaveMetaData(LoadMetaData());

                    foreach (TValue entry in this)
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
            _capacity = capacity;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _stream.Dispose();

            if (DeleteFileUponDisposal)
            {
                File.Delete(FileName);
            }
        }

        /// <inheritdoc />
        public IEnumerator<TValue> GetEnumerator()
        {
            using (BinaryReader reader = GetReader())
            {
                using (var valueOffsetsEnumerator = new ValueOffsetsEnumerator(reader, Count))
                {
                    while (valueOffsetsEnumerator.MoveNext())
                    {
                        _stream.Seek(valueOffsetsEnumerator.Current, SeekOrigin.Begin);
                        yield return _valueIO.Read(reader);
                    }
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public void Clear()
        {
            TMetaData metaData = LoadMetaData();

            using (BinaryWriter writer = GetWriter())
            {
                _stream.Seek(sizeof(int) + sizeof(long), SeekOrigin.Begin);
                Count = 0;
                writer.Write(Count);
                SeekToValueOffsetIndex(_capacity);
                _stream.SetLength(_stream.Position);
            }

            SaveMetaData(metaData);
        }

        /// <inheritdoc />
        public bool Contains(TValue item) => IndexOf(item) != -1;

        /// <inheritdoc />
        public void CopyTo(TValue[] array, int arrayIndex)
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

            foreach (TValue entry in this)
            {
                array[arrayIndex++] = entry;
            }
        }

        /// <inheritdoc />
        public bool Remove(TValue item)
        {
            int match = IndexOf(item);

            if (match == -1)
            {
                return false;
            }

            RemoveAt(match);
            return true;
        }

        /// <inheritdoc cref="IList{T}.Count" />
        public int Count { get; private set; }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(TValue value) => Insert(Count, value);

        /// <inheritdoc />
        public int IndexOf(TValue item)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (Equals(item, this[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <inheritdoc />
        public void Insert(int index, TValue item)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (Count == _capacity)
            {
                Rebuild(Count * 3); // more than the usual doubling, because this is expensive
            }

            using (BinaryReader reader = GetReader())
            using (BinaryWriter writer = GetWriter())
            {
                ShiftValueOffsetsRight(index, reader, writer);
                SeekToValueOffsetIndex(index);
                WriteValue(index, item, writer);
                AdjustCount(1, writer);
            }
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            using (BinaryReader reader = GetReader())
            using (BinaryWriter writer = GetWriter())
            {
                ShiftValueOffsetsLeft(index + 1, reader, writer);
                AdjustCount(-1, writer);
            }
        }

        /// <inheritdoc cref="IList{T}.this[int]" />
        public TValue this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                using (BinaryReader reader = GetReader())
                {
                    SeekToValueOffsetIndex(index);
                    long valueOffset = reader.ReadInt64();
                    _stream.Seek(valueOffset, SeekOrigin.Begin);
                    return _valueIO.Read(reader);
                }
            }

            set
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                using (BinaryWriter writer = GetWriter())
                {
                    WriteValue(index, value, writer);
                }
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

        private void WriteValue(int index, TValue value, BinaryWriter writer)
        {
            long valueOffset = _stream.Seek(0L, SeekOrigin.End);
            _valueIO.Write(value, writer);
            SeekToValueOffsetIndex(index);
            writer.Write(valueOffset);
        }

        private void ShiftValueOffsetsRight(int startingIndex, BinaryReader reader, BinaryWriter writer)
        {
            int count = Count - startingIndex;

            while (count > _valueOffsetsBuffer.Length)
            {
                int offset = count - _valueOffsetsBuffer.Length;
                ShiftValueOffsets(startingIndex + offset, _valueOffsetsBuffer.Length, 1, reader, writer);
                count -= _valueOffsetsBuffer.Length;
            }

            ShiftValueOffsets(startingIndex, count, 1, reader, writer);
        }

        private void ShiftValueOffsetsLeft(int startingIndex, BinaryReader reader, BinaryWriter writer)
        {
            while (startingIndex < Count)
            {
                int batchSize = Math.Min(_valueOffsetsBuffer.Length, Count - startingIndex);
                ShiftValueOffsets(startingIndex, batchSize, -1, reader, writer);
                startingIndex += batchSize;
            }
        }

        private void ShiftValueOffsets(int startingIndex, int count, int shift, BinaryReader reader, BinaryWriter writer)
        {
            if (count == 0 || shift == 0)
            {
                return;
            }

            SeekToValueOffsetIndex(startingIndex);

            for (int i = 0; i < count; ++i)
            {
                _valueOffsetsBuffer[i] = reader.ReadInt64();
            }

            SeekToValueOffsetIndex(startingIndex + shift);

            for (int i = 0; i < count; ++i)
            {
                writer.Write(_valueOffsetsBuffer[i]);
            }
        }

        private void SeekToValueOffsetIndex(int index) => _stream.Seek(sizeof(int) * 3 + sizeof(long) + sizeof(long) * index, SeekOrigin.Begin);

        private class ValueOffsetsEnumerator : IEnumerator<long>
        {
            internal ValueOffsetsEnumerator(BinaryReader reader, int count)
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

                if (++_currentIndex % _valueOffsetsBuffer.Length == 0)
                {
                    int batchSize = Math.Min(_valueOffsetsBuffer.Length, _count - _currentIndex);
                    _reader.BaseStream.Seek(sizeof(int) * 3 + sizeof(long) + sizeof(long) * _currentIndex, SeekOrigin.Begin);

                    for (int i = 0; i < batchSize; ++i)
                    {
                        _valueOffsetsBuffer[i] = _reader.ReadInt64();
                    }
                }

                return true;
            }

            public void Reset() => _currentIndex = -1;

            public long Current => _valueOffsetsBuffer[_currentIndex % _valueOffsetsBuffer.Length];

            object IEnumerator.Current => Current;

            private readonly BinaryReader _reader;
            private readonly int _count;
            private readonly long[] _valueOffsetsBuffer = new long[BUFFER_SIZE / sizeof(long)];
            private int _currentIndex = -1;
        }

        private FileStream _stream;
        private readonly IReadWrite<TValue> _valueIO;
        private readonly IReadWrite<TMetaData> _metaDataIO;
        private readonly Encoding _encoding = new UTF8Encoding(false, true);
        private const int VERSION = 2;
        private const int BUFFER_SIZE = 4096;
        private int _capacity;

        private readonly long[] _valueOffsetsBuffer = new long[BUFFER_SIZE / sizeof(long)];
    }
}
