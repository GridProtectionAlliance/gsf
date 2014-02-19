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
using System.Linq;
using System.Threading;

namespace GSF.Collections
{
    /// <summary>
    /// A thread-safe double-buffered queue that allows for low-contention
    /// item processing in single-producer, single-consumer scenarios.
    /// </summary>
    /// <typeparam name="T">Type of items being queued.</typeparam>
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

        // Delegates

        /// <summary>
        /// Defines an item processing function for the <see cref="AsyncQueue{T}"/>.
        /// </summary>
        /// <param name="items">Items to be processed.</param>
        public delegate void ProcessItemsFunctionSignature(IList<T> items);

        // Fields

        // Fields
        private int m_listIndex;
        private List<T>[] m_lists;
        private SpinLock m_swapLock;
        private int m_count;

        private volatile ProcessItemsFunctionSignature m_processItemsFunction;
        private SpinLock m_autoProcessLock;
        private int m_processing;

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
            m_swapLock = new SpinLock();
            m_autoProcessLock = new SpinLock();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets item processing function.
        /// </summary>
        public virtual ProcessItemsFunctionSignature ProcessItemsFunction
        {
            get
            {
                return m_processItemsFunction;
            }
            set
            {
                m_processItemsFunction = value;
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
            bool lockTaken = false;

            try
            {
                m_swapLock.Enter(ref lockTaken);
                m_lists[m_listIndex].AddRange(items);
                m_count = m_lists[m_listIndex].Count;
            }
            finally
            {
                if (lockTaken)
                    m_swapLock.Exit();
            }

            if ((object)m_processItemsFunction != null)
            {
                lockTaken = false;

                try
                {
                    // This lock prevents a race condition that could result during a context switch between the interlocked
                    // operation and the dequeue which could lead to items being left in the queue and not processed. As long
                    // as items are being enqueued, this lock will never contend with lock in the ProcessItem method.
                    m_autoProcessLock.Enter(ref lockTaken);

                    if (Interlocked.CompareExchange(ref m_processing, 1, 0) == 0)
                    {
                        items = Dequeue();

                        if (items.Any())
                            ThreadPool.QueueUserWorkItem(ProcessItems, items);
                        else
                            Interlocked.Exchange(ref m_processing, 0);
                    }
                }
                finally
                {
                    if (lockTaken)
                        m_autoProcessLock.Exit();
                }
            }
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
            bool lockTaken = false;
            int listIndex;

            try
            {
                m_swapLock.Enter(ref lockTaken);

                listIndex = m_listIndex;
                m_listIndex = 1 - m_listIndex;
                m_lists[m_listIndex].Clear();
                m_count = 0;

                return m_lists[listIndex];
            }
            finally
            {
                if (lockTaken)
                    m_swapLock.Exit();
            }
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

            if (lockTaken && (object)m_processItemsFunction != null)
            {
                lockTaken = false;

                try
                {
                    // This lock prevents a race condition that could result during a context switch between the interlocked
                    // operation and the dequeue which could lead to items being left in the queue and not processed. As long
                    // as items are being enqueued, this lock will never contend with lock in the ProcessItem method.
                    m_autoProcessLock.Enter(ref lockTaken);

                    if (Interlocked.CompareExchange(ref m_processing, 1, 0) == 0)
                    {
                        items = Dequeue();

                        if (items.Any())
                            ThreadPool.QueueUserWorkItem(ProcessItems, items);
                        else
                            Interlocked.Exchange(ref m_processing, 0);
                    }
                }
                finally
                {
                    if (lockTaken)
                        m_autoProcessLock.Exit();
                }

                return true;
            }

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
            bool lockTaken = false;
            int listIndex;

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
        }

        // Process next item in the queue
        private void ProcessItems(object state)
        {
            bool lockTaken = false;

            try
            {
                IList<T> items = (IList<T>)state;
                ProcessItemsFunctionSignature processItemsFunction = m_processItemsFunction;

                if ((object)processItemsFunction != null)
                {
                    // Attempt to process current set of items
                    processItemsFunction(items);

                    // Attempt to dequeue next set of items for processing
                    items = Dequeue();

                    if (items.Any())
                    {
                        ThreadPool.QueueUserWorkItem(ProcessItems, items);
                    }
                    else
                    {
                        // We need to check for items in the queue again inside a lock in case there were items added
                        // during a context switch before the lock was entered. This lock should rarely contend with
                        // lock in the Enqueue method; if you enqueue frequently the lock will never be taken since
                        // items will be dequeued outside the lock in the code above.
                        try
                        {
                            m_autoProcessLock.Enter(ref lockTaken);
                            items = Dequeue();

                            if (items.Any())
                                ThreadPool.QueueUserWorkItem(ProcessItems, items);
                            else
                                Interlocked.Exchange(ref m_processing, 0);
                        }
                        finally
                        {
                            if (lockTaken)
                                m_autoProcessLock.Exit();
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Exception occurred while processing item: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// Raises the base class <see cref="ProcessException"/> event.
        /// </summary>
        /// <remarks>
        /// Derived classes cannot raise events of their base classes, so we expose event wrapper methods to accomodate as needed.
        /// </remarks>
        /// <param name="ex"><see cref="Exception"/> to be passed to ProcessException.</param>
        protected virtual void OnProcessException(Exception ex)
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
