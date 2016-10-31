//******************************************************************************************************
//  DedicatedSynchronizedOperation.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  10/31/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// A synchronized operation that uses its own dedicated thread. 
    /// </summary>
    /// <remarks>
    /// The action performed by the <see cref="DedicatedSynchronizedOperation"/> is executed on
    /// its own dedicated thread when running the operation asynchronously. When running on
    /// its own thread, the action is executed in a tight loop until all pending operations
    /// have been completed. This type of synchronized operation should be preferred if
    /// operations may take a long time, block the thread, or put it to sleep. It is also
    /// recommended to prefer this type of operation if the speed of the operation is not
    /// critical or if completion of the operation is critical, such as when saving data
    /// to a file.
    /// 
    /// 
    /// </remarks>
    public class DedicatedSynchronizedOperation : SynchronizedOperationBase
    {
        //Note: A ScheduledTask will auto-dispose of Foreground threads, 
        //so this task does not have to be disposed.
        #region [ Members ]

        // Fields
        private ScheduledTask m_task;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="LongSynchronizedOperation"/> class.
        /// </summary>
        /// <param name="action">The action to be performed during this operation.</param>
        /// <param name="isBackground">Specifies if this operation will be a background thread.</param>
        public DedicatedSynchronizedOperation(Action action, bool isBackground)
            : base(action)
        {
            if (isBackground)
                m_task = new ScheduledTask(ThreadingMode.DedicatedBackground);
            else
                m_task = new ScheduledTask(ThreadingMode.DedicatedForeground);

            m_task.Running += m_task_Running;

        }

        /// <summary>
        /// Creates a new instance of the <see cref="LongSynchronizedOperation"/> class.
        /// </summary>
        /// <param name="action">The action to be performed during this operation.</param>
        /// <param name="exceptionAction">The action to be performed if an exception is thrown from the action.</param>
        /// <param name="isBackground">Specifies if this operation will be a background thread.</param>
        public DedicatedSynchronizedOperation(Action action, Action<Exception> exceptionAction, bool isBackground)
            : base(action, exceptionAction)
        {
            if (isBackground)
                m_task = new ScheduledTask(ThreadingMode.DedicatedBackground);
            else
                m_task = new ScheduledTask(ThreadingMode.DedicatedForeground);

            m_task.Running += m_task_Running;

        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Executes the action on a separate thread.
        /// </summary>
        protected override void ExecuteActionAsync()
        {
            m_task.Start();
        }

        private void m_task_Running(object sender, EventArgs<ScheduledTaskRunningReason> e)
        {
            while (ExecuteAction())
            {

            }
        }

        #endregion
    }
}
