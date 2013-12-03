//******************************************************************************************************
//  AsyncQueue.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  11/08/2012 - J. Ritchie Carroll / Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace GSF.Collections
{
    /// <summary>
    /// Creates a fast, light-weight asynchronous processing queue with very low contention.
    /// </summary>
    /// <typeparam name="T">Type of items to process.</typeparam>
    public class AsyncQueue<T> : IEnumerable<T>
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
        /// <param name="item">Item to be processed.</param>
        public delegate void ProcessItemFunctionSignature(T item);

        // Fields
        private ProcessItemFunctionSignature m_processItemFunction;
        private readonly ConcurrentQueue<T> m_asyncQueue;
        private SpinLock m_dequeueLock;
        private int m_processing;
        private volatile bool m_enabled;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AsyncQueue{T}"/>.
        /// </summary>
        public AsyncQueue()
        {
            m_asyncQueue = new ConcurrentQueue<T>();
            m_dequeueLock = new SpinLock();
            m_enabled = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets item processing function.
        /// </summary>
        public virtual ProcessItemFunctionSignature ProcessItemFunction
        {
            get
            {
                return m_processItemFunction;
            }
            set
            {
                m_processItemFunction = value;
            }
        }

        /// <summary>
        /// Gets the total number of items currently in the queue.
        /// </summary>
        public int Count
        {
            get
            {
                return m_asyncQueue.Count;
            }
        }

        /// <summary>
        /// Gets or sets flag that enables or disables processing.
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                bool lockTaken = false;
                T item;

                try
                {
                    m_dequeueLock.Enter(ref lockTaken);

                    if (m_enabled && !value)
                    {
                        // If we are currently enabled and want to disable, make sure processing flag is set to 0
                        m_enabled = false;
                    }
                    else if (!m_enabled && value)
                    {
                        // If we are currently disabled and want to enable, kick off queue processing if needed
                        m_enabled = true;

                        if (Interlocked.CompareExchange(ref m_processing, 1, 0) == 0)
                        {
                            if (m_asyncQueue.TryDequeue(out item))
                                ThreadPool.QueueUserWorkItem(ProcessItem, item);
                            else
                                Interlocked.Exchange(ref m_processing, 0);
                        }
                    }
                }
                finally
                {
                    if (lockTaken)
                        m_dequeueLock.Exit();
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="AsyncQueue{T}"/>.
        /// </summary>
        /// <returns>An enumerator for the contents of the <see cref="AsyncQueue{T}"/>.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return m_asyncQueue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_asyncQueue).GetEnumerator();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Enqueues an item for processing.
        /// </summary>
        /// <param name="item">Item to be queued for processing.</param>
        public virtual void Enqueue(T item)
        {
            if ((object)m_processItemFunction == null)
                throw new NullReferenceException("No item processing function has been assigned - cannot enqueue item for processing.");

            bool lockTaken = false;

            // Queue item for processing
            m_asyncQueue.Enqueue(item);

            if (m_enabled)
            {
                // This lock prevents a race condition that could result during a context switch between the interlocked
                // operation and the dequeue which could lead to items being left in the queue and not processed. As long
                // as items are being enqueued, this lock will never contend with lock in the ProcessItem method.
                try
                {
                    m_dequeueLock.Enter(ref lockTaken);

                    if (Interlocked.CompareExchange(ref m_processing, 1, 0) == 0)
                    {
                        if (m_asyncQueue.TryDequeue(out item))
                            ThreadPool.QueueUserWorkItem(ProcessItem, item);
                        else
                            Interlocked.Exchange(ref m_processing, 0);
                    }
                }
                finally
                {
                    if (lockTaken)
                        m_dequeueLock.Exit();
                }
            }
        }

        // Process next item in the queue
        private void ProcessItem(object state)
        {
            bool lockTaken = false;

            try
            {
                T item = (T)state;

                // Attempt to process current item
                m_processItemFunction(item);

                // Continue with processing next item so long as we're still enabled
                if (!m_enabled)
                {
                    try
                    {
                        m_dequeueLock.Enter(ref lockTaken);

                        if (!m_enabled)
                        {
                            Interlocked.Exchange(ref m_processing, 0);
                            return;
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                            m_dequeueLock.Exit();
                    }
                }

                // Attempt to dequeue next item for processing
                if (m_asyncQueue.TryDequeue(out item))
                {
                    ThreadPool.QueueUserWorkItem(ProcessItem, item);
                }
                else
                {
                    // We need to check for items in the queue again inside a lock in case there were items added
                    // during a context switch before the lock was entered. This lock should rarely contend with
                    // lock in the Enqueue method; if you enqueue frequently the lock will never be taken since
                    // items will be dequeued outside the lock in the code above.
                    lockTaken = false;

                    try
                    {
                        m_dequeueLock.Enter(ref lockTaken);

                        if (m_asyncQueue.TryDequeue(out item))
                            ThreadPool.QueueUserWorkItem(ProcessItem, item);
                        else
                            Interlocked.Exchange(ref m_processing, 0);
                    }
                    finally
                    {
                        if (lockTaken)
                            m_dequeueLock.Exit();
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
    }
}
