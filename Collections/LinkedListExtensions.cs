using System;
using System.Collections.Generic;

namespace Sayer.Collections
{
    public static class LinkedListExtensions
    {
        /// <summary>
        /// Inserts an element at the proper location into a sorted linked list
        /// </summary>
        /// <typeparam name="T">Type of element contained in the linked list</typeparam>
        /// <param name="list">The linked list</param>
        /// <param name="value">The value to insert</param>
        /// <returns>The LinkedListNode of the newly inserted element</returns>
        public static LinkedListNode<T> InsertSorted<T>(this LinkedList<T> list, T value) where T : IComparable
        {
            if (list == null) { throw new ArgumentNullException(nameof(list)); }
            if (value == null) { throw new ArgumentNullException(nameof(value)); }

            if (list.Last == null || value.CompareTo(list.Last.Value) >= 0)
            {
                return list.AddLast(value);
            }

            if (value.CompareTo(list.First.Value) <= 0)
            {
                return list.AddFirst(value);
            }

            LinkedListNode<T> node = list.First;
            while (node != null && node.Value.CompareTo(value) < 0)
            {
                node = node.Next;
            }

            if (node == null) { return list.AddLast(value); }
            return list.AddBefore(node, value);
        }
    }
}
