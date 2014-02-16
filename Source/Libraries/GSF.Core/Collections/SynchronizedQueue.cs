//******************************************************************************************************
//  SynchronizedQueue.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  2/15/2014 - Steven E. Chisholm
//       Generated original version of source code. 
//     
//*****************************************************************************************************

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GSF.Collections
{
    /// <summary>
    /// Implements a circular buffer that is synchronized but allows bulk operations. This is similiar 
    /// to a ConcurrentQueue except bulk operations can be done at one time. 
    /// </summary>
    /// <typeparam name="T">The type to make the elements.</typeparam>
    /// <remarks>
    /// The synchronization method is via exclusive locking. There may not perform any faster than a 
    /// concurrent queue unless bulk operations are done.
    /// </remarks>
    public class SynchronizedQueue<T> : IProducerConsumerCollection<T>
    {
        #region [ Members ]

        private object m_syncRoot;

        /// <summary>
        /// Contains the array of objects
        /// </summary>
        private T[] m_items;

        /// <summary>
        /// Contains the head pointer of the circular buffer.
        /// </summary>
        private int m_head;

        /// <summary>
        /// Contains the tail of the circular buffer
        /// </summary>
        private int m_tail;

        /// <summary>
        /// The number of items in the buffer
        /// </summary>
        volatile private int m_count;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new ContinuousQueue
        /// </summary>
        /// <param name="capacity"></param>
        public SynchronizedQueue(int capacity = 16)
        {
            m_syncRoot = new object();
            m_head = 0;
            m_tail = 0;
            m_count = 0;
            InternalSetCapacity(capacity);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public int Count
        {
            get
            {
                return m_count;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object SyncRoot
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the current capacity of the Queue
        /// </summary>
        public int Capacity
        {
            get
            {
                return m_items.Length;
            }
        }

        /// <summary>
        /// Gets if the queue does not have any items in it.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Count == 0;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds an item to the end of the queue.
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            lock (m_syncRoot)
            {
                InternalEnqueue(item);
            }
        }

        /// <summary>
        /// Adds items to the end of the queue.
        /// </summary>
        /// <param name="items"></param>
        public void Enqueue(IEnumerable<T> items)
        {
            lock (m_syncRoot)
            {
                InternalEnqueue(items);
            }
        }

        /// <summary>
        /// Removes an item from the front of the queue
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            lock (m_syncRoot)
            {
                return InternalDequeue();
            }
        }

        /// <summary>
        /// Removes all items from the queue
        /// </summary>
        /// <returns></returns>
        public T[] DequeueAll()
        {
            lock (m_syncRoot)
            {
                return InternalDequeueAll();
            }
        }

        void InternalSetCapacity(int capacity)
        {
            capacity = Math.Max(capacity, Count);
            T[] items = new T[capacity];

            InternalCopyTo(items);
            m_items = items;
            m_tail = 0;
            m_head = Count;
            if (m_head >= capacity)
                m_head -= capacity;
        }

        T InternalDequeue()
        {
            if (Count == 0)
                throw new Exception("Queue is empty");
            T rv = m_items[m_tail];
            m_items[m_tail] = default(T);
            m_count--;
            m_tail++;
            if (m_tail == Capacity)
                m_tail -= Capacity;
            return rv;
        }

        T[] InternalDequeueAll()
        {
            T[] items = InternalToArray();
            m_head = 0;
            m_tail = 0;
            m_count = 0;
            return items;
        }

        void InternalEnqueue(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (Count == Capacity)
                    InternalSetCapacity(Count * 2);
                m_items[m_head] = item;
                m_count++;
                m_head++;
                if (m_head == Capacity)
                    m_head -= Capacity;
            }
            
        }
    
        void InternalEnqueue(T item)
        {
            if (Count == Capacity)
                InternalSetCapacity(Count * 2);
            m_items[m_head] = item;
            m_count++;
            m_head++;
            if (m_head == Capacity)
                m_head -= Capacity;
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing. </param><param name="index">The zero-based index in <paramref name="array"/> at which copying begins. </param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero. </exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>.-or-The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception><filterpriority>2</filterpriority>
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)ToArray()).CopyTo(array, index);
        }


        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1"/> to an <see cref="T:System.Array"/>, starting at a specified index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1"/>. The array must have zero-based indexing.</param><param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is a null reference (Nothing in Visual Basic).</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception><exception cref="T:System.ArgumentException"><paramref name="index"/> is equal to or greater than the length of the <paramref name="array"/> -or- The number of elements in the collection is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>. </exception>
        public void CopyTo(T[] array, int index)
        {
            ToArray().CopyTo(array, index);
        }

        /// <summary>
        /// Attempts to add an object to the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if the object was added successfully; otherwise, false.
        /// </returns>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1"/>.</param><exception cref="T:System.ArgumentException">The <paramref name="item"/> was invalid for this collection.</exception>
        public bool TryAdd(T item)
        {
            lock (m_syncRoot)
            {
                InternalEnqueue(item);
                return true;
            }
        }

        /// <summary>
        /// Attempts to remove and return an object from the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if an object was removed and returned successfully; otherwise, false.
        /// </returns>
        /// <param name="item">When this method returns, if the object was removed and returned successfully, <paramref name="item"/> contains the removed object. If no object was available to be removed, the value is unspecified.</param>

        public bool TryDequeue(out T item)
        {
            return TryTake(out item);
        }

        /// <summary>
        /// Attempts to remove and return an object from the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if an object was removed and returned successfully; otherwise, false.
        /// </returns>
        /// <param name="item">When this method returns, if the object was removed and returned successfully, <paramref name="item"/> contains the removed object. If no object was available to be removed, the value is unspecified.</param>
        public bool TryTake(out T item)
        {
            lock (m_syncRoot)
            {
                if (Count > 0)
                {
                    item = InternalDequeue();
                    return true;
                }
                item = default(T);
                return false;
            }
        }

        /// <summary>
        /// Copies the elements contained in the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1"/> to a new array.
        /// </summary>
        /// <returns>
        /// A new array containing the elements copied from the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1"/>.
        /// </returns>
        public T[] ToArray()
        {
            lock (m_syncRoot)
            {
                return InternalToArray();
            }
        }

        T[] InternalToArray()
        {
            T[] items = new T[Count];
            InternalCopyTo(items);
            return items;
        }

        void InternalCopyTo(T[] items)
        {
            if (Count > 0)
            {
                if (m_head > m_tail)
                {
                    Array.Copy(m_items, m_tail, items, 0, Count);
                }
                else
                {
                    int remainingAtEnd = m_items.Length - m_tail;
                    Array.Copy(m_items, m_tail, items, 0, remainingAtEnd);
                    Array.Copy(m_items, 0, items, remainingAtEnd, m_head);
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)ToArray()).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


    }
}