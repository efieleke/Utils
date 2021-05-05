using System;

namespace Sayer.Collections
{
    /// <summary>
    /// Represents a pool of items, useful in some cases to prevent lots of unnecessary allocations and the resulting expense
    /// of garbage collection.
    ///
    /// The .NET framework has the MemoryPool and ArrayPool classes, but I found them limited in terms of how one could define
    /// custom behavior. Inheriting from MemoryPool offered no benefit for this implementation.
    /// </summary>
    /// <typeparam name="T">Type of item in the pool</typeparam>
    public interface IPool<T> : IDisposable
    {
        /// <summary>
        /// Borrows an item from the pool. If the pool is empty, a new item is created. The item is stored in the returned object's Value property.
        /// </summary>
        /// <returns>
        /// An IBorrowedItem, which can be used to access the pool item via its Value property. When the borrowed item is disposed,
        /// the item is returned to the pool.
        /// </returns>
        IBorrowedItem<T> Borrow();

        /// <summary>
        /// Retrieves an item from the pool, if the pool is not empty. If the pool is empty, a new item is created. The value returned from
        /// this method should always be returned to the pool via the Return() method. If the caller neglects to do so, there is no harm, other than
        /// that the pool is not being used effectively.
        /// </summary>
        /// <returns>An instance of type T. This should be returned to the pool via the Return method when it is no longer needed.</returns>
        T Take();

        /// <summary>
        /// Returns an item to the pool. If the pool is at capacity, this is a no-op.
        /// </summary>
        /// <param name="item">
        /// The item to return. This will have been initially retrieved from the Take() method.
        /// </param>
        void Return(T item);

        /// <summary>
        /// The maximum number of items that the pool can hold.
        /// </summary>
        int MaxPoolSize { get; set; }
    }

    /// <summary>
    /// Pool accessor interface, which borrows from the pool when instantiated, and returns to the pool when disposed.
    /// </summary>
    public interface IBorrowedItem<out T> : IDisposable
    {
        /// <summary>
        /// The item that was retrieved from the pool.
        /// </summary>
        T Value { get; }
    }
}
