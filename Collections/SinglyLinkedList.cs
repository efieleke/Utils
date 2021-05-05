using System;
using System.Collections;
using System.Collections.Generic;

namespace Sayer.Collections
{
    /// <summary>
    /// A singly linked list. This is both slightly faster and consumes slightly
    /// less memory than LinkedList. Good for when removal only happens at the front of the list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SinglyLinkedList<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            for (Node node = _head; node != null; node = node.Next)
            {
                yield return node.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public void Add(T item) => AddLast(item);

        /// <inheritdoc />
        public void Clear()
        {
            _head = null;
            _tail = null;
            Count = 0;
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            for (Node node = _head; node != null; node = node.Next)
            {
                if (Equals(item, node.Value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(array));
            }

            if (Count > array.Length - arrayIndex)
            {
                throw new ArgumentException("Array not large enough to hold all of the elements in this collection");
            }

            for (Node node = _head; node != null; node = node.Next)
            {
                array[arrayIndex] = node.Value;
                ++arrayIndex;
            }
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            Node previous = null;

            for (Node node = _head; node != null; node = node.Next)
            {
                if (Equals(item, node.Value))
                {
                    if (previous == null) // This will only be true if node is at the head
                    {
                        _head = _head.Next;

                        if (_head == null)
                        {
                            _tail = null;
                        }
                    }
                    else
                    {
                        previous.Next = node.Next;

                        if (previous.Next == null)
                        {
                            _tail = previous;
                        }
                    }

                    --Count;
                    return true;
                }

                previous = node;
            }

            return false;
        }

        /// <summary>
        /// Returns the number of elements in the list
        /// </summary>
        public int Count { get; private set; }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds en element to the front of the list
        /// </summary>
        /// <param name="value">The value to add</param>
        public void AddFirst(T value)
        {
            _head = new Node(value, _head);

            if (_tail == null)
            {
                _tail = _head;
            }

            ++Count;
        }

        /// <summary>
        /// Adds an element to the back of the list.
        /// </summary>
        /// <param name="value">The element to add</param>
        public void AddLast(T value)
        {
            if (_tail == null)
            {
                _tail = new Node(value, null);
                _head = _tail;
            }
            else
            {
                _tail.Next = new Node(value, null);
                _tail = _tail.Next;
            }

            ++Count;
        }

        /// <summary>
        /// Removes the first element in this list
        /// </summary>
        public void RemoveFirst()
        {
            _head = _head.Next;

            if (_head == null)
            {
                _tail = null;
            }

            --Count;
        }

        /// <summary>
        /// Removes the first element in this list and returns the removed element's value
        /// </summary>
        /// <returns></returns>
        public T PopFirst()
        {
            T value = _head.Value;
            RemoveFirst();
            return value;
        }

        /// <summary>
        /// Returns the first element in the list, which must not be empty
        /// </summary>
        public T First => _head.Value;

        /// <summary>
        /// Returns the last element in the list, which must not be empty
        /// </summary>
        public T Last => _tail.Value;

        private class Node
        {
            internal Node(T value, Node next)
            {
                Value = value;
                Next = next;
            }

            internal T Value { get; set; }
            internal Node Next { get; set; }
        }

        private Node _head;
        private Node _tail;
    }
}
