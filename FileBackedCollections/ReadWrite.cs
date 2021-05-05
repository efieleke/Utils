using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Sayer.Collections;

namespace Sayer.FileBackedCollections
{
    /// <summary>
    /// Streaming classes, particularly helpful in regards to FileBackedDictionary and FileBackedList.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadWrite<T>
    {
        /// <summary>
        /// Reads an object. The underlying stream position should be just past the end of the object upon return.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>The object.</returns>
        T Read(BinaryReader reader);

        /// <summary>
        /// Writes an object. The underlying stream position should be just past the end of the object upon return.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="writer"></param>
        void Write(T value, BinaryWriter writer);
    }

    public class BoolIO : IReadWrite<bool>
    {
        public bool Read(BinaryReader reader) => reader.ReadBoolean();
        public void Write(bool value, BinaryWriter writer) => writer.Write(value);
    }

    public class ByteIO : IReadWrite<byte>
    {
        public byte Read(BinaryReader reader) => reader.ReadByte();
        public void Write(byte value, BinaryWriter writer) => writer.Write(value);
    }

    public class SByteIO : IReadWrite<sbyte>
    {
        public sbyte Read(BinaryReader reader) => reader.ReadSByte();
        public void Write(sbyte value, BinaryWriter writer) => writer.Write(value);
    }

    public class CharIO : IReadWrite<char>
    {
        public char Read(BinaryReader reader) => reader.ReadChar();
        public void Write(char value, BinaryWriter writer) => writer.Write(value);
    }

    public class ShortIO : IReadWrite<short>
    {
        public short Read(BinaryReader reader) => reader.ReadInt16();
        public void Write(short value, BinaryWriter writer) => writer.Write(value);
    }

    public class UShortIO : IReadWrite<ushort>
    {
        public ushort Read(BinaryReader reader) => reader.ReadUInt16();
        public void Write(ushort value, BinaryWriter writer) => writer.Write(value);
    }

    public class IntIO : IReadWrite<int>
    {
        public int Read(BinaryReader reader) => reader.ReadInt32();
        public void Write(int value, BinaryWriter writer) => writer.Write(value);
    }

    public class UIntIO : IReadWrite<uint>
    {
        public uint Read(BinaryReader reader) => reader.ReadUInt32();
        public void Write(uint value, BinaryWriter writer) => writer.Write(value);
    }

    public class LongIO : IReadWrite<long>
    {
        public long Read(BinaryReader reader) => reader.ReadInt64();
        public void Write(long value, BinaryWriter writer) => writer.Write(value);
    }

    public class ULongIO : IReadWrite<ulong>
    {
        public ulong Read(BinaryReader reader) => reader.ReadUInt64();
        public void Write(ulong value, BinaryWriter writer) => writer.Write(value);
    }

    public class FloatIO : IReadWrite<float>
    {
        public float Read(BinaryReader reader) => reader.ReadSingle();
        public void Write(float value, BinaryWriter writer) => writer.Write(value);
    }

    public class DoubleIO : IReadWrite<double>
    {
        public double Read(BinaryReader reader) => reader.ReadDouble();
        public void Write(double value, BinaryWriter writer) => writer.Write(value);
    }

    public class DecimalIO : IReadWrite<decimal>
    {
        public decimal Read(BinaryReader reader) => reader.ReadDecimal();
        public void Write(decimal value, BinaryWriter writer) => writer.Write(value);
    }

    public class StringIO : IReadWrite<string>
    {
        public string Read(BinaryReader reader) => reader.ReadString();
        public void Write(string value, BinaryWriter writer) => writer.Write(value);
    }

    public class DateTimeIO : IReadWrite<DateTime>
    {
        public DateTime Read(BinaryReader reader) => new DateTime(reader.ReadInt64());
        public void Write(DateTime dateTime, BinaryWriter writer) => writer.Write(dateTime.Ticks);
    }

    public class TypeIO : IReadWrite<Type>
    {
        public Type Read(BinaryReader reader)
        {
            string typeName = reader.ReadString();

            try
            {
                return Type.GetType(typeName, throwOnError: true, ignoreCase: false);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to read type {typeName}. Reason: {e.Message}", e);
            }
        }

        public void Write(Type value, BinaryWriter writer) => writer.Write(value.AssemblyQualifiedName ?? throw new ArgumentException($"Assembly qualified name not available for type {value}"));
    }

    public class ReferenceIO<T> : IReadWrite<T> where T : class
    {
        public ReferenceIO(IReadWrite<T> readWrite) => _readWrite = readWrite;

        public T Read(BinaryReader reader)
        {
            bool hasValue = reader.ReadBoolean();
            return hasValue ? _readWrite.Read(reader) : (T)null;
        }

        public void Write(T value, BinaryWriter writer)
        {
            writer.Write(value != null);

            if (value != null)
            {
                _readWrite.Write(value, writer);
            }
        }

        private readonly IReadWrite<T> _readWrite;
    }

    public class NullableIO<T> : IReadWrite<T?> where T : struct
    {
        public NullableIO(IReadWrite<T> readWrite) => _readWrite = readWrite;

        public T? Read(BinaryReader reader)
        {
            bool hasValue = reader.ReadBoolean();
            return hasValue ? _readWrite.Read(reader) : (T?)null;
        }

        public void Write(T? value, BinaryWriter writer)
        {
            writer.Write(value.HasValue);

            if (value.HasValue)
            {
                _readWrite.Write(value.Value, writer);
            }
        }

        private readonly IReadWrite<T> _readWrite;
    }

    public class KeyValuePairIO<TKey, TValue> : IReadWrite<KeyValuePair<TKey, TValue>>
    {
        public KeyValuePairIO(IReadWrite<TKey> keyIO, IReadWrite<TValue> valueIO)
        {
            _keyIO = keyIO;
            _valueIO = valueIO;
        }

        public KeyValuePair<TKey, TValue> Read(BinaryReader reader) =>
            new KeyValuePair<TKey, TValue>(_keyIO.Read(reader), _valueIO.Read(reader));

        public void Write(KeyValuePair<TKey, TValue> value, BinaryWriter writer)
        {
            _keyIO.Write(value.Key, writer);
            _valueIO.Write(value.Value, writer);
        }

        private readonly IReadWrite<TKey> _keyIO;
        private readonly IReadWrite<TValue> _valueIO;
    }

    public class TupleIO<T1, T2> : IReadWrite<Tuple<T1, T2>>
    {
        public TupleIO(IReadWrite<T1> t1IO, IReadWrite<T2> t2IO)
        {
            _t1IO = t1IO;
            _t2IO = t2IO;
        }

        public Tuple<T1, T2> Read(BinaryReader reader) => Tuple.Create(_t1IO.Read(reader), _t2IO.Read(reader));

        public void Write(Tuple<T1, T2> value, BinaryWriter writer)
        {
            _t1IO.Write(value.Item1, writer);
            _t2IO.Write(value.Item2, writer);
        }

        private readonly IReadWrite<T1> _t1IO;
        private readonly IReadWrite<T2> _t2IO;
    }

    public class ListIO<T> : IReadWrite<IReadOnlyList<T>>
    {
        public ListIO(IReadWrite<T> elementIO)
        {
            _elementIO = elementIO;
        }

        public IReadOnlyList<T> Read(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            var result = new List<T>(count);

            for (int i = 0; i < count; ++i)
            {
                result.Add(_elementIO.Read(reader));
            }

            return result;
        }

        public void Write(IReadOnlyList<T> value, BinaryWriter writer)
        {
            writer.Write(value.Count);

            foreach (T element in value)
            {
                _elementIO.Write(element, writer);
            }
        }

        private readonly IReadWrite<T> _elementIO;
    }

    public class ArrayIO<T> : IReadWrite<T[]>
    {
        public ArrayIO(IReadWrite<T> elementIO)
        {
            _elementIO = elementIO;
        }

        public T[] Read(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            var result = new T[count];

            for (int i = 0; i < count; ++i)
            {
                result[i] = _elementIO.Read(reader);
            }

            return result;
        }

        public void Write(T[] value, BinaryWriter writer)
        {
            writer.Write(value.Length);

            foreach (T element in value)
            {
                _elementIO.Write(element, writer);
            }
        }

        private readonly IReadWrite<T> _elementIO;
    }

    public class DictionaryIO<TKey, TValue> : IReadWrite<IReadOnlyDictionary<TKey, TValue>>
    {
        public DictionaryIO(IReadWrite<TKey> keyIO, IReadWrite<TValue> valueIO) : this(new KeyValuePairIO<TKey, TValue>(keyIO, valueIO))
        {
        }

        public DictionaryIO(KeyValuePairIO<TKey, TValue> keyValuePairIO)
        {
            _keyValuePairIO = keyValuePairIO;
        }

        public IReadOnlyDictionary<TKey, TValue> Read(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            var result = new Dictionary<TKey, TValue>(count);

            for (int i = 0; i < count; ++i)
            {
                KeyValuePair<TKey, TValue> keyValuePair = _keyValuePairIO.Read(reader);
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return result;
        }

        public void Write(IReadOnlyDictionary<TKey, TValue> value, BinaryWriter writer)
        {
            writer.Write(value.Count);

            foreach (KeyValuePair<TKey, TValue> entry in value)
            {
                _keyValuePairIO.Write(entry, writer);
            }
        }

        private readonly KeyValuePairIO<TKey, TValue> _keyValuePairIO;
    }

    public class MultiMapIO<TKey, TValue> : IReadWrite<IReadOnlyMultiMap<TKey, TValue>>
    {
        public MultiMapIO(IReadWrite<TKey> keyIO, IReadWrite<TValue> valueIO)
        {
            _keyIO = keyIO;
            _valueIO = valueIO;
        }

        public IReadOnlyMultiMap<TKey, TValue> Read(BinaryReader reader)
        {
            int keyCount = reader.ReadInt32();
            var result = new MultiMap<TKey, TValue>(keyCount);

            for (int i = 0; i < keyCount; ++i)
            {
                TKey key = _keyIO.Read(reader);
                int valueCount = reader.ReadInt32();

                for (int j = 0; j < valueCount; ++j)
                {
                    result.Add(key, _valueIO.Read(reader));
                }
            }

            return result;
        }

        public void Write(IReadOnlyMultiMap<TKey, TValue> value, BinaryWriter writer)
        {
            writer.Write(value.KeyCount);

            foreach (TKey key in value.Keys)
            {
                _keyIO.Write(key, writer);
                IReadOnlyCollection<TValue> values = value[key];
                writer.Write(values.Count);

                foreach (TValue val in values)
                {
                    _valueIO.Write(val, writer);
                }
            }
        }

        private readonly IReadWrite<TKey> _keyIO;
        private readonly IReadWrite<TValue> _valueIO;
    }

    public class JsonIO<T> : IReadWrite<T>
    {
        public T Read(BinaryReader reader)
        {
            Type type = _typeIO.Read(reader);
            string jsonString = _stringIO.Read(reader);
            var serializer = new DataContractJsonSerializer(type);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return (T)serializer.ReadObject(stream);
            }
        }

        public void Write(T value, BinaryWriter writer)
        {
            Type type = value.GetType();
            _typeIO.Write(type, writer);
            var serializer = new DataContractJsonSerializer(type);

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, value);
                string jsonString = Encoding.UTF8.GetString(stream.ToArray());
                _stringIO.Write(jsonString, writer);
            }
        }

        private readonly TypeIO _typeIO = new TypeIO();
        private readonly StringIO _stringIO = new StringIO();
    }
}
