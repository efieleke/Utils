using Sayer.Rand;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sayer.Collections
{
    /// <summary>
    /// Allows for enumeration of all possible permutations given a list of lists. All possible index combinations between the lists are enumerated.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Permutations<T> : IEnumerable<IReadOnlyList<T>>
    {
        /// <summary>
        /// Creates an enumerable set of permutations, given a list of lists.
        /// </summary>
        /// <param name="lists">
        /// The lists of lists that forms the input. Note that the implementation may change the order of elements in these lists.
        /// </param>
        /// <param name="reusePermutationBuffer">
        /// If true, the same buffer will be returned for each enumeration. Be careful, because that buffer will be overwritten with each iteration.
        /// Only set this to true if you are not making a shallow copy of the elements in the returned buffer.
        /// </param>
        public Permutations(IList<IList<T>> lists, bool reusePermutationBuffer = false)
        {
            // Weed out empty lists. The iteration code expects each list to not be empty.
            _lists = lists.Any(l => l.Count == 0) ? lists.Where(l => l.Count > 0).ToList() : lists;
            _reusePermutationBuffer = reusePermutationBuffer;
            _currentPermutation = new List<T>(_lists.Count);
            _indexMultipliers = new int[_lists.Count];
            BigInteger count = _lists.Count == 0 ? 0 : 1;

            for (int i = _lists.Count - 1; i >= 0; --i)
            {
                _indexMultipliers[i] = i == _lists.Count - 1 ? 1 : _indexMultipliers[i + 1] * _lists[i + 1].Count;
                _currentPermutation.Add(default);
                count *= _lists[i].Count;
            }

            Count = count;
        }

        /// <summary>
        /// The total number of possible permutations, made up of all possible combinations of indices between the lists.
        /// </summary>
        public BigInteger Count { get; }

        /// <summary>
        /// Access the permutation at a particular index.
        /// </summary>
        /// <param name="index">The index of the permutation.</param>
        /// <returns>The permutation at the index</returns>
        public IReadOnlyList<T> this[BigInteger index]
        {
            get
            {
                for (int i = 0; i < _currentPermutation.Count; ++i)
                {
                    BigInteger listIndex = index / _indexMultipliers[i];
                    _currentPermutation[i] = _lists[i][(int)listIndex];
                    index -= (listIndex * _indexMultipliers[i]);
                }

                return _reusePermutationBuffer ? _currentPermutation : new List<T>(_currentPermutation);
            }
        }

        /// <summary>
        /// Enumerates all possible permutations of the list of lists provided to the constructor. Each permutation is a
        /// created from a unique combination of indices into the input collections.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IReadOnlyList<T>> GetEnumerator()
        {
            for (BigInteger i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Iterates through all possible permutations, in random order. Each permutation is created from a unique
        /// combination of indices into the input collections. Note that this method alters the contents of the lists
        /// of lists passed to the constructor, as well as the lists within that collection.
        /// </summary>
        /// <param name="random">Random number generator.</param>
        /// <returns></returns>
        public IEnumerable<IReadOnlyList<T>> GetShufflingEnumerable(Random random = null)
        {
            // Because this routine yields, we could continue on a different thread (after an await). We therefore
            // acquire the ThreadSafeRandom each time.

            if (Count == 1)
            {
                yield return this[0];
            }
            else if (Count == 2)
            {
                int index = (random ?? ThreadSafeRandom.Get()).Next(2);
                yield return this[index];
                yield return this[index == 0 ? 1 : 0];
            }
            else if (Count == 3)
            {
                int index = (random ?? ThreadSafeRandom.Get()).Next(3);
                yield return this[index];

                switch (index)
                {
                    case 0:
                        index = (random ?? ThreadSafeRandom.Get()).Next(2) + 1;
                        yield return this[index];
                        yield return this[index == 1 ? 2 : 1];
                        break;
                    case 1:
                        index = (random ?? ThreadSafeRandom.Get()).Next() % 2 == 0 ? 0 : 2;
                        yield return this[index];
                        yield return this[index == 0 ? 2 : 0];
                        break;
                    case 2:
                        index = (random ?? ThreadSafeRandom.Get()).Next(2);
                        yield return this[index];
                        yield return this[index == 0 ? 1 : 0];
                        break;
                }
            }
            else if (Count > 0)
            {
                for (int i = 0; i < _lists.Count; ++i)
                {
                    if (_lists[i].Count > 1)
                    {
                        if (_lists[i] is RandomOrderList randomOrderList)
                        {
                            randomOrderList.Reset();
                        }
                        else if (_lists[i].Count < 5)
                        {
                            _lists[i].Shuffle(random);
                        }
                        else
                        {
                            _lists[i] = new RandomOrderList(_lists[i], random);
                        }
                    }
                }

                for (BigInteger i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }
        }

        // I would have preferred this to inherit from IReadOnlyList, but that caused type issues
        // (IList does not inherit from IReadOnlyList).
        private class RandomOrderList : IList<T>
        {
            internal RandomOrderList(IList<T> original, Random random)
            {
                _original = original;
                _random = random;
            }

            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < _original.Count; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            // This is optimized for sequential access (starting with index 0), which is exactly how it is used. If iteration stops early,
            // we won't have wasted time shuffling elements that were never accessed.
            public T this[int index]
            {
                get
                {
                    if (_shuffledCount <= index)
                    {
                        foreach (var _ in _original.GetShufflingEnumerable(_random, _shuffledCount))
                        {
                            if (++_shuffledCount > index)
                            {
                                break;
                            }
                        }
                    }

                    return _original[index];
                }
                set => throw new NotImplementedException();
            }


            public int Count => _original.Count;

            public bool IsReadOnly => true;

            internal void Reset() { _shuffledCount = 0; }

            // We don't care about any of the methods below.
            public void Add(T item) { throw new NotImplementedException(); }
            public void Clear() { throw new NotImplementedException(); }
            public bool Contains(T item) { throw new NotImplementedException(); }
            public void CopyTo(T[] array, int arrayIndex) { throw new NotImplementedException(); }
            public bool Remove(T item) { throw new NotImplementedException(); }
            public int IndexOf(T item) { throw new NotImplementedException(); }
            public void Insert(int index, T item) { throw new NotImplementedException(); }
            public void RemoveAt(int index) { throw new NotImplementedException(); }

            private readonly IList<T> _original;
            private readonly Random _random;
            private int _shuffledCount;
        }

        private readonly IList<IList<T>> _lists;
        private readonly bool _reusePermutationBuffer;
        private readonly int[] _indexMultipliers;
        private readonly List<T> _currentPermutation;
    }
}
