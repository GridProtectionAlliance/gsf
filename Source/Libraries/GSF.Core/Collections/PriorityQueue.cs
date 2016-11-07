//******************************************************************************************************
//  PriorityQueue.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/28/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GSF.Collections
{
    /// <summary>
    /// Represents a queue of items to which priority can be assigned.
    /// </summary>
    /// <typeparam name="T">The type of the items stored in the queue.</typeparam>
    public class PriorityQueue<T> : IList<T>
    {
        #region [ Members ]

        // Nested Types
        private class PriorityQueueNode : IComparable<PriorityQueueNode>
        {
            public int Priority;
            public int InsertionOrder;
            public T Item;

            public int CompareTo(PriorityQueueNode other)
            {
                // Higher priority than other
                if (Priority > other.Priority)
                    return 1;

                // Lower priority than other
                if (Priority < other.Priority)
                    return -1;

                // Same priority, but inserted after other
                if (InsertionOrder - other.InsertionOrder > 0)
                    return -1;

                // Assuming nodes are never compared to themselves:
                // Same priority, but inserted before other
                return 1;
            }

            public static bool operator >(PriorityQueueNode node1, PriorityQueueNode node2)
            {
                return node1.CompareTo(node2) > 0;
            }

            public static bool operator <(PriorityQueueNode node1, PriorityQueueNode node2)
            {
                return node1.CompareTo(node2) < 0;
            }
        }

        // Fields
        private PriorityQueueNode[] m_heap;
        private int m_count;
        private int m_insertionOrder;
        private int m_version;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="PriorityQueue{T}"/> class.
        /// </summary>
        public PriorityQueue()
            : this(0)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PriorityQueue{T}"/> class.
        /// </summary>
        /// <param name="capacity">The initial size of the underlying array.</param>
        public PriorityQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            if (capacity == 0)
                m_heap = EmptyHeap;
            else
                m_heap = new PriorityQueueNode[capacity];
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the head of the queue. The value returned
        /// is the same as the <see cref="Peek"/> method.
        /// </summary>
        public T Head
        {
            get
            {
                return Peek();
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="PriorityQueue{T}"/>.</exception>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= m_count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return m_heap[index].Item;
            }
            set
            {
                if (index < 0 || index >= m_count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                m_heap[index].Item = value;
                m_version++;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="PriorityQueue{T}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="PriorityQueue{T}"/>.
        /// </returns>
        public int Count
        {
            get
            {
                return m_count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="PriorityQueue{T}"/> is read-only.
        /// </summary>
        /// <returns>
        /// False
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Enqueues an item into the queue.
        /// </summary>
        /// <param name="priority">The priority of the item.</param>
        /// <param name="item">The item to be enqueued.</param>
        /// <remarks>
        /// After adding an item to the queue, the heap will need to be fixed,
        /// therefore this is an O(log n) operation.
        /// </remarks>
        public void Enqueue(int priority, T item)
        {
            int newIndex;

            if (m_heap.Length == m_count)
                IncreaseCapacity();

            newIndex = m_count;
            m_count++;

            m_heap[newIndex] = new PriorityQueueNode()
            {
                Priority = priority,
                InsertionOrder = m_insertionOrder,
                Item = item
            };

            FixHeapUp(newIndex);

            m_insertionOrder++;
            m_version++;
        }

        /// <summary>
        /// Gets the item with the highest priority and removes it from the queue.
        /// </summary>
        /// <returns>The item with the highest priority.</returns>
        /// <remarks>
        /// After removing an item from the queue, the heap will need to be fixed,
        /// therefore this is an O(log n) operation.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The queue is empty.</exception>
        public T Dequeue()
        {
            T head;

            if (m_count == 0)
                throw new InvalidOperationException("Cannot dequeue from an empty queue.");

            head = m_heap[0].Item;
            RemoveAt(0);

            return head;
        }

        /// <summary>
        /// Gets the item with the highest priority.
        /// </summary>
        /// <returns>The item with the highest priority.</returns>
        /// <exception cref="InvalidOperationException">The queue is empty.</exception>
        public T Peek()
        {
            if (m_count == 0)
                throw new InvalidOperationException("Cannot peek at an empty queue.");

            return m_heap[0].Item;
        }

        /// <summary>
        /// Gets the priority of the item at the given index.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        /// <returns>The priority of the item.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Index does not fall within the bounds of the queue.</exception>
        public int GetPriority(int index)
        {
            if (index < 0 || index >= m_count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return m_heap[index].Priority;
        }

        /// <summary>
        /// Sets the priority of the item at the given index.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        /// <param name="priority">The new priority of the item.</param>
        /// <remarks>
        /// This method can be used to change the priority of the item
        /// at the given index. After setting the priority, the queue
        /// will have to fix the heap so this is an O(log n) operation.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Index does not fall within the bounds of the queue.</exception>
        public void SetPriority(int index, int priority)
        {
            int oldPriority;

            if (index < 0 || index >= m_count)
                throw new ArgumentOutOfRangeException(nameof(index));

            oldPriority = m_heap[index].Priority;
            m_heap[index].Priority = priority;

            if (priority > oldPriority)
                FixHeapUp(index);
            else if(priority < oldPriority)
                FixHeapDown(index);

            m_version++;
        }

        /// <summary>
        /// Adds the given value to the priority of all values in the priority queue.
        /// </summary>
        /// <param name="delta">The amount by which to adjust priorities.</param>
        /// <remarks>
        /// This allows for adjusting the priorities of all items in the heap without
        /// having to fix the heap each time a priority is changed. This can be useful
        /// to increase the priorities of items that have been in the queue for significant
        /// periods of time to prevent starvation. This is an O(n) operation.
        /// </remarks>
        public void AdjustPriority(int delta)
        {
            foreach (PriorityQueueNode node in m_heap)
                node.Priority += delta;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        /// <remarks>
        /// The enumerator performs an in-order traversal of the heap, so the caller should
        /// expect that items will be iterated in no particular order. Any operations that
        /// would modify the collection during iteration will cause the enumerator to throw
        /// an <see cref="InvalidOperationException"/>.
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            int version = m_version;
            int count = 0;

            foreach (PriorityQueueNode node in m_heap)
            {
                if (version != m_version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute");

                if (count >= m_count)
                    break;

                yield return node.Item;

                count++;
            }
        }

        /// <summary>
        /// Determines whether the <see cref="PriorityQueue{T}"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="PriorityQueue{T}"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="PriorityQueue{T}"/>; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            Func<T, bool> comparer;

            if ((object)item != null)
                comparer = heapItem => item.Equals(heapItem);
            else
                comparer = heapItem => (object)heapItem == null;

            return this.Any(comparer);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="PriorityQueue{T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="PriorityQueue{T}"/>.</param>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            Func<T, bool> comparer;
            int index;

            if ((object)item != null)
                comparer = heapItem => !item.Equals(heapItem);
            else
                comparer = heapItem => (object)heapItem != null;

            index = this.TakeWhile(comparer).Count();

            return (index < m_count) ? index : -1;
        }

        /// <summary>
        /// Copies the elements of the <see cref="PriorityQueue{T}"/> to an
        /// <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="PriorityQueue{T}"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source <see cref="PriorityQueue{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if ((object)array == null)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (array.Length - arrayIndex < m_count)
                throw new ArgumentException("Not enough space in destination.", nameof(array));

            foreach (T item in this)
                array[arrayIndex++] = item;
        }

        /// <summary>
        /// Removes the <see cref="PriorityQueue{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <remarks>
        /// After removing an item from the queue, the heap will need to be fixed,
        /// therefore this is an O(log n) operation.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="PriorityQueue{T}"/>.</exception>
        public void RemoveAt(int index)
        {
            PriorityQueueNode removedNode;
            PriorityQueueNode swappedNode;

            if (index < 0 || index >= m_count)
                throw new ArgumentOutOfRangeException(nameof(index));

            m_count--;
            m_version++;

            if (index == m_count)
            {
                m_heap[m_count] = null;
            }
            else
            {
                removedNode = m_heap[index];
                swappedNode = m_heap[m_count];

                m_heap[index] = swappedNode;
                m_heap[m_count] = null;

                if (swappedNode > removedNode)
                    FixHeapUp(index);
                else if (swappedNode < removedNode)
                    FixHeapDown(index);
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="PriorityQueue{T}"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="PriorityQueue{T}"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="PriorityQueue{T}"/>; otherwise, false.
        /// This method also returns false if <paramref name="item"/> is not found in the original <see cref="PriorityQueue{T}"/>.
        /// </returns>
        /// <remarks>
        /// After removing an item from the queue, the heap will need to be fixed,
        /// therefore this is an O(log n) operation.
        /// </remarks>
        public bool Remove(T item)
        {
            int index = IndexOf(item);

            if (index < 0)
                return false;

            RemoveAt(index);

            return true;
        }

        /// <summary>
        /// Removes all items from the <see cref="PriorityQueue{T}"/>.
        /// </summary>
        public void Clear()
        {
            Array.Clear(m_heap, 0, m_count);
            m_count = 0;
            m_version++;
        }

        private void FixHeapUp(int child)
        {
            int parent;
            int left;
            int right;
            int highestChild;

            // If the child has no parent,
            // there's nothing to do
            if (child == 0)
                return;

            // Get references to the child's
            // parent and both its children
            parent = GetParent(child);
            left = GetLeftChild(parent);
            right = GetRightChild(parent);

            // Determine which of the children has greater priority
            highestChild = left;

            if (right < m_count && m_heap[right] > m_heap[left])
                highestChild = right;

            // If the higher-priority child has a greater priority than its parent,
            // swap the parent and child nodes. Since the priority of the parent has
            // changed, we'll need to continue up the heap to get it to the right place
            if (m_heap[highestChild] > m_heap[parent])
            {
                Swap(parent, highestChild);
                FixHeapUp(parent);
            }
        }

        private void FixHeapDown(int parent)
        {
            int left;
            int right;
            int highestChild;

            // Get a reference to the left child
            left = GetLeftChild(parent);

            // If the parent has no children,
            // then there's nothing to do
            if (left >= m_count)
                return;

            // Get a reference tothe right child and
            // determine which child has greater priority
            right = GetRightChild(parent);
            highestChild = left;

            if (right < m_count && m_heap[right] > m_heap[left])
                highestChild = right;

            // If the higher priority child has a greater priority than its parent,
            // swap the parent and child nodes. Since the priority of the child has
            // changed, we'll need to continue down the heap to get it to the right place
            if (m_heap[highestChild] > m_heap[parent])
            {
                Swap(parent, highestChild);
                FixHeapDown(highestChild);
            }
        }

        // The indexes represent the positions of the nodes when performing
        // an in-order traversal of the heap. Parent and child nodes can be
        // reached using simple mathematical operations.

        private int GetParent(int index)
        {
            return (index - 1) / 2;
        }

        private int GetLeftChild(int index)
        {
            return (index * 2) + 1;
        }

        private int GetRightChild(int index)
        {
            return (index * 2) + 2;
        }

        // Swaps the nodes at the given indexes.
        private void Swap(int index1, int index2)
        {
            PriorityQueueNode temp = m_heap[index1];
            m_heap[index1] = m_heap[index2];
            m_heap[index2] = temp;
        }

        // Increases the capacity of the array representing the heap.
        private void IncreaseCapacity()
        {
            if (m_heap == EmptyHeap)
                m_heap = new PriorityQueueNode[4];
            else
                Array.Resize(ref m_heap, m_heap.Length * 2);
        }

        // Explicit implementation because items should
        // be enqueued with a specified priority.
        void ICollection<T>.Add(T item)
        {
            Enqueue(int.MinValue, item);
        }

        // Not supported. Allowing insertion would break heap ordering.
        // Fixing the heap would change the index of the inserted item anyway.
        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException("Allowing insertion into the heap would break heap ordering.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly PriorityQueueNode[] EmptyHeap = new PriorityQueueNode[0];

        #endregion
    }
}
