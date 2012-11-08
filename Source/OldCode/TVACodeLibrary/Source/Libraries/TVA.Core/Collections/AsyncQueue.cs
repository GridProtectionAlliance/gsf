using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace TVA.Collections
{
    /// <summary>
    /// Creates a fast, light-weight asynchronous processing queue with very low contention.
    /// </summary>
    /// <typeparam name="T">Type of items to process.</typeparam>
    public class AsyncQueue<T> : IEnumerable<T>
    {
        #region [ Members ]

        // Delegates

        /// <summary>
        /// Defines an item processing function for the <see cref="AsyncQueue{T}"/>.
        /// </summary>
        /// <param name="item">Item to be processed.</param>
        public delegate void ProcessItemFunctionSignature(T item);

        // Fields
        private readonly ConcurrentQueue<T> m_asyncQueue;
        private SpinLock m_asyncLock;
        private int m_processing;
        private ProcessItemFunctionSignature m_processItemFunction;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AsyncQueue{T}"/>.
        /// </summary>
        public AsyncQueue()
        {
            m_asyncQueue = new ConcurrentQueue<T>();
            m_asyncLock = new SpinLock();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets item processing function.
        /// </summary>
        public ProcessItemFunctionSignature ProcessItemFunction
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Enqueues an item for processing.
        /// </summary>
        /// <param name="item">Item to be queued for processing.</param>
        public void Enqueue(T item)
        {
            if ((object)m_processItemFunction == null)
                throw new NullReferenceException("No item processing function has been assigned - cannot enqueue item for processing.");

            bool lockTaken = false;

            // Queue item for processing
            m_asyncQueue.Enqueue(item);

            // This lock prevents a race condition that could result during a context switch between the interlocked
            // operation and the dequeue which could lead to items being left in the queue and not processed.
            try
            {
                m_asyncLock.Enter(ref lockTaken);

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
                    m_asyncLock.Exit();
            }
        }

        // Process next item in the queue
        private void ProcessItem(object state)
        {
            T item = (T)state;

            m_processItemFunction(item);

            // Attempt to dequeue next item for processing
            if (m_asyncQueue.TryDequeue(out item))
            {
                ThreadPool.QueueUserWorkItem(ProcessItem, item);
            }
            else
            {
                // We need to check for items in the queue again inside a lock in case there were
                // items added during a context switch before the lock was entered. This lock should
                // rarely contend with lock in the Enqueue method; if you enqueue frequently the
                // lock will never be taken since items will be dequeued.
                bool lockTaken = false;

                try
                {
                    m_asyncLock.Enter(ref lockTaken);

                    if (m_asyncQueue.TryDequeue(out item))
                        ThreadPool.QueueUserWorkItem(ProcessItem, item);
                    else
                        Interlocked.Exchange(ref m_processing, 0);
                }
                finally
                {
                    if (lockTaken)
                        m_asyncLock.Exit();
                }
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return m_asyncQueue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_asyncQueue).GetEnumerator();
        }

        #endregion
    }
}
