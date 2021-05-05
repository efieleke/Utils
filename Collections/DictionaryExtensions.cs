using System;
using System.Collections;
using System.Collections.Generic;

namespace Sayer.Collections
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Given two dictionaries with the same key type, enumerates the keys that are common to both, and for each enumeration,
        /// returns the key and the two values associated with that key (one value for the first dictionary, and one for the second).
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValueA"></typeparam>
        /// <typeparam name="TValueB"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<TKey, Tuple<TValueA, TValueB>>> EnumerateCommonKeys<TKey, TValueA, TValueB>(this Dictionary<TKey, TValueA> a, Dictionary<TKey, TValueB> b) =>
            EnumerateCommonKeys(a.AsReadOnly(), b.AsReadOnly());

        /// <summary>
        /// Given two dictionaries with the same key type, enumerates the keys that are common to both, and for each enumeration,
        /// returns the key and the two values associated with that key (one value for the first dictionary, and one for the second).
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValueA"></typeparam>
        /// <typeparam name="TValueB"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<TKey, Tuple<TValueA, TValueB>>> EnumerateCommonKeys<TKey, TValueA, TValueB>(this IDictionary<TKey, TValueA> a, IDictionary<TKey, TValueB> b) =>
            EnumerateCommonKeys(a.AsReadOnly(), b.AsReadOnly());

        /// <summary>
        /// Given two dictionaries with the same key type, enumerates the keys that are common to both, and for each enumeration,
        /// returns the key and the two values associated with that key (one value for the first dictionary, and one for the second).
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValueA"></typeparam>
        /// <typeparam name="TValueB"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<TKey, Tuple<TValueA, TValueB>>> EnumerateCommonKeys<TKey, TValueA, TValueB>(this IReadOnlyDictionary<TKey, TValueA> a, IReadOnlyDictionary<TKey, TValueB> b)
        {
            // Optimize by iterating through the smaller collection rather than the larger one
            if (a.Count < b.Count)
            {
                foreach (KeyValuePair<TKey, TValueA> entry in a)
                {
                    if (b.TryGetValue(entry.Key, out TValueB valueB))
                    {
                        yield return new KeyValuePair<TKey, Tuple<TValueA, TValueB>>(entry.Key, Tuple.Create(entry.Value, valueB));
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<TKey, TValueB> entry in b)
                {
                    if (a.TryGetValue(entry.Key, out TValueA valueA))
                    {
                        yield return new KeyValuePair<TKey, Tuple<TValueA, TValueB>>(entry.Key, Tuple.Create(valueA, entry.Value));
                    }
                }
            }
        }

        /// <summary>
        /// Given two dictionaries of the same type, enumerates the keys are present in one dictionary but not both (i.e. an exclusive or), and for each enumeration,
        /// returns the key/value pair for the element.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<TKey, TValue>> EnumerateDisjointKeys<TKey, TValue>(this Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b) =>
            EnumerateDisjointKeys(a.AsReadOnly(), b.AsReadOnly());

        /// <summary>
        /// Given two dictionaries of the same type, enumerates the keys are present in one set but not both (i.e. an exclusive or), and for each enumeration,
        /// returns the key/value pair for the element.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<TKey, TValue>> EnumerateDisjointKeys<TKey, TValue>(this IDictionary<TKey, TValue> a, IDictionary<TKey, TValue> b) =>
            EnumerateDisjointKeys(a.AsReadOnly(), b.AsReadOnly());

        /// <summary>
        /// Given two dictionaries of the same type, enumerates the keys that are present in one set but not both (i.e. an exclusive or), and for each enumeration,
        /// returns the key/value pair for the element.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<TKey, TValue>> EnumerateDisjointKeys<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> a, IReadOnlyDictionary<TKey, TValue> b)
        {
            foreach (KeyValuePair<TKey, TValue> entry in a)
            {
                if (!b.ContainsKey(entry.Key))
                {
                    yield return entry;
                }
            }

            foreach (KeyValuePair<TKey, TValue> entry in b)
            {
                if (!a.ContainsKey(entry.Key))
                {
                    yield return entry;
                }
            }
        }

        /// <summary>
        /// Copies an IReadOnlyDictionary to a Dictionary. If you want to instead copy an IDictionary, just use one of the Dictionary ctors.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary">The dictionary to copy</param>
        /// <returns>A copy of the input dictionary</returns>
        public static Dictionary<TKey, TValue> Copy<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary) => Copy(dictionary, 0);

        /// <summary>
        /// Copies an IReadOnlyDictionary to a Dictionary. If you want to instead copy an IDictionary, just use one of the Dictionary ctors.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary">The dictionary to copy</param>
        /// <param name="capacity">
        /// The starting capacity of the output dictionary. Leave this at zero unless the returned dictionary will grow
        /// to be larger than the copied dictionary, and you know how what its maximum number of elements will be.
        /// </param>
        /// <returns>A copy of the input dictionary</returns>
        public static Dictionary<TKey, TValue> Copy<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, int capacity)
        {
            var result = new Dictionary<TKey, TValue>(Math.Max(dictionary.Count, capacity));

            // This is a bit faster than LINQ ToDictionary()
            foreach (var entry in dictionary)
            {
                result.Add(entry.Key, entry.Value);
            }

            return result;
        }

        /// <summary>
        /// Casts an IDictionary to an IReadOnlyDictionary. No deep copy is made. Rather, the resulting dictionary refers to the source.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary">the source to convert</param>
        /// <returns>The converted dictionary</returns>
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            // It would be silly to pass something which is already an IReadOnlyDictionary to this method, but nothing's stopping
            // anyone from doing so (e.g. Dictionary implements both IDictionary and IReadOnlyDictionary). Be efficient in this case.
            if (dictionary is IReadOnlyDictionary<TKey, TValue> readOnly)
            {
                return readOnly;
            }

            return new ReadOnlyDict<TKey, TValue>(dictionary);
        }

        private class ReadOnlyDict<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        {
            internal ReadOnlyDict(IDictionary<TKey, TValue> source) { _source = source; }
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _source.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public int Count => _source.Count;
            public bool ContainsKey(TKey key) => _source.ContainsKey(key);
            public bool TryGetValue(TKey key, out TValue value) => _source.TryGetValue(key, out value);
            public TValue this[TKey key] => _source[key];
            public IEnumerable<TKey> Keys => _source.Keys;
            public IEnumerable<TValue> Values => _source.Values;

            private readonly IDictionary<TKey, TValue> _source;
        }
    }
}
