using System;
using System.Collections.Generic;

namespace Sayer.Collections
{
    /// <summary>
    /// A cache with a maximum size. When a new entry is added, the oldest entry is removed if the maximum size has
    /// been reached. Accessing an entry refreshes it, marking it as the most recent entry.
    ///
    /// If an expiration period is defined, entries will be expired if not accessed within that period. Expired entries
    /// will be removed before new items are added, or when PruneExpired() is called.
    ///
    /// This class is not thread safe. A thread-safe version of this class is LimitedDictionary (which should eventually
    /// be renamed to ConcurrentCache).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class Cache<TKey, TValue> : OrderedDictionary<TKey, TValue>
    {
        public Cache(uint maxSize, TimeSpan? expiration) : base(0, AddBehavior.AddFirst)
        {
            MaxSize = maxSize < 1 ? throw new ArgumentException("Cache max size must be non-zero", nameof(maxSize)) : maxSize;

            if (expiration.HasValue && expiration.Value.Ticks <= 0)
            {
                throw new ArgumentException("Expiration must be a positive value, or null");
            }

            Expiration = expiration;
        }

        public uint MaxSize { get; }
        public TimeSpan? Expiration { get; }

        public override void Add(TKey key, TValue value)
        {
            PruneExpired();
            base.Add(key, value);

            // maybe someday MaxSize will be read/write, which explains 'while' instead of 'if'
            while (Count > MaxSize)
            {
                Remove(LastNode);
            }
        }

        public override TValue this[TKey key]
        {
            get
            {
                if (TryGetNode(key, out LinkedListNode<Entry> node))
                {
                    TouchNode(node);
                    return node.Value.Item;
                }

                throw new KeyNotFoundException($"Key {key} not found");
            }

            set
            {
                if (TryGetNode(key, out LinkedListNode<Entry> node))
                {
                    node.Value.Item = value;
                    TouchNode(node);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public override bool ContainsKey(TKey key)
        {
            bool contains = false;

            if (TryGetNode(key, out LinkedListNode<Entry> node))
            {
                TouchNode(node);
                contains = true;
            }

            return contains;
        }

        public override bool Contains(KeyValuePair<TKey, TValue> item)
        {
            bool contains = false;

            if (TryGetNode(item.Key, out LinkedListNode<Entry> node) &&
                EqualityComparer<TValue>.Default.Equals(item.Value, node.Value.Item))
            {
                TouchNode(node);
                contains = true;
            }

            return contains;
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            bool foundIt = false;

            if (TryGetNode(key, out LinkedListNode<Entry> node))
            {
                value = node.Value.Item;
                TouchNode(node);
                foundIt = true;
            }
            else
            {
                value = default;
            }

            return foundIt;
        }

        public override bool TryAdd(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                return false;
            }

            Add(key, value);
            return true;
        }

        public override TValue AddOrUpdate(TKey key, Func<TValue> addRoutine, Func<TValue, TValue> updateRoutine)
        {
            TValue result;

            if (TryGetNode(key, out LinkedListNode<Entry> node))
            {
                result = updateRoutine(node.Value.Item);
                node.Value = CreateEntry(key, result);
                MoveNodeToFront(node);
            }
            else
            {
                result = addRoutine();
                Add(key, result);
            }

            return result;
        }

        public bool Touch(TKey key) => ContainsKey(key);

        public void PruneExpired()
        {
            if (Expiration.HasValue)
            {
                DateTime expiryTime = DateTime.UtcNow - Expiration.Value;
                const int optimizationCheckMagicNumber = 3;
                int numberPruned = 0;

                Prune(entry =>
                {
                    // It's possible that the entire collection has not been accessed for a while. We can save a lot of
                    // cycles if the most recently accessed element is also expired by simply clearing the entire collection.
                    // We don't check this every time, because PruneExpired is called with every add routine, and all the
                    // additional expiry checks against the most recent element would be wasteful. But if the oldest
                    // 3 elements are expired, then perhaps all the elements are expired.
                    //
                    // The magic number of 3 was chosen because it's a small enough number that we will have wasted little time
                    // pruning elements before the check, but will have pruned enough elements to warrant the quick optimization check.
                    if (numberPruned++ == optimizationCheckMagicNumber && expiryTime >= ((EntryWithTimeStamp)FirstNode.Value).LastAccessed)
                    {
                        Clear();
                        return false;
                    }

                    return expiryTime >= entry.LastAccessed;
                });
            }
        }

        protected override Entry CreateEntry(TKey key, TValue value)
        {
            return Expiration.HasValue ? new EntryWithTimeStamp(key, value) : new Entry(key, value);
        }

        private class EntryWithTimeStamp : Entry
        {
            internal DateTime LastAccessed { get; set; }

            internal EntryWithTimeStamp(TKey key, TValue item) : base(key, item)
            {
                LastAccessed = DateTime.UtcNow;
            }
        }

        private void TouchNode(LinkedListNode<Entry> node)
        {
            if (node.Value is EntryWithTimeStamp entryWithTimeStamp)
            {
                entryWithTimeStamp.LastAccessed = DateTime.UtcNow;
            }

            MoveNodeToFront(node);
        }

        private void Prune(Predicate<EntryWithTimeStamp> shouldRemove)
        {
            for (LinkedListNode<Entry> node = LastNode; node != null && shouldRemove((EntryWithTimeStamp)node.Value); node = LastNode)
            {
                Remove(node);
            }
        }
    }
}
