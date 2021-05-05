using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Sayer.Collections
{
    /// <summary>
    /// Implementation done in terms of ConcurrentDictionary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentHashSet<T> : ISet<T>, IReadOnlyCollection<T>
    {
        public ConcurrentHashSet() =>
            _dictionary = new ConcurrentDictionary<T, object>();

        public ConcurrentHashSet(IEqualityComparer<T> comparer) =>
            _dictionary = new ConcurrentDictionary<T, object>(comparer);

        public IEnumerator<T> GetEnumerator() => _dictionary.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<T>.Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _dictionary.TryAdd(item, null);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                _dictionary.TryAdd(item, null);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (!(other is ISet<T> set))
            {
                set = new HashSet<T>(other);
            }

            var toRemove = new List<T>(_dictionary.Count);

            foreach (T key in _dictionary.Keys.Where(k => !set.Contains(k)))
            {
                toRemove.Add(key);
            }

            foreach (T key in toRemove)
            {
                _dictionary.TryRemove(key, out _);
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                _dictionary.TryRemove(item, out _);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                if (!_dictionary.TryAdd(item, null))
                {
                    _dictionary.TryRemove(item, out _);
                }
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (!(other is ISet<T> set))
            {
                set = new HashSet<T>(other);
            }

            return _dictionary.Count <= set.Count && _dictionary.Keys.All(set.Contains);
        }

        public bool IsSupersetOf(IEnumerable<T> other) => other.All(_dictionary.ContainsKey);

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            int count = 0;

            foreach (T item in other)
            {
                if (!_dictionary.ContainsKey(item))
                {
                    return false;
                }

                ++count;
            }

            return count < _dictionary.Count;
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (!(other is ISet<T> set))
            {
                set = new HashSet<T>(other);
            }

            return _dictionary.Count < set.Count && _dictionary.Keys.All(set.Contains);
        }

        public bool Overlaps(IEnumerable<T> other) => other.Any(_dictionary.ContainsKey);

        public bool SetEquals(IEnumerable<T> other)
        {
            if (!(other is ISet<T> set))
            {
                set = new HashSet<T>(other);
            }

            return set.Count == _dictionary.Count && set.All(_dictionary.ContainsKey);
        }

        public bool Add(T item) => _dictionary.TryAdd(item, null);
        public void Clear() => _dictionary.Clear();
        public bool Contains(T item) => _dictionary.ContainsKey(item);
        public void CopyTo(T[] array, int arrayIndex) => _dictionary.Keys.CopyTo(array, arrayIndex);
        public bool Remove(T item) => _dictionary.TryRemove(item, out _);
        public bool IsReadOnly => false;
        public int Count => _dictionary.Count;

        private readonly ConcurrentDictionary<T, object> _dictionary;
    }
}
