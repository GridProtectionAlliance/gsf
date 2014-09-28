//******************************************************************************************************
//  DoubleBufferedQueue.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  06/13/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using GSF.Threading;

namespace GSF.Collections
{
    /// <summary>
    /// A thread-safe double-buffered queue that allows for low-contention
    /// item processing in single-producer, single-consumer scenarios.
    /// </summary>
    /// <typeparam name="T">Type of items being queued.</typeparam>
    /// <remarks>
    /// It is not safe to use this class with multiple consumer threads.
    /// The <see cref="Dequeue"/> method must be called by one thread at
    /// a time, and the consumer must not access a list returned by Dequeue
    /// after its next call to Dequeue.
    /// </remarks>
    public class DoubleBufferedQueue<T>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Event that is raised if an exception is encountered while attempting to processing an item in the <see cref="AsyncQueue{T}"/>.
        /// </summary>
        /// <remarks>
        /// Processing will not stop for any exceptions thrown by user processing function, but exceptions will be exposed through this event.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        // Fields
        private int m_listIndex;
        private readonly List<T>[] m_lists;
#if MONO
        private readonly object m_swapLock;
#else
        private SpinLock m_swapLock;
#endif
        private int m_count;

        private Action<IList<T>> m_processItemsFunction;
        private readonly ISynchronizedOperation m_processItemsOperation;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DoubleBufferedQueue{T}"/> class.
        /// </summary>
        public DoubleBufferedQueue()
        {
            m_lists = new List<T>[2];
            m_lists[0] = new List<T>();
            m_lists[1] = new List<T>();
#if MONO
            m_swapLock = new object();
#else
            m_swapLock = new SpinLock();
#endif
            m_processItemsOperation = new ShortSynchronizedOperation(TryProcessItems, OnProcessException);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets item processing function.
        /// </summary>
        public Action<IList<T>> ProcessItemsFunction
        {
            get
            {
                return Interlocked.CompareExchange(ref m_processItemsFunction, null, null);
            }
            set
            {
                Interlocked.Exchange(ref m_processItemsFunction, value);
            }
        }

        /// <summary>
        /// Gets the current number of items in the queue.
        /// </summary>
        public int Count
        {
            get
            {
                return Interlocked.CompareExchange(ref m_count, 0, 0);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Enqueues a collection of items into the double-buffered queue.
        /// </summary>
        /// <param name="items">The collection of items to be enqueued.</param>
        public void Enqueue(IEnumerable<T> items)
        {
#if MONO
            lock (m_swapLock)
            {
#else
            bool lockTaken = false;

            try
            {
                m_swapLock.Enter(ref lockTaken);
#endif
                m_lists[m_listIndex].AddRange(items);
                m_count = m_lists[m_listIndex].Count;
            }
#if !MONO
            finally
            {
                if (lockTaken)
                    m_swapLock.Exit();
            }
#endif
            if ((object)ProcessItemsFunction != null)
                m_processItemsOperation.RunOnceAsync();
        }

        /// <summary>
        /// Dequeues a collection of items from the queue.
        /// </summary>
        /// <returns>
        /// A collection of items that have previously been enqueued,
        /// or no items if none have been enqueued since last dequeue.
        /// </returns>
        public IList<T> Dequeue()
        {
            if (m_count == 0)
                return EmptyList;

            int listIndex;
#if MONO
            lock (m_swapLock)
            {
#else
            bool lockTaken = false;

            try
            {
                m_swapLock.Enter(ref lockTaken);
#endif
                listIndex = m_listIndex;
                m_listIndex = 1 - m_listIndex;
                m_lists[m_listIndex].Clear();
                m_count = 0;

                return m_lists[listIndex];
            }
#if !MONO
            finally
            {
                if (lockTaken)
                    m_swapLock.Exit();
            }
#endif
        }

        /// <summary>
        /// Empties the producer's buffer so that the
        /// items can no longer be consumed by the consumer.
        /// </summary>
        public void Clear()
        {
#if MONO
            lock (m_swapLock)
            {
#else
            bool lockTaken = false;

            try
            {
                m_swapLock.Enter(ref lockTaken);
#endif
                m_lists[m_listIndex].Clear();
                m_count = 0;
            }
#if !MONO
            finally
            {
                if (lockTaken)
                    m_swapLock.Exit();
            }
#endif
        }

        /// <summary>
        /// Attempts to enqueue a collection of items into the double-buffered queue.
        /// </summary>
        /// <param name="items">The collection of items to be enqueued.</param>
        /// <returns>
        /// True if the items were successfully enqueued; false otherwise.
        /// </returns>
        public bool TryEnqueue(IEnumerable<T> items)
        {
            bool lockTaken = false;
#if MONO
            if (Monitor.TryEnter(m_swapLock))
            {
                try
                {
                    lockTaken = true;
                    m_lists[m_listIndex].AddRange(items);
                    m_count = m_lists[m_listIndex].Count;
                }
                finally
                {
                    Monitor.Exit(m_swapLock);
                }
            }
#else
            try
            {
                m_swapLock.TryEnter(ref lockTaken);

                if (lockTaken)
                {
                    m_lists[m_listIndex].AddRange(items);
                    m_count = m_lists[m_listIndex].Count;
                }
            }
            finally
            {
                if (lockTaken)
                    m_swapLock.Exit();
            }
#endif
            if (lockTaken && (object)ProcessItemsFunction != null)
                m_processItemsOperation.RunOnceAsync();

            return lockTaken;
        }

        /// <summary>
        /// Attempts to dequeue a collection of items from the queue and
        /// returns the number of items left in the queue after dequeuing.
        /// </summary>
        /// <param name="items">The items that were dequeued.</param>
        /// <returns>
        /// The number of items left in the queue after
        /// dequeuing as many items as possible.
        /// </returns>
        public int TryDequeue(out IList<T> items)
        {
            if (m_count == 0)
            {
                items = EmptyList;
                return 0;
            }

            int listIndex;
#if MONO
            if (Monitor.TryEnter(m_swapLock))
            {
                try
                {
                    listIndex = m_listIndex;
                    m_listIndex = 1 - m_listIndex;
                    m_lists[m_listIndex].Clear();
                    m_count = 0;

                    items = m_lists[listIndex];
                    return 0;
                }
                finally
                {
                    Monitor.Exit(m_swapLock);
                }
            }

            items = EmptyList;
            return m_count;
#else
            bool lockTaken = false;

            try
            {
                m_swapLock.TryEnter(ref lockTaken);

                if (lockTaken)
                {
                    listIndex = m_listIndex;
                    m_listIndex = 1 - m_listIndex;
                    m_lists[m_listIndex].Clear();
                    m_count = 0;

                    items = m_lists[listIndex];
                }
                else
                {
                    items = EmptyList;
                }

                return m_count;
            }
            finally
            {
                if (lockTaken)
                    m_swapLock.Exit();
            }
#endif
        }

        /// <summary>
        /// Attempts to enqueue a collection of items into the double-buffered queue.
        /// </summary>
        /// <returns>
        /// True if the items were successfully enqueued; false otherwise.
        /// </returns>
        public bool TryClear()
        {
            bool lockTaken = false;
#if MONO
            if (Monitor.TryEnter(m_swapLock))
            {
                try
                {
                    lockTaken = true;
                    m_lists[m_listIndex].Clear();
                    m_count = 0;
                }
                finally
                {
                    Monitor.Exit(m_swapLock);
                }
            }
#else
            try
            {
                m_swapLock.TryEnter(ref lockTaken);

                if (lockTaken)
                {
                    m_lists[m_listIndex].Clear();
                    m_count = 0;
                }
            }
            finally
            {
                if (lockTaken)
                    m_swapLock.Exit();
            }
#endif
            return lockTaken;
        }

        // Attempts to dequeue and process items from the queue.
        private void TryProcessItems()
        {
            try
            {
                IList<T> items = Dequeue();

                if (items.Count > 0)
                    m_processItemsFunction(items);
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        // Raises the ProcessException event.
        private void OnProcessException(Exception ex)
        {
            if ((object)ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly IList<T> EmptyList = new T[0];

        #endregion
    }
}
