//******************************************************************************************************
//  LogicalThread.cs - Gbtc
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
//  09/24/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Represents a thread of execution to which
    /// actions can be dispatched from other threads.
    /// </summary>
    /// <remarks>
    /// This class provides a simple alternative to synchronization primitives
    /// such as wait handles and locks. Actions dispatched to a logical thread
    /// will be processed synchronously as though it was executed as consecutive
    /// method calls. All such actions can be dispatched from any thread in the
    /// system so method calls coming from multiple threads can be easily
    /// synchronized without locks, loops, wait handles, or timeouts.
    /// </remarks>
    public class LogicalThread
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Handler for unhandled exceptions on the thread.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> UnhandledException;

        // Fields
        private LogicalThreadScheduler m_scheduler;
        private ConcurrentQueue<Action> m_queue;
        private Dictionary<Guid, object> m_threadLocalStorage;
        private int m_isActive;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="LogicalThread"/> class.
        /// </summary>
        public LogicalThread()
            : this(DefaultScheduler)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LogicalThread"/> class.
        /// </summary>
        /// <param name="scheduler">The <see cref="LogicalThreadScheduler"/> that created this thread.</param>
        internal LogicalThread(LogicalThreadScheduler scheduler)
        {
            if ((object)scheduler == null)
                throw new ArgumentNullException("scheduler");

            m_scheduler = scheduler;
            m_queue = new ConcurrentQueue<Action>();
            m_threadLocalStorage = new Dictionary<Guid, object>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a flag that indicates whether the logical
        /// thread has any unprocessed actions left in its queue.
        /// </summary>
        public bool HasAction
        {
            get
            {
                return !m_queue.IsEmpty;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Pushes an action to the logical thread.
        /// </summary>
        /// <param name="action">The action to be executed on this thread.</param>
        public void Push(Action action)
        {
            ConcurrentQueue<Action> queue = m_queue;

            if ((object)queue != null)
            {
                m_queue.Enqueue(action);
                m_scheduler.SignalItemHandler(this);
            }
        }

        /// <summary>
        /// Clears all actions from the logical thread.
        /// </summary>
        public void Clear()
        {
            Action action;

            while (!m_queue.IsEmpty)
                m_queue.TryDequeue(out action);
        }

        /// <summary>
        /// Pulls an action from the logical thread's internal queue to be executed on a physical thread.
        /// </summary>
        /// <returns>An action from the logical thread's internal queue.</returns>
        internal Action Pull()
        {
            Action action;

            return m_queue.TryDequeue(out action)
                ? action
                : null;
        }

        /// <summary>
        /// Activates the thread if it is not already active.
        /// </summary>
        /// <returns>True if this call to TryActivate caused the thread to be active.</returns>
        internal bool TryActivate()
        {
            return Interlocked.CompareExchange(ref m_isActive, 1, 0) == 0;
        }

        /// <summary>
        /// Deactivates the thread.
        /// </summary>
        internal void Deactivate()
        {
            Interlocked.Exchange(ref m_isActive, 0);
        }

        /// <summary>
        /// Returns the thread local object with the given ID.
        /// </summary>
        /// <param name="id">The ID of the thread local object.</param>
        /// <returns>The value of the thread local object with the given ID.</returns>
        internal object GetThreadLocal(Guid id)
        {
            object value;
            m_threadLocalStorage.TryGetValue(id, out value);
            return value;
        }

        /// <summary>
        /// Sets the value of the thread local object with the given ID.
        /// </summary>
        /// <param name="id">The ID of the thread local object.</param>
        /// <param name="value">The new value for the thread local object.</param>
        internal void SetThreadLocal(Guid id, object value)
        {
            if (value != null)
                m_threadLocalStorage[id] = value;
            else
                m_threadLocalStorage.Remove(id);
        }

        /// <summary>
        /// Raises the <see cref="UnhandledException"/> event.
        /// </summary>
        /// <param name="ex">The unhandled exception.</param>
        /// <returns>True if there are any handlers attached to this event; false otherwise.</returns>
        internal bool OnUnhandledException(Exception ex)
        {
            EventHandler<EventArgs<Exception>> unhandledException = UnhandledException;

            if ((object)unhandledException == null)
                return false;

            unhandledException(this, new EventArgs<Exception>(ex));

            return true;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly LogicalThreadScheduler DefaultScheduler = new LogicalThreadScheduler();
        private static readonly ThreadLocal<LogicalThread> LocalThread = new ThreadLocal<LogicalThread>();

        // Static Properties

        /// <summary>
        /// Gets the logical thread that is currently executing.
        /// </summary>
        public static LogicalThread CurrentThread
        {
            get
            {
                return LocalThread.Value;
            }
            internal set
            {
                LocalThread.Value = value;
            }
        }

        #endregion
    }
}
