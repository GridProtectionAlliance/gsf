//******************************************************************************************************
//  ConcurrentIsolatedQueue.cs - Gbtc
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
//  11/28/2016 - Steven E. Chisholm
//       Generated original version of source code based on IsolatedQueue
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Collections
{
    /// <summary>
    /// Provides a buffer of point data where reads are isolated from writes.
    /// This class differs from <see cref="IsolatedQueue{T}"/> because writes can be concurrent. 
    /// Reads on the other hand have to be externally synchronized.
    /// </summary>
    public class ConcurrentIsolatedQueue<T>
        where T : class
    {
        private static readonly DynamicObjectPool<IsolatedNode> Pool = new DynamicObjectPool<IsolatedNode>(() => new IsolatedNode(), 10);

        class IsolatedNode
        {
            /// <summary>
            /// The slots to put stuff.
            /// </summary>
            public readonly T[] Slots;
            /// <summary>
            /// The next node of the linked list.
            /// </summary>
            public IsolatedNode NextNode;
            /// <summary>
            /// The base index of this node
            /// </summary>
            public long PositionIndex;

            /// <summary>
            /// Creates a <see cref="IsolatedNode"/>
            /// </summary>
            public IsolatedNode()
            {
                Slots = new T[NodeSize];
            }
        }

        private const int NodeSize = 128;
        private const int ShiftBits = 7;
        private const int BitMask = 127;

        private IsolatedNode m_currentHead;
        private IsolatedNode m_currentTail;
        private long m_enqueueCount;
        private long m_dequeueCount;

        /// <summary>
        /// Creates an <see cref="IsolatedQueue{T}"/>
        /// </summary>
        public ConcurrentIsolatedQueue()
        {
            m_currentHead = m_currentTail = new IsolatedNode();
            m_currentHead.PositionIndex = 0;
        }

        /// <summary>
        /// The number of elements in the queue. 
        /// </summary>
        /// <returns>
        /// Note: Due to the nature of concurrency. This is a representative number.
        /// and does not mean the exact number of items in the queue unless both Enqueue and Dequeue
        /// are not currently processing.
        /// </returns>
        public int Count
        {
            get
            {
                if (Environment.Is64BitProcess)
                {
                    return Math.Max(0, (int)(m_enqueueCount - m_dequeueCount));
                }
                return Math.Max(0, (int)(Interlocked.Read(ref m_enqueueCount) - Interlocked.Read(ref m_dequeueCount)));
            }
        }

        /// <summary>
        /// Adds the provided item to the <see cref="ConcurrentIsolatedQueue{T}"/>. This method is thread safe.
        /// </summary>
        /// <param name="item">cannot be null</param>
        public void Enqueue(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            //I must acquire the current head before I increment m_enqueueCount
            //This effectively takes a snapshot before the increment operation occurs
            //If I waited until after the increment, then it's possible that multiple 
            //nodes have passed and I'd never be able to find the one needed to
            //assign this item.
            IsolatedNode currentNode = m_currentHead;
            Thread.MemoryBarrier();

            long index = Interlocked.Increment(ref m_enqueueCount) - 1;
            long positionIndex = index >> ShiftBits;
            int arrayIndex = (int)(index & BitMask);

            while (true)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (currentNode.PositionIndex == positionIndex)
                {
                    currentNode.Slots[arrayIndex] = item;
                    return;
                }
                if (currentNode.NextNode == null)
                {
                    Grow(currentNode);
                }
                currentNode = currentNode.NextNode;
            }
        }

        private void Grow(IsolatedNode currentNode)
        {
            //Each thread is in a race to grow the current node. This is to ensure that 
            //Enqueue will never block.
            //The person who wins the race will assign m_current with the node they placed.
            //while it's possible that a race condition could assign m_currentHead out of sequence
            //eventually it will catch back up and there will be little performance penalty in the mean time.

            //Note: There is a very small chance of ConcurrentQueue blocking here. But that's ok.
            IsolatedNode nextNode = Pool.Dequeue();
            nextNode.PositionIndex = currentNode.PositionIndex + 1;
            if (Interlocked.CompareExchange(ref currentNode.NextNode, nextNode, null) == null)
            {
                m_currentHead = nextNode;
            }
            else
            {
                Pool.Enqueue(nextNode);
            }
        }

        /// <summary>
        /// Attempts to dequeue the specified item from the <see cref="ConcurrentIsolatedQueue{T}"/>. This method is NOT thread safe.
        /// </summary>
        /// <param name="item">an output for the item</param>
        /// <returns></returns>
        /// <remarks>
        /// During a race condition, the queue might not be completely empty when TryDequeue returns false. Instead this method returns false 
        /// rather than blocking and waiting on the race condition to satisfy.
        /// </remarks>
        public bool TryDequeue(out T item)
        {
            long positionIndex = m_dequeueCount >> ShiftBits;
            int arrayIndex = (int)(m_dequeueCount & BitMask);

            TryAgain:
            if (m_currentTail.PositionIndex == positionIndex)
            {
                item = m_currentTail.Slots[arrayIndex];
                if (item == null)
                    return false;

                m_currentTail.Slots[arrayIndex] = null;

                if (Environment.Is64BitProcess)
                    m_dequeueCount++;
                else
                    Interlocked.Increment(ref m_dequeueCount);

                return true;
            }
            if (m_currentTail.NextNode == null)
            {
                item = null;
                return false;
            }

            IsolatedNode node = m_currentTail;
            m_currentTail = m_currentTail.NextNode;
            node.NextNode = null;
            Pool.Enqueue(node);

            goto TryAgain;
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