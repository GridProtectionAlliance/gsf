//******************************************************************************************************
//  MixedSynchronizedOperation.cs - Gbtc
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
//  10/08/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Indicates modes of execution for the <see cref="MixedSynchronizedOperation"/>.
    /// </summary>
    public enum AsynchronousExecutionMode
    {
        /// <summary>
        /// Executes asynchronous operations on the thread pool
        /// (same as <see cref="ShortSynchronizedOperation"/>).
        /// </summary>
        Short = 0,

        /// <summary>
        /// Executes asynchronous operations on a dedicated background thread
        /// (same as <see cref="LongSynchronizedOperation"/>).
        /// </summary>
        Long = 1
    }

    /// <summary>
    /// Represents an operation that cannot run while it is already in progress.
    /// </summary>
    /// <remarks>
    /// The behavior of asynchronous executions will depend on the value of the
    /// <see cref="AsynchronousExecutionMode"/> property. When using the short asynchronous
    /// execution mode, refer to the <see cref="ShortSynchronizedOperation"/> class for a
    /// description of the behavior of this class. When using the long asynchronous execution
    /// mode, refer to the <see cref="LongSynchronizedOperation"/> class. Actions executed
    /// by this class will always be executed on a background thread, even when using the
    /// long asynchronous execution mode.
    /// </remarks>
    public class MixedSynchronizedOperation : SynchronizedOperationBase
    {
        #region [ Members ]

        // Fields
        private int m_asynchronousExecutionMode;
        private AsynchronousExecutionMode m_currentExecutionMode;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MixedSynchronizedOperation"/> class.
        /// </summary>
        /// <param name="action">The action to be performed during this operation.</param>
        public MixedSynchronizedOperation(Action action)
            : base(action)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MixedSynchronizedOperation"/> class.
        /// </summary>
        /// <param name="action">The action to be performed during this operation.</param>
        /// <param name="exceptionAction">The action to be performed if an exception is thrown from the action.</param>
        public MixedSynchronizedOperation(Action action, Action<Exception> exceptionAction)
            : base(action, exceptionAction)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the mode of execution used to execute the action asynchronously.
        /// </summary>
        public AsynchronousExecutionMode AsynchronousExecutionMode
        {
            get
            {
                return (AsynchronousExecutionMode)Interlocked.CompareExchange(ref m_asynchronousExecutionMode, 0, 0);
            }
            set
            {
                Interlocked.Exchange(ref m_asynchronousExecutionMode, (int)value);
            }
        }

        /// <summary>
        /// Gets the execution mode of the currently executing action.
        /// </summary>
        public AsynchronousExecutionMode CurrentExecutionMode
        {
            get
            {
                return m_currentExecutionMode;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Executes the action on a separate thread.
        /// </summary>
        protected override void ExecuteActionAsync()
        {
            if (AsynchronousExecutionMode == AsynchronousExecutionMode.Short)
                ExecuteAsyncOnThreadPool();
            else
                ExecuteAsyncOnDedicatedThread();
        }

        private void ExecuteAsyncOnThreadPool()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                m_currentExecutionMode = AsynchronousExecutionMode.Short;

                if (ExecuteAction())
                    ExecuteActionAsync();
            });
        }

        private void ExecuteAsyncOnDedicatedThread()
        {
            Thread t = new Thread(() =>
            {
                m_currentExecutionMode = AsynchronousExecutionMode.Long;

                while (ExecuteAction())
                {
                    if (AsynchronousExecutionMode != AsynchronousExecutionMode.Long)
                    {
                        ExecuteActionAsync();
                        break;
                    }
                }
            });

            t.IsBackground = true;
            t.Start();
        }

        #endregion
    }
}
