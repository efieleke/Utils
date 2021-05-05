
namespace Sayer.Collections
{
    /// <summary>
    /// Represents a pool of items, useful in some cases to prevent lots of unnecessary allocations and the resulting expense
    /// of garbage collection.
    ///
    /// It is only necessary to call Dispose() on a Pool instance item if type T implements IDisposable
    ///
    /// The .NET framework has the MemoryPool and ArrayPool classes, but I found them limited in terms of how one could define
    /// custom behavior. Inheriting from MemoryPool offered no benefit for this implementation.
    /// </summary>
    /// <typeparam name="T">Type of item in the pool</typeparam>
    public abstract class AbstractPool<T> : IPool<T>
    {
        /// <inheritdoc />
        public IBorrowedItem<T> Borrow() => new BorrowedItem(this);

        /// <inheritdoc />
        public abstract T Take();

        /// <inheritdoc />
        public abstract void Return(T item);

        /// <inheritdoc />
        public abstract int MaxPoolSize { get; set; }

        /// <inheritdoc />
        public abstract void Dispose();

        private class BorrowedItem : IBorrowedItem<T>
        {
            internal BorrowedItem(IPool<T> pool)
            {
                _pool = pool;
                Value = pool.Take();
            }

            public T Value { get; }
            public void Dispose() => _pool.Return(Value);

            private readonly IPool<T> _pool;
        }
    }
}
