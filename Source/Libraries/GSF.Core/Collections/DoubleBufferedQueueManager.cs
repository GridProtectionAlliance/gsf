//******************************************************************************************************
//  DoubleBufferedQueueManager.cs - Gbtc
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
//  02/15/2014 - Stephen C. Wills
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
    /// A producer for a <see cref="DoubleBufferedQueue{T}"/> which can
    /// only be used to provide items to the queue for consumption.
    /// </summary>
    /// <typeparam name="T">The type of the items produced to the queue.</typeparam>
    public class DoubleBufferedQueueProducer<T> : IDisposable
    {
        #region [ Members ]

        // Fields
        private DoubleBufferedQueueManager<T> m_manager;
        private DoubleBufferedQueue<T> m_queue;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DoubleBufferedQueueProducer{T}"/> class.
        /// </summary>
        /// <param name="manager">The <see cref="DoubleBufferedQueueManager{T}"/> that created this producer.</param>
        /// <param name="queue">The <see cref="DoubleBufferedQueue{T}"/> that this producer will be producing to.</param>
        internal DoubleBufferedQueueProducer(DoubleBufferedQueueManager<T> manager, DoubleBufferedQueue<T> queue)
        {
            if ((object)manager == null)
                throw new ArgumentNullException(nameof(manager));

            if ((object)queue == null)
                throw new ArgumentNullException(nameof(queue));

            m_manager = manager;
            m_queue = queue;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="DoubleBufferedQueueProducer{T}"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~DoubleBufferedQueueProducer()
        {
            Dispose(false);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Produces a collection of items to be processed by the consumer.
        /// </summary>
        /// <param name="items">The collection of items to be enqueued.</param>
        public void Produce(IEnumerable<T> items)
        {
            DoubleBufferedQueue<T> queue = m_queue;

            if ((object)queue != null)
            {
                m_queue.Enqueue(items);
                m_manager.SignalItemHandler();
            }
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="DoubleBufferedQueueProducer{T}"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DoubleBufferedQueueProducer{T}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    DoubleBufferedQueue<T> queue = Interlocked.Exchange(ref m_queue, null);

                    if ((object)queue != null)
                        m_manager.ReturnQueue(queue);
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Manages queues to reduce contention for a multithreaded, multiple-producer, single-consumer scenario.
    /// </summary>
    /// <remarks>
    /// For best results, each thread that is producing items to the consumer should call
    /// <see cref="GetProducer"/> to receive a producer object that will not contend with
    /// any other producer. The consumer should either provide a handler to process the queued
    /// items or poll the manager by calling <see cref="Dequeue"/> (not both!). It is not
    /// safe to use this class with multiple consumer threads.
    /// </remarks>
    /// <typeparam name="T">The types of items to be queued.</typeparam>
    /// <remarks>
    /// It is not safe to use this class with multiple consumer threads.
    /// The list returned by <see cref="Dequeue"/> is not thread-safe and
    /// is reused on each Dequeue operation, so no other thread should
    /// access the list while another thread is calling Dequeue.
    /// </remarks>
    public class DoubleBufferedQueueManager<T>
    {
        #region [ Members ]

        // Fields
        private readonly ShortSynchronizedOperation m_itemHandlingOperation;
        private readonly Action m_itemHandler;

        private readonly List<DoubleBufferedQueue<T>> m_queues;
        private readonly List<T> m_dequeuedItems;
        private volatile bool m_itemsLeft;

        private readonly object m_queuesLock;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DoubleBufferedQueueManager{T}"/> class.
        /// </summary>
        public DoubleBufferedQueueManager()
        {
            m_queues = new List<DoubleBufferedQueue<T>>();
            m_dequeuedItems = new List<T>();
            m_queuesLock = new object();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DoubleBufferedQueueManager{T}"/> class.
        /// </summary>
        /// <param name="itemHandler">The method to handle processing of queued items.</param>
        public DoubleBufferedQueueManager(Action itemHandler)
            : this(itemHandler, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DoubleBufferedQueueManager{T}"/> class.
        /// </summary>
        /// <param name="itemHandler">The method to handle processing of queued items.</param>
        public DoubleBufferedQueueManager(Action<IList<T>> itemHandler)
            : this(itemHandler, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DoubleBufferedQueueManager{T}"/> class.
        /// </summary>
        /// <param name="itemHandler">The method to handle processing of queued items.</param>
        /// <param name="exceptionHandler">The method to handle exceptions that occur when processing items.</param>
        public DoubleBufferedQueueManager(Action itemHandler, Action<Exception> exceptionHandler)
            : this()
        {
            m_itemHandlingOperation = new ShortSynchronizedOperation(CallItemHandler, exceptionHandler);
            m_itemHandler = itemHandler;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DoubleBufferedQueueManager{T}"/> class.
        /// </summary>
        /// <param name="itemHandler">The method to handle processing of queued items.</param>
        /// <param name="exceptionHandler">The method to handle exceptions that occur when processing items.</param>
        public DoubleBufferedQueueManager(Action<IList<T>> itemHandler, Action<Exception> exceptionHandler)
            : this()
        {
            m_itemHandlingOperation = new ShortSynchronizedOperation(CallItemHandler, exceptionHandler);

            m_itemHandler = () =>
            {
                IList<T> items = Dequeue();

                if (items.Count > 0)
                    itemHandler(items);
            };
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a flag that indicates whether there are any items left to
        /// be consumed after the last call to <see cref="Dequeue"/>.
        /// </summary>
        public bool ItemsLeft
        {
            get
            {
                return m_itemsLeft;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a producer used to produce items to the consumer of this <see cref="DoubleBufferedQueueManager{T}"/>.
        /// </summary>
        /// <returns>A <see cref="DoubleBufferedQueueProducer{T}"/> used to produce items to the consumer.</returns>
        public DoubleBufferedQueueProducer<T> GetProducer()
        {
            DoubleBufferedQueue<T> queue = new DoubleBufferedQueue<T>();

            lock (m_queuesLock)
            {
                m_queues.Add(queue);
            }

            return new DoubleBufferedQueueProducer<T>(this, queue);
        }

        /// <summary>
        /// Dequeues a list of items produced by the <see cref="DoubleBufferedQueueProducer{T}"/>s.
        /// </summary>
        /// <returns>A list of items to be consumed.</returns>
        public IList<T> Dequeue()
        {
            IList<T> dequeuedItems;

            lock (m_queuesLock)
            {
                m_itemsLeft = false;

                if (m_queues.Count == 0)
                    return EmptyList;

                m_dequeuedItems.Clear();

                foreach (DoubleBufferedQueue<T> queue in m_queues)
                {
                    if (queue.TryDequeue(out dequeuedItems) <= 0)
                    {
                        m_dequeuedItems.AddRange(dequeuedItems);

                        // Clearing the list is not necessary, but is a nice
                        // optimization allowing the garbage collector to
                        // potentially clean up the items before the next
                        // dequeue and to reduce the amount of time spent in
                        // the DoubleBufferedQueue's lock since it won't have
                        // to clear the list during the next dequeue operation
                        if (!dequeuedItems.IsReadOnly)
                            dequeuedItems.Clear();
                    }
                    else
                    {
                        m_itemsLeft = true;
                    }
                }
            }

            return m_dequeuedItems;
        }

        /// <summary>
        /// Runs the operation to process items produced by the <see cref="DoubleBufferedQueueProducer{T}"/>s.
        /// </summary>
        internal void SignalItemHandler()
        {
            if ((object)m_itemHandlingOperation != null)
                m_itemHandlingOperation.RunOnceAsync();
        }

        /// <summary>
        /// Returns a queue to the <see cref="DoubleBufferedQueueManager{T}"/>
        /// so that it can be removed from the list of queues to be consumed.
        /// </summary>
        /// <param name="queue">The queue to be returned.</param>
        internal void ReturnQueue(DoubleBufferedQueue<T> queue)
        {
            int last;
            int index;

            lock (m_queuesLock)
            {
                index = m_queues.IndexOf(queue);

                if (index >= 0)
                {
                    last = m_queues.Count - 1;
                    m_queues[index] = m_queues[last];
                    m_queues.RemoveAt(last);
                }
            }
        }

        private void CallItemHandler()
        {
            m_itemHandler();

            if (m_itemsLeft)
                m_itemHandlingOperation.RunOnceAsync();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly IList<T> EmptyList = new T[0];

        #endregion
    }
}
