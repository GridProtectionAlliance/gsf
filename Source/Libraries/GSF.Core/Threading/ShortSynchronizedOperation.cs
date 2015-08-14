//******************************************************************************************************
//  ShortSynchronizedOperation.cs - Gbtc
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
//  03/21/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Represents a short-running synchronized operation
    /// that cannot run while it is already in progress.
    /// </summary>
    /// <remarks>
    /// The action performed by the <see cref="ShortSynchronizedOperation"/> is executed on
    /// the <see cref="ThreadPool"/> when running the operation asynchronously. When the
    /// operation is set to pending, the action is executed in an asynchronous loop on the
    /// thread pool until all pending operations have been completed. Since the action is
    /// executed on the thread pool, it is best if it can be executed quickly, without
    /// blocking the thread or putting it to sleep. If completion of the operation is
    /// critical, such as when saving data to a file, this type of operation should not
    /// be used since thread pool threads are background threads and will not prevent the
    /// program from ending before the operation is complete.
    /// </remarks>
    public class ShortSynchronizedOperation : SynchronizedOperationBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ShortSynchronizedOperation"/> class.
        /// </summary>
        /// <param name="action">The action to be performed during this operation.</param>
        public ShortSynchronizedOperation(Action action)
            : base(action)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ShortSynchronizedOperation"/> class.
        /// </summary>
        /// <param name="action">The action to be performed during this operation.</param>
        /// <param name="exceptionAction">The action to be performed if an exception is thrown from the action.</param>
        public ShortSynchronizedOperation(Action action, Action<Exception> exceptionAction)
            : base(action, exceptionAction)
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Executes the action in an asynchronous loop on
        /// the thread pool, as long as the operation is pending.
        /// </summary>
        protected override void ExecuteActionAsync()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                if (ExecuteAction())
                    ExecuteActionAsync();
            });
        }

        #endregion
    }
}
