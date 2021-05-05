using System;

namespace Sayer.Collections
{
    /// <summary>
    /// Represents a thread-safe pool of items, useful in some cases to prevent lots of unnecessary allocations
    /// and the resulting expense of garbage collection.
    ///
    /// It is only necessary to call Dispose() on a Pool instance item if type T implements IDisposable
    ///
    /// The .NET framework has the MemoryPool and ArrayPool classes, but I found them limited in terms of how one could define
    /// custom behavior.
    /// </summary>
    /// <typeparam name="T">Type of item in the pool</typeparam>
    public sealed class ConcurrentPool<T> : AbstractPool<T>
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
        public ConcurrentPool(int poolSize, Func<T> createFunc, Action<T> returnAction = null)
        {
            _pool = new Pool<T>(poolSize, createFunc, returnAction);
        }

        /// <inheritdoc />
        public override T Take()
        {
            lock (_lock)
            {
                return _pool.Take();
            }
        }

        /// <inheritdoc />
        public override void Return(T item)
        {
            lock (_lock)
            {
                _pool.Return(item);
            }
        }

        /// <inheritdoc />
        public override int MaxPoolSize
        {
            get
            {
                lock (_lock)
                {
                    return _pool.MaxPoolSize;
                }
            }

            set
            {
                lock (_lock)
                {
                    _pool.MaxPoolSize = value;
                }
            }
        }

        public override void Dispose() => _pool.Dispose();

        private readonly Pool<T> _pool;
        private readonly object _lock = new object();
    }
}
