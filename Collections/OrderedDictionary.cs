using System;
using System.Collections;
using System.Collections.Generic;

namespace Sayer.Collections
{
    /// <summary>
    /// A dictionary that is enumerated in the order its elements were added, either oldest to newest or newest to oldest.
    /// </summary>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <typeparam name="TValue">The value type</typeparam>
    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Defines whether to insert new entries at the beginning or the end of the order of enumeration.
        /// </summary>
        public enum AddBehavior { AddFirst, AddLast }

        /// <summary>
        /// Constructor with default capacity. New entries are added to the end of the order.
        /// </summary>
        public OrderedDictionary() : this(0)
        {
        }

        /// <summary>
        /// Constructor. New entries are added to the end of the order.
        /// </summary>
        /// <param name="capacity">Initial capacity of the dictionary</param>
        public OrderedDictionary(int capacity) : this(capacity, AddBehavior.AddLast)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Initial capacity of the dictionary</param>
        /// <param name="addBehavior">Defines whether to insert new entries at the beginning or the end of the order of enumeration.</param>
        public OrderedDictionary(int capacity, AddBehavior addBehavior) : this(capacity, addBehavior, null)
        {
        }

        public OrderedDictionary(int capacity, AddBehavior addBehavior, IEqualityComparer<TKey> keyComparer)
        {
            Behavior = addBehavior;
            Contents = new Dictionary<TKey, LinkedListNode<Entry>>(capacity, keyComparer);
            OrderedList = new LinkedList<Entry>();
        }

        /// <inheritdoc />
        public virtual void Add(TKey key, TValue value)
        {
            var node = new LinkedListNode<Entry>(CreateEntry(key, value));
            Contents.Add(key, node); // Try adding to contents first, as this may throw

            if (Behavior == AddBehavior.AddFirst)
            {
                OrderedList.AddFirst(node);
            }
            else
            {
                OrderedList.AddLast(node);
            }
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            if (Contents.TryGetValue(key, out LinkedListNode<Entry> node))
            {
                Remove(node);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void Clear()
        {
            Contents.Clear();
            OrderedList.Clear();
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (arrayIndex < 0) { throw new ArgumentOutOfRangeException(nameof(arrayIndex)); }
            if (arrayIndex >= array.Length) { throw new ArgumentOutOfRangeException(nameof(arrayIndex)); }

            if (Contents.Count > array.Length - arrayIndex) { throw new ArgumentException("Array is not large enough to hold the elements."); }

            int i = arrayIndex;

            foreach (var entry in Contents)
            {
                // Holy crap.
                //
                // entry is of type KeyValuePair<TKey, LinkedListNode<KeyAndItem>>
                // entry.Value is of type LinkedListNode<KeyAndItem>
                // entry.Value.Value is of type KeyAndItem
                // entry.Value.Value.Item is of type TValue (phew!)
                array[i++] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value.Value.Item);
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            // Verify the values match
            if (Contents.TryGetValue(item.Key, out LinkedListNode<Entry> node) &&
                EqualityComparer<TValue>.Default.Equals(item.Value, node.Value.Item))
            {
                Remove(node);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public virtual TValue this[TKey key]
        {
            get => Contents[key].Value.Item;

            set
            {
                if (Contents.TryGetValue(key, out LinkedListNode<Entry> node))
                {
                    node.Value = CreateEntry(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        /// <inheritdoc />
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        /// <inheritdoc />
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        /// <inheritdoc />
        public virtual bool ContainsKey(TKey key)
        {
            return Contents.ContainsKey(key);
        }

        /// <inheritdoc />
        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return Contents.TryGetValue(item.Key, out LinkedListNode<Entry> node) &&
                    EqualityComparer<TValue>.Default.Equals(item.Value, node.Value.Item);
        }

        /// <inheritdoc />
        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            bool foundIt = false;

            if (Contents.TryGetValue(key, out LinkedListNode<Entry> node))
            {
                value = node.Value.Item;
                foundIt = true;
            }
            else
            {
                value = default;
            }

            return foundIt;
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys => new KeyCollection(this);

        /// <inheritdoc />
        public ICollection<TValue> Values => new ValueCollection(this);

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item) { Add(item.Key, item.Value); }

        /// <inheritdoc />
        public int Count => Contents.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (Entry e in OrderedList)
            {
                yield return new KeyValuePair<TKey, TValue>(e.Key, e.Item);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns the first entry in the collection
        /// </summary>
        public KeyValuePair<TKey, TValue> First => new KeyValuePair<TKey, TValue>(OrderedList.First.Value.Key, OrderedList.First.Value.Item);

        /// <summary>
        /// Returns the last entry in the collection
        /// </summary>
        public KeyValuePair<TKey, TValue> Last => new KeyValuePair<TKey, TValue>(OrderedList.Last.Value.Key, OrderedList.Last.Value.Item);

        /// <summary>
        /// Adds the key and its associated value to the dictionary if there is not already a mapping for the given key.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="value">the value</param>
        /// <returns>true if the key was successfully added, false if the dictionary already contained the key</returns>
        public virtual bool TryAdd(TKey key, TValue value)
        {
            if (Contents.ContainsKey(key))
            {
                return false;
            }

            Add(key, value);
            return true;
        }

        /// <summary>
        /// Gets an entry for the given key. If no entry for the key exists, it will be created using
        /// the supplied creation routine.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="addRoutine">
        /// A delegate that will be invoked only if there is no value associated with the key. Implementations
        /// should return the value to add.
        /// </param>
        /// <returns>the matching item, which may have been newly created</returns>
        public TValue GetOrAdd(TKey key, Func<TValue> addRoutine)
        {
            return AddOrUpdate(key, addRoutine, v => v);
        }

        /// <summary>
        /// Adds an entry or updates an existing entry
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="addRoutine">
        /// A delegate that will be invoked only if there is no value associated with the key. Implementations
        /// should return the value to add.
        /// </param>
        /// <param name="updateRoutine">
        /// A delegate that will be invoked only if there is a value already associated with the key.
        /// The argument to the delegate will be the existing value. Implementations should return the updated
        /// value (which can be the same object that is passed to the delegate).
        /// </param>
        /// <returns>the newly added or updated item</returns>
        public virtual TValue AddOrUpdate(TKey key, Func<TValue> addRoutine, Func<TValue, TValue> updateRoutine)
        {
            TValue result;

            if (Contents.TryGetValue(key, out LinkedListNode<Entry> node))
            {
                result = updateRoutine(node.Value.Item);
                node.Value = CreateEntry(key, result);
            }
            else
            {
                result = addRoutine();
                this[key] = result;
            }

            return result;
        }

        /// <summary>
        /// This method calls the supplied predicate for each element in this dictionary. Each time the
        /// predicate returns true, the item supplied to the predicate will be removed from this dictionary.
        /// If the predicate returns false, the element will not be removed, but iteration will continue.
        /// </summary>
        /// <param name="shouldRemove">Return true to remove the item, false if it should not be removed.</param>
        /// <returns>the number of elements removed</returns>
        public int RemoveIf(Predicate<KeyValuePair<TKey, TValue>> shouldRemove)
        {
            int numberRemoved = 0;
            LinkedListNode<Entry> node = OrderedList.First;

            while (node != null)
            {
                LinkedListNode<Entry> next = node.Next;

                if (shouldRemove(new KeyValuePair<TKey, TValue>(node.Value.Key, node.Value.Item)))
                {
                    Remove(node);
                    ++numberRemoved;
                }

                node = next;
            }

            return numberRemoved;
        }

        protected class Entry
        {
            internal TKey Key { get; }
            internal TValue Item { get; set; }

            internal Entry(TKey key, TValue item)
            {
                Key = key;
                Item = item;
            }
        }

        protected virtual Entry CreateEntry(TKey key, TValue value)
        {
            return new Entry(key, value);
        }

        protected LinkedListNode<Entry> FirstNode => OrderedList.First;
        protected LinkedListNode<Entry> LastNode => OrderedList.Last;

        protected bool TryGetNode(TKey key, out LinkedListNode<Entry> entry)
        {
            return Contents.TryGetValue(key, out entry);
        }

        protected void MoveNodeToFront(LinkedListNode<Entry> node)
        {
            if (node.Previous != null)
            {
                OrderedList.Remove(node);
                OrderedList.AddFirst(node);
            }
        }

        protected void MoveNodeToBack(LinkedListNode<Entry> node)
        {
            if (node.Next != null)
            {
                OrderedList.Remove(node);
                OrderedList.AddLast(node);
            }
        }

        protected void Remove(LinkedListNode<Entry> node)
        {
            Contents.Remove(node.Value.Key);
            OrderedList.Remove(node);
        }

        private abstract class BackedCollection<T> : ICollection<T>
        {
            protected abstract T FromEntry(Entry entry);

            protected BackedCollection(OrderedDictionary<TKey, TValue> orderedDictionary)
            {
                OrderedDictionary = orderedDictionary;
            }

            public bool IsReadOnly => true;
            public int Count => OrderedDictionary.Count;

            public IEnumerator<T> GetEnumerator()
            {
                using (LinkedList<Entry>.Enumerator enumerator = OrderedDictionary.OrderedList.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        yield return FromEntry(enumerator.Current);
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            void ICollection<T>.Add(T item) { throw new NotSupportedException("Collection is read only"); }
            bool ICollection<T>.Remove(T item) { throw new NotSupportedException("Collection is read only"); }
            void ICollection<T>.Clear() { throw new NotSupportedException("Collection is read only"); }

            public bool Contains(T item)
            {
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;

                foreach (Entry entry in OrderedDictionary.OrderedList)
                {
                    if (comparer.Equals(item, FromEntry(entry)))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                if (arrayIndex < 0) { throw new ArgumentOutOfRangeException(nameof(arrayIndex)); }
                if (arrayIndex >= array.Length) { throw new ArgumentOutOfRangeException(nameof(arrayIndex)); }

                if (OrderedDictionary.Count > array.Length - arrayIndex)
                {
                    throw new ArgumentException("Array is not large enough to hold the elements.");
                }

                int i = arrayIndex;

                foreach (Entry entry in OrderedDictionary.OrderedList)
                {
                    array[i++] = FromEntry(entry);
                }
            }

            private OrderedDictionary<TKey, TValue> OrderedDictionary { get; }
        }

        private sealed class KeyCollection : BackedCollection<TKey>
        {
            internal KeyCollection(OrderedDictionary<TKey, TValue> orderedDictionary) : base(orderedDictionary) { }
            protected override TKey FromEntry(Entry entry) { return entry.Key; }
        }

        private sealed class ValueCollection : BackedCollection<TValue>
        {
            internal ValueCollection(OrderedDictionary<TKey, TValue> orderedDictionary) : base(orderedDictionary) { }
            protected override TValue FromEntry(Entry entry) { return entry.Item; }
        }

        private Dictionary<TKey, LinkedListNode<Entry>> Contents { get; }
        private LinkedList<Entry> OrderedList { get; }
        private AddBehavior Behavior { get; }
    }
}
