//******************************************************************************************************
//  SynchronizedOperation.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  01/29/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Represents an operation that cannot run while it is already in progress.
    /// </summary>
    public class SynchronizedOperation
    {
        #region [ Members ]

        // Fields
        private Action m_action;
        private Action<Exception> m_exceptionAction;
        private int m_operationExecuting;
        private int m_operationPending;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SynchronizedOperation"/> class.
        /// </summary>
        /// <param name="action">The action to be performed during this operation.</param>
        public SynchronizedOperation(Action action)
            : this(action, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SynchronizedOperation"/> class.
        /// </summary>
        /// <param name="action">The action to be performed during this operation.</param>
        /// <param name="exceptionAction">The action to be performed if an exception is thrown from the action.</param>
        public SynchronizedOperation(Action action, Action<Exception> exceptionAction)
        {
            if ((object)action == null)
                throw new ArgumentNullException("action");

            m_action = action;
            m_exceptionAction = exceptionAction;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Executes the action on this thread or marks the
        /// operation as pending if the operation is already running.
        /// </summary>
        /// <remarks>
        /// When the operation is marked as pending, it will run again after the
        /// operation that is currently running has completed. This is useful if
        /// an update has invalidated the operation that is currently running and
        /// will therefore need to be run again.
        /// </remarks>
        public void RunOnce()
        {
            Interlocked.Exchange(ref m_operationPending, 1);
            TryRun();
        }

        /// <summary>
        /// Executes the action on another thread or marks the
        /// operation as pending if the operation is already running.
        /// </summary>
        /// <remarks>
        /// When the operation is marked as pending, it will run again after the
        /// operation that is currently running has completed. This is useful if
        /// an update has invalidated the operation that is currently running and
        /// will therefore need to be run again.
        /// </remarks>
        public void RunOnceAsync()
        {
            ThreadPool.QueueUserWorkItem(state => RunOnce());
        }

        /// <summary>
        /// Attempts to execute the action on this thread.
        /// Does nothing if the operation is already running.
        /// </summary>
        public void TryRun()
        {
            if (Interlocked.CompareExchange(ref m_operationExecuting, 1, 0) == 0)
            {
                Interlocked.Exchange(ref m_operationPending, 0);
                ExecuteAction();
                Interlocked.Exchange(ref m_operationExecuting, 0);

                if (Interlocked.CompareExchange(ref m_operationPending, 0, 1) == 1)
                    ThreadPool.QueueUserWorkItem(state => RunOnce());
            }
        }

        /// <summary>
        /// Attempts to execute the action on another thread.
        /// Does nothing if the operation is already running.
        /// </summary>
        public void TryRunAsync()
        {
            ThreadPool.QueueUserWorkItem(state => TryRun());
        }

        private void ExecuteAction()
        {
            try
            {
                m_action();
            }
            catch (Exception ex)
            {
                try
                {
                    if ((object)m_exceptionAction != null)
                        m_exceptionAction(ex);
                }
                catch
                {
                }
            }
        }

        #endregion
    }
}
