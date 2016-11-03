//******************************************************************************************************
//  IsolatedQueue.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  01/04/2013 - Steven E. Chisholm
//       Generated original version of source code. 
//  11/02/2016 - Steven E. Chisholm
//       Simplified implementation to reduce the likelihood of bugs.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GSF.Collections
{
    /// <summary>
    /// Provides a buffer of point data where reads are isolated from writes.
    /// However, reads must be synchronized with other reads and writes must be synchronized with other writes.
    /// </summary>
    public class IsolatedQueue<T>
    {
        /// <summary>
        /// Represents an individual node that allows for items to be added and removed from the 
        /// queue independently and without locks. 
        /// </summary>
        class IsolatedNode
        {
            private readonly int m_lastBlock;
            private volatile int m_tail;
            private volatile int m_head;
            private readonly T[] m_blocks;

            /// <summary>
            /// Creates a <see cref="IsolatedNode"/>
            /// </summary>
            /// <param name="count">the number of items in each node.</param>
            public IsolatedNode(int count)
            {
                m_tail = 0;
                m_head = 0;
                m_blocks = new T[count];
                m_lastBlock = m_blocks.Length;
            }

            /// <summary>
            /// Gets if the current node is out of entries.
            /// </summary>
            public bool DequeueMustMoveToNextNode
            {
                get
                {
                    return m_tail == m_lastBlock;
                }
            }

            /// <summary>
            /// Gets if there are items that can be dequeued
            /// </summary>
            public bool CanDequeue
            {
                get
                {
                    return m_head != m_tail;
                }
            }

            /// <summary>
            /// Gets if this list can be enqueued
            /// </summary>
            public bool CanEnqueue
            {
                get
                {
                    return m_head != m_lastBlock;
                }
            }

            /// <summary>
            /// Adds the following item to the queue. Be sure to check if it is full first.
            /// </summary>
            /// <param name="item"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Enqueue(T item)
            {
                m_blocks[m_head] = item;
                //No memory barior here since .NET 2.0 ensures that writes will not be reordered.
                m_head = m_head + 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Dequeue()
            {
                T item = m_blocks[m_tail];
                m_blocks[m_tail] = default(T);
                //No memory barior here since .NET 2.0 ensures that writes will not be reordered.
                m_tail = m_tail + 1;
                return item;
            }
        }

        private readonly ConcurrentQueue<IsolatedNode> m_blocks;

        private IsolatedNode m_currentHead;
        private IsolatedNode m_currentTail;

        private readonly int m_unitCount;
        private int m_enqueueCount;
        private int m_dequeueCount;

        /// <summary>
        /// Creates an <see cref="IsolatedQueue{T}"/>
        /// </summary>
        public IsolatedQueue()
        {
            m_unitCount = 128;
            m_blocks = new ConcurrentQueue<IsolatedNode>();
        }

        /// <summary>
        /// The number of elements in the queue. 
        /// </summary>
        /// <returns>
        /// Note: Due to the nature of simultaneous access. This is a representative number.
        /// and does not mean the exact number of items in the queue unless both Enqueue and Dequeue
        /// are not currently processing.
        /// </returns>
        public int Count
        {
            get
            {
                int delta = m_enqueueCount - m_dequeueCount;
                if (delta < int.MinValue / 2)
                {
                    return int.MaxValue;
                }
                if (delta > int.MaxValue / 2)
                {
                    return int.MaxValue;
                }
                if (delta < 0)
                {
                    return 0;
                }
                return delta;
            }
        }

        /// <summary>
        /// Addes the provided item to the <see cref="IsolatedQueue{T}"/>.
        /// </summary>
        /// <param name="item"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Enqueue(T item)
        {
            if (m_currentHead != null && m_currentHead.CanEnqueue)
            {
                m_currentHead.Enqueue(item);
                m_enqueueCount++;
                return;
            }
            EnqueueSlower(item);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void EnqueueSlower(T item)
        {
            if (m_currentHead == null || !m_currentHead.CanEnqueue)
            {
                m_currentHead = new IsolatedNode(m_unitCount);
                Thread.MemoryBarrier();
                m_blocks.Enqueue(m_currentHead);
                m_enqueueCount++;
            }
            m_currentHead.Enqueue(item);
        }

        /// <summary>
        /// Attempts to dequeue the specified item from the <see cref="IsolatedQueue{T}"/>
        /// </summary>
        /// <param name="item">an output for the item</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool TryDequeue(out T item)
        {
            if (m_currentTail != null && m_currentTail.CanDequeue)
            {
                item = m_currentTail.Dequeue();
                m_dequeueCount++;
                return true;
            }
            return TryDequeueSlower(out item);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool TryDequeueSlower(out T item)
        {
            if (m_currentTail == null)
            {
                if (!m_blocks.TryDequeue(out m_currentTail))
                {
                    item = default(T);
                    return false;
                }
            }
            else if (m_currentTail.DequeueMustMoveToNextNode)
            {
                if (!m_blocks.TryDequeue(out m_currentTail))
                {
                    item = default(T);
                    return false;
                }
            }
            if (m_currentTail.CanDequeue)
            {
                item = m_currentTail.Dequeue();
                m_dequeueCount++;
                return true;
            }
            item = default(T);
            return false;
        }

        /// <summary>
        /// Adds the provided items to the <see cref="IsolatedQueue{T}"/>.
        /// </summary>
        /// <param name="items">the items to add</param>
        /// <param name="offset">the offset position</param>
        /// <param name="length">the length</param>
        public void Enqueue(T[] items, int offset, int length)
        {
            items.ValidateParameters(offset, length);
            for (int x = 0; x < length; x++)
            {
                Enqueue(items[offset + x]);
            }
        }

        /// <summary>
        /// Dequeues all of the items into the provided array
        /// </summary>
        /// <param name="items">where to put the items</param>
        /// <param name="startingIndex">the starting index</param>
        /// <param name="length">the maximum number of times to store</param>
        /// <returns>the number of items dequeued</returns>
        public int Dequeue(T[] items, int startingIndex, int length)
        {
            items.ValidateParameters(startingIndex, length);
            for (int x = 0; x < length; x++)
            {
                T item;
                if (TryDequeue(out item))
                {
                    items[x] = item;
                }
                else
                {
                    return x;
                }
            }
            return length;
        }

    }
}