using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sayer.FileBackedCollections
{
    /// <summary>
    /// Represents an append-only collection backed by a file
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class FileBackedAppendOnlyCollection<TValue> : FileBackedAppendOnlyCollection<TValue, bool>
    {
        public FileBackedAppendOnlyCollection(string fileName, FileMode fileMode, IReadWrite<TValue> valueIO) : base(fileName, fileMode, false, valueIO, new BoolIO()) { }
    }

    /// <summary>
    /// Represents an append-only collection backed by a file
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TMetaData"></typeparam>
    public class FileBackedAppendOnlyCollection<TValue, TMetaData> : IReadOnlyCollection<TValue>, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        /// <param name="fileMode"></param>
        /// <param name="metaData">Metadata to include as part of the file</param>
        /// <param name="valueIO"> Defines how the value is written and read from a stream. Implementations for common types exist in this assembly and namespace.</param>
        /// <param name="metaDataIO">Defines how metadata is written and read from a stream. Implementations for common types exist in this assembly and namespace.</param>
        public FileBackedAppendOnlyCollection(
            string fileName,
            FileMode fileMode,
            TMetaData metaData,
            IReadWrite<TValue> valueIO,
            IReadWrite<TMetaData> metaDataIO)
        {
            FileName = fileName;
            _valueIO = valueIO;
            _metaDataIO = metaDataIO;
            _stream = new FileStream(fileName, fileMode, FileAccess.ReadWrite, FileShare.None, BUFFER_SIZE, FileOptions.SequentialScan);

            try
            {
                if (_stream.Length == 0)
                {
                    MetaData = metaData;

                    using (BinaryWriter writer = GetWriter())
                    {
                        writer.Write(VERSION);
                        writer.Write(Count);
                        metaDataIO.Write(metaData, writer);
                    }
                }
                else
                {
                    using (BinaryReader reader = GetReader())
                    {
                        int version = reader.ReadInt32();

                        if (version != VERSION)
                        {
                            throw new Exception($"Unexpected version {version} for FileBackedAppendOnlyCollection");
                        }

                        Count = reader.ReadInt32();
                        MetaData = _metaDataIO.Read(reader);
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

        /// <summary>
        /// Reads metadata for this FileBackedReadOnlyCollection.
        /// </summary>
        /// <returns>The metadata</returns>
        public TMetaData MetaData { get; }

        /// <summary>
        /// The path of the backing file
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// If true, the underlying file will be deleted when this instance is disposed.
        /// </summary>
        public bool DeleteFileUponDisposal { get; set; }

        /// <inheritdoc />
        public void Add(TValue value)
        {
            _stream.Seek(0, SeekOrigin.End);

            using (BinaryWriter writer = GetWriter())
            {
                _valueIO.Write(value, writer);
                _stream.Seek(sizeof(int), SeekOrigin.Begin);
                writer.Write(Count);
            }
        }

        /// <summary>
        /// Adds all of the items in the range to the end of the collection
        /// </summary>
        /// <param name="values">The range items to add</param>
        public void AddRange(IEnumerable<TValue> values)
        {
            _stream.Seek(0, SeekOrigin.End);

            using (BinaryWriter writer = GetWriter())
            {
                try
                {
                    foreach (TValue value in values)
                    {
                        _valueIO.Write(value, writer);
                        ++Count;
                    }
                }
                finally
                {
                    _stream.Seek(sizeof(int), SeekOrigin.Begin);
                    writer.Write(Count);
                }
            }
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
            _stream.Seek(sizeof(int) * 2, SeekOrigin.Begin);

            using (BinaryReader reader = GetReader())
            {
                _metaDataIO.Read(reader);

                for (int i = 0; i < Count; ++i)
                {
                    yield return _valueIO.Read(reader);
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc cref="IList{T}.Count" />
        public int Count { get; private set; }

        private BinaryReader GetReader() => new BinaryReader(_stream, _encoding, leaveOpen: true);
        private BinaryWriter GetWriter() => new BinaryWriter(_stream, _encoding, leaveOpen: true);

        private readonly FileStream _stream;
        private readonly IReadWrite<TValue> _valueIO;
        private readonly IReadWrite<TMetaData> _metaDataIO;
        private readonly Encoding _encoding = new UTF8Encoding(false, true);
        private const int VERSION = 1;
        private const int BUFFER_SIZE = 4096;
    }
}
