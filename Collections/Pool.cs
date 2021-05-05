using System;
using System.Collections.Generic;
using System.Linq;

namespace Sayer.Collections
{
    /// <summary>
    /// Represents a pool of items, useful in some cases to prevent lots of unnecessary allocations and the resulting expense
    /// of garbage collection. This class is not thread safe. Use ConcurrentPool if thread safe access is required.
    ///
    /// It is only necessary to call Dispose() on a Pool instance item if type T implements IDisposable
    ///
    /// The .NET framework has the MemoryPool and ArrayPool classes, but I found them limited in terms of how one could define
    /// custom behavior. Inheriting from MemoryPool offered no benefit for this implementation.
    /// </summary>
    /// <typeparam name="T">Type of item in the pool</typeparam>
    public sealed class Pool<T> : AbstractPool<T>
    {
        /// <summary>
        /// Constructs a pool filled with items of type T.
        /// </summary>
        /// <param name="poolSize">The maximum number of items that can be held in the pool. The pool is filled with this many items when created.</param>
        /// <param name="createFunc">
        /// A function that returns an instance of type T.
        /// </param>
        /// <param name="returnAction">
        /// Action to perform upon an item after returning it to the pool. Note that this is called lazily, before the item is next taken.
        /// If null, there will be no return action.
        /// </param>
        public Pool(int poolSize, Func<T> createFunc, Action<T> returnAction = null)
        {
            if (poolSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(poolSize));
            }

            _poolSize = poolSize;
            _createFunc = createFunc;
            _returnAction = returnAction;
            _pool = new List<T>(poolSize);

            foreach (var _ in Enumerable.Range(0, _poolSize))
            {
                _pool.Add(_createFunc());
            }
        }

        /// <inheritdoc />
        public override T Take()
        {
            if (_pool.Count == 0)
            {
                return _createFunc();
            }

            T item = _pool[_pool.Count - 1];
            _pool.RemoveAt(_pool.Count - 1);
            _returnAction?.Invoke(item);
            return item;
        }

        /// <inheritdoc />
        public override void Return(T item)
        {
            if (_pool.Count < _poolSize)
            {
                _pool.Add(item);
            }
            else if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <inheritdoc />
        public override int MaxPoolSize
        {
            get => _poolSize;

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Pool size must be greater than zero");
                }

                _poolSize = value;

                while (_pool.Count > _poolSize)
                {
                    T item = _pool[_pool.Count - 1];
                    _pool.RemoveAt(_pool.Count - 1);

                    if (item is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        public override void Dispose()
        {
            foreach (T value in _pool)
            {
                if (value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private volatile int _poolSize;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _returnAction;

        // An array is more performant than a linked list due to the elements being contiguous (better for caching),
        // and because every node creation in a linked list requires a heap allocation.
        private readonly List<T> _pool;
    }
}
