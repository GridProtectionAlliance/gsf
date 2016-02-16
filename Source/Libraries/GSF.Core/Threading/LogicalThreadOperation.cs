//******************************************************************************************************
//  LogicalThreadOperation.cs - Gbtc
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
//  02/16/2016 - J. Ritchie Carroll
//       Corrected priority behavior and added override feature, made explicit RunIfPending optional 
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Synchronized operation that executes on a logical thread.
    /// </summary>
    /// <remarks>
    /// This synchronized operation optionally supports a different usage pattern which will
    /// allow for asynchronous loops and signals to be passed between threads throughout an
    /// operation. The following is an example of how to implement a simple asynchronous loop
    /// between two threads using this class. Note that when used, the pattern requires more
    /// diligence on the user's part for handling exceptions and signaling when the operation
    /// is complete:
    /// 
    /// <code>
    /// LogicalThread thread1 = new LogicalThread();
    /// LogicalThread thread2 = new LogicalThread();
    /// 
    /// // Create logical thread operation with manually controlled call to RunIfPending
    /// LogicalThreadOperation operation = new LogicalThreadOperation(thread1, DoOperation, false);
    /// 
    /// private void DoOperation()
    /// {
    ///     ExecuteOnThread1();
    ///     thread2.Push(() => operation.ExecuteAction(() =>
    ///     {
    ///         ExecuteOnThread2();
    ///         operation.RunIfPending();
    ///     }));
    /// });
    /// </code>
    /// </remarks>
    public class LogicalThreadOperation
    {
        #region [ Members ]

        // Constants
        private const int NotRunning = 0;
        private const int Running = 1;
        private const int Pending = 2;

        // Fields
        private readonly LogicalThread m_thread;
        private readonly Action m_action;
        private readonly bool m_autoRunIfPending;
        private int m_defaultPriority;
        private int m_state;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="LogicalThreadOperation"/> class.
        /// </summary>
        /// <param name="thread">The thread on which to execute the operation's action.</param>
        /// <param name="action">The action to be executed.</param>
        /// <param name="autoRunIfPending">
        /// Set to <c>true</c> to execute <see cref="RunIfPending"/> automatically; otherwise,
        /// set to <c>false</c> for user controlled call timing.
        /// </param>
        public LogicalThreadOperation(LogicalThread thread, Action action, bool autoRunIfPending = true)
            : this(thread, action, 1, autoRunIfPending)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LogicalThreadOperation"/> class.
        /// </summary>
        /// <param name="thread">The thread on which to execute the operation's action.</param>
        /// <param name="action">The action to be executed.</param>
        /// <param name="defaultPriority">The default priority with which the action should be executed on the logical thread.</param>
        /// <param name="autoRunIfPending">
        /// Set to <c>true</c> to execute <see cref="RunIfPending"/> automatically; otherwise, 
        /// set to <c>false</c> for user controlled call timing.
        /// </param>
        /// <exception cref="ArgumentException"><paramref name="defaultPriority"/> is outside the range between 1 and <see cref="LogicalThread.PriorityLevels"/>.</exception>
        public LogicalThreadOperation(LogicalThread thread, Action action, int defaultPriority, bool autoRunIfPending = true)
        {
            if (defaultPriority < 1 || defaultPriority > thread.PriorityLevels)
                throw new ArgumentException($"Logical thread does not support priority level {defaultPriority}. Specify a priority between 1 and {thread.PriorityLevels}.", nameof(defaultPriority));

            m_thread = thread;
            m_action = action;
            m_defaultPriority = defaultPriority;
            m_autoRunIfPending = autoRunIfPending;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a value to indicate whether the operation is currently executing actions.
        /// </summary>
        public bool IsRunning => Interlocked.CompareExchange(ref m_state, NotRunning, NotRunning) != NotRunning;

        /// <summary>
        /// Gets a value to indicate whether the operation has an additional operation
        /// that is pending execution after the currently running operation has completed.
        /// </summary>
        public bool IsPending => Interlocked.CompareExchange(ref m_state, NotRunning, NotRunning) == Pending;

        /// <summary>
        /// Gets flag that determines if <see cref="RunIfPending"/> will be called automatically.
        /// </summary>
        public bool AutoRunIfPending => m_autoRunIfPending;

        /// <summary>
        /// Gets or sets default priority for logical thread operation.
        /// </summary>
        /// <remarks>
        /// Updates to default priority will only take effect during next <see cref="LogicalThread.Push(int, Action)"/> call.
        /// </remarks>
        public int DefaultPriority
        {
            get
            {
                return m_defaultPriority;
            }
            set
            {
                if (value < 1 || value > m_thread.PriorityLevels)
                    throw new ArgumentException($"Logical thread does not support priority level {value}. Specify a priority between 1 and {m_thread.PriorityLevels}.", nameof(value));

                m_defaultPriority = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Executes the action on the current thread or marks the operation as
        /// pending if the operation is already running.
        /// </summary>
        /// <param name="priority">Target priority for action, defaults to <see cref="DefaultPriority"/>.</param>
        /// <remarks>
        /// When the operation is marked as pending, it will run again at the next
        /// call to <see cref="RunIfPending"/>. This can be useful if an update to
        /// an object's state has invalidated the operation that is currently running
        /// and will therefore need to be run again.
        /// </remarks>
        public void RunOnce(int? priority = null)
        {
            // if (m_state == NotRunning)
            //     TryRunOnce();
            // else if (m_state == Running)
            //     m_state = Pending;

            if (Interlocked.CompareExchange(ref m_state, Pending, Running) == NotRunning)
                TryRunOnce(priority);
        }

        /// <summary>
        /// Executes the action on another thread or marks the operation as pending
        /// if the operation is already running.
        /// </summary>
        /// <param name="priority">Target priority for action, defaults to <see cref="DefaultPriority"/>.</param>
        /// <remarks>
        /// When the operation is marked as pending, it will run again at the next
        /// call to <see cref="RunIfPending"/>. This can be useful if an update to
        /// an object's state has invalidated the operation that is currently running
        /// and will therefore need to be run again.
        /// </remarks>
        public void RunOnceAsync(int? priority = null)
        {
            // if (m_state == NotRunning)
            //     TryRunOnceAsync();
            // else if (m_state == Running)
            //     m_state = Pending;

            if (Interlocked.CompareExchange(ref m_state, Pending, Running) == NotRunning)
                TryRunOnceAsync(priority);
        }

        /// <summary>
        /// Attempts to execute the action on the current thread.
        /// Does nothing if the operation is already running.
        /// </summary>
        /// <param name="priority">Target priority for action, defaults to <see cref="DefaultPriority"/>.</param>
        public void TryRunOnce(int? priority = null)
        {
            // if (m_state == NotRunning)
            // {
            //     m_state = Running;
            //
            //     if (ExecuteAction())
            //         ExecuteActionAsync();
            // }

            if (Interlocked.CompareExchange(ref m_state, Running, NotRunning) == NotRunning)
                ExecuteAction(m_action, priority);
        }

        /// <summary>
        /// Attempts to execute the action on another thread.
        /// Does nothing if the operation is already running.
        /// </summary>
        /// <param name="priority">Target priority for action, defaults to <see cref="DefaultPriority"/>.</param>
        public void TryRunOnceAsync(int? priority = null)
        {
            // if (m_state == NotRunning)
            // {
            //     m_state = Running;
            //     ExecuteActionAsync();
            // }

            if (Interlocked.CompareExchange(ref m_state, Running, NotRunning) == NotRunning)
                m_thread.Push(priority ?? m_defaultPriority, () => ExecuteAction(m_action, priority));
        }

        /// <summary>
        /// Starts the operation over at the beginning if the operation is pending or sets
        /// the operation state back to not running so it can run again.
        /// </summary>
        /// <param name="priority">Target priority for action, defaults to <see cref="DefaultPriority"/>.</param>
        /// <remarks>
        /// This method must be called at the end of an operation in order to set the state
        /// of the operation back to running or not running so that the operation can run again.
        /// The existence of this method makes this implementation different from other synchronized
        /// operations in that it requires more diligence on the user's part to signal when the
        /// operation is complete. In turn, this allows the user to implement complex operations
        /// that may involve asynchronous loops and signaling patterns that would not be possible
        /// with the <see cref="ISynchronizedOperation"/> interface.
        /// </remarks>
        public void RunIfPending(int? priority = null)
        {
            // if (m_state == Pending)
            // {
            //     m_state = Running;
            //     return true;
            // }
            // else if (m_state == Running)
            // {
            //     m_state = NotRunning;
            // }

            if (Interlocked.CompareExchange(ref m_state, NotRunning, Running) == Pending)
            {
                // There is no race condition here because if m_state is Pending,
                // then it cannot be changed by any other line of code except this one
                Interlocked.Exchange(ref m_state, Running);
                m_thread.Push(priority ?? m_defaultPriority, () => ExecuteAction(m_action, priority));
            }
        }

        /// <summary>
        /// Executes an action once on the current thread.
        /// </summary>
        /// <param name="action"><see cref="Action"/> to run on current thread.</param>
        /// <param name="priority">Target priority for action, defaults to <see cref="DefaultPriority"/>.</param>
        /// <remarks>
        /// This method provides exception handling for the action passed into this
        /// method with a couple of guarantees. The first is that regardless of what
        /// thread is executing the action passed into this method, the exception
        /// will be raised on the thread that the logical operation runs on. The
        /// second is that the RunIfPending method will be called if an exception
        /// does occur in the given action.
        /// </remarks>
        public void ExecuteAction(Action action, int? priority = null)
        {
            try
            {
                action();

                if (m_autoRunIfPending)
                    RunIfPending(priority);
            }
            catch (Exception ex)
            {
                if (LogicalThread.CurrentThread != m_thread)
                {
                    m_thread.Push(priority ?? m_defaultPriority, () =>
                    {
                        string message = $"Exception occurred while executing logical thread operation: {ex.Message}";
                        throw new Exception(message, ex);
                    });
                }

                RunIfPending(priority);

                if (LogicalThread.CurrentThread == m_thread)
                    throw;
            }
        }

        #endregion
    }
}
