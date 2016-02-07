//******************************************************************************************************
//  LogicalThreadScheduler.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/23/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Manages synchronization of actions by dispatching actions
    /// to logical threads to be processed synchronously.
    /// </summary>
    public class LogicalThreadScheduler
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Triggered when an action managed by this
        /// synchronization manager throws an exception.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> UnhandledException;

        // Fields
        private ConcurrentQueue<LogicalThread> m_logicalThreads;
        private int m_maxThreadCount;
        private int m_threadCount;
        private bool m_useBackgroundThreads;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="LogicalThreadScheduler"/> class.
        /// </summary>
        public LogicalThreadScheduler()
        {
            m_maxThreadCount = Environment.ProcessorCount;
            m_logicalThreads = new ConcurrentQueue<LogicalThread>();
            m_useBackgroundThreads = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the target for the maximum number of physical
        /// threads managed by this synchronization manager at any given time.
        /// </summary>
        public int MaxThreadCount
        {
            get
            {
                return Interlocked.CompareExchange(ref m_maxThreadCount, 0, 0);
            }
            set
            {
                int diff = value - m_maxThreadCount;
                int inactiveThreads = Math.Min(diff, m_logicalThreads.Count);

                Interlocked.Exchange(ref m_maxThreadCount, value);

                for (int i = 0; i < inactiveThreads; i++)
                    ActivatePhysicalThread();
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether the threads in
        /// the scheduler's thread pool should be background threads.
        /// </summary>
        public bool UseBackgroundThreads
        {
            get
            {
                return m_useBackgroundThreads;
            }
            set
            {
                m_useBackgroundThreads = value;
            }
        }

        /// <summary>
        /// Gets the current number of active physical threads.
        /// </summary>
        private int ThreadCount
        {
            get
            {
                return Interlocked.CompareExchange(ref m_threadCount, 0, 0);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a new logical thread whose
        /// execution is managed by this scheduler.
        /// </summary>
        /// <returns>A new logical thread managed by this scheduler.</returns>
        public LogicalThread CreateThread()
        {
            return CreateThread(1);
        }

        /// <summary>
        /// Creates a new logical thread whose
        /// execution is managed by this scheduler.
        /// </summary>
        /// <param name="priorityLevels">The number of levels of priority supported by the logical thread.</param>
        /// <returns>A new logical thread managed by this scheduler.</returns>
        public LogicalThread CreateThread(int priorityLevels)
        {
            return new LogicalThread(this, priorityLevels);
        }

        /// <summary>
        /// Signals the manager when a logical
        /// thread has new actions to be processed.
        /// </summary>
        /// <param name="thread">The thread with new actions to be processed.</param>
        internal void SignalItemHandler(LogicalThread thread)
        {
            if (thread.TryActivate())
            {
                m_logicalThreads.Enqueue(thread);
                ActivatePhysicalThread();
            }
        }

        /// <summary>
        /// Activates a new physical thread if the thread
        /// count has not yet reached its maximum limit.
        /// </summary>
        private void ActivatePhysicalThread()
        {
            int threadCount;
            int newThreadCount;

            threadCount = ThreadCount;

            while (threadCount < Interlocked.CompareExchange(ref m_maxThreadCount, 0, 0))
            {
                newThreadCount = Interlocked.CompareExchange(ref m_threadCount, threadCount + 1, threadCount);

                if (newThreadCount == threadCount)
                {
                    StartNewPhysicalThread();
                    break;
                }

                threadCount = newThreadCount;
            }
        }

        /// <summary>
        /// Starts a new physical thread to
        /// process actions from logical threads.
        /// </summary>
        private void StartNewPhysicalThread()
        {
            Thread thread = new Thread(ProcessLogicalThreads);
            thread.IsBackground = m_useBackgroundThreads;
            thread.Start();
        }

        /// <summary>
        /// Processes the next available task from the least recently
        /// processed logical thread that is available for processing.
        /// </summary>
        private void ProcessLogicalThreads()
        {
            Stopwatch stopwatch;
            LogicalThread thread;
            Action action;

            stopwatch = new Stopwatch();

            while (ThreadCount <= MaxThreadCount && m_logicalThreads.TryDequeue(out thread))
            {
                action = thread.Pull();

                if ((object)action != null)
                {
                    LogicalThread.CurrentThread = thread;

                    stopwatch.Restart();
                    TryExecute(action);
                    stopwatch.Stop();

                    LogicalThread.CurrentThread = null;
                    thread.UpdateStatistics(stopwatch.Elapsed);
                }

                if (thread.HasAction)
                    m_logicalThreads.Enqueue(thread);
                else
                    thread.Deactivate();
            }

            DeactivatePhysicalThread();

            if (!m_logicalThreads.IsEmpty)
                ActivatePhysicalThread();
        }

        /// <summary>
        /// Attempts to execute the given action.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        private void TryExecute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (!TryHandleException(ex))
                    throw;
            }
        }

        /// <summary>
        /// Decrements the thread count upon deactivation of a physical thread.
        /// </summary>
        private void DeactivatePhysicalThread()
        {
            Interlocked.Decrement(ref m_threadCount);
        }

        /// <summary>
        /// Attempts to handle the exception via either the logical thread's
        /// exception handler or the scheduler's exception handler.
        /// </summary>
        /// <param name="unhandledException">The unhandled exception thrown by an action on the logical thread.</param>
        /// <returns>True if the exception could be handled; false otherwise.</returns>
        private bool TryHandleException(Exception unhandledException)
        {
            AggregateException aggregateException;
            StringBuilder message;
            bool handled;

            message = new StringBuilder();
            message.AppendFormat("Logical thread action threw an exception of type {0}: {1}", unhandledException.GetType().FullName, unhandledException.Message);
            aggregateException = new AggregateException(message.ToString(), unhandledException);

            try
            {
                // Attempt to handle the exception via the logical thread's exception handler
                handled = LogicalThread.CurrentThread.OnUnhandledException(unhandledException);
            }
            catch (Exception handlerException)
            {
                // If the handler throws an exception,
                // make a note of it in the exception's exception message
                message.AppendLine();
                message.AppendFormat("Logical thread exception handler threw an exception of type {0}: {1}", handlerException.GetType().FullName, handlerException.Message);
                aggregateException = new AggregateException(message.ToString(), aggregateException.InnerExceptions.Concat(new Exception[] { handlerException }));
                handled = false;
            }

            try
            {
                // If the logical thread's exception handler was not able to handle the exception,
                // attempt to handle the exception via the thread scheduler's exception handler
                Exception ex = (aggregateException.InnerExceptions.Count > 1) ? aggregateException : unhandledException;
                handled = handled || OnUnhandledException(ex);
            }
            catch (Exception handlerException)
            {
                // If the handler throws an exception,
                // make a note of it in the exception's exception message
                message.AppendLine();
                message.AppendFormat("Scheduler exception handler threw an exception of type {0}: {1}", handlerException.GetType().FullName, handlerException.Message);
                aggregateException = new AggregateException(message.ToString(), aggregateException.InnerExceptions.Concat(new Exception[] { handlerException }));
                handled = false;
            }

            if (!handled)
            {
                // If the exception could not be handled by either the
                // logical thread's exception handler or the scheduler's
                // exception handler, throw it as an unhandled exception
                if (aggregateException.InnerExceptions.Count > 1)
                    throw aggregateException;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Raises the <see cref="UnhandledException"/> event.
        /// </summary>
        /// <param name="ex">The unhandled exception.</param>
        /// <returns>True if there are any handlers attached to this event; false otherwise.</returns>
        private bool OnUnhandledException(Exception ex)
        {
            EventHandler<EventArgs<Exception>> unhandledException = UnhandledException;

            if ((object)unhandledException == null)
                return false;

            unhandledException(this, new EventArgs<Exception>(ex));

            return true;
        }

        #endregion
    }
}
