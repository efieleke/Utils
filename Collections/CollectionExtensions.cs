using System;
using System.Collections;
using System.Collections.Generic;

namespace Sayer.Collections
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Casts the input collection to an IReadOnlyCollection. No deep copy is made. Rather, the resulting collection refers to the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> collection)
        {
            if (collection is IReadOnlyCollection<T> readOnly)
            {
                return readOnly;
            }

            return new ReadOnlyColl<T>(collection);
        }

        [Serializable]
        private class ReadOnlyColl<T> : IReadOnlyCollection<T>
        {
            public ReadOnlyColl(ICollection<T> collection) { _collection = collection; }
            public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public int Count => _collection.Count;

            private readonly ICollection<T> _collection;
        }
    }
}
