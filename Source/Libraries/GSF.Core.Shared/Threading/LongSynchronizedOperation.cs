//******************************************************************************************************
//  LongSynchronizedOperation.cs - Gbtc
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
    /// Represents a long-running synchronized operation
    /// that cannot run while it is already in progress.
    /// </summary>
    /// <remarks>
    /// The action performed by the <see cref="LongSynchronizedOperation"/> is executed on
    /// its own dedicated thread when running the operation asynchronously. When running on
    /// its own thread, the action is executed in a tight loop until all pending operations
    /// have been completed. This type of synchronized operation should be preferred if
    /// operations may take a long time, block the thread, or put it to sleep. It is also
    /// recommended to prefer this type of operation if the speed of the operation is not
    /// critical or if completion of the operation is critical, such as when saving data
    /// to a file.
    /// </remarks>
    public class LongSynchronizedOperation : SynchronizedOperationBase
    {
        #region [ Members ]

        // Fields
        private bool m_isBackground;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="LongSynchronizedOperation"/> class.
        /// </summary>
        /// <param name="action">The action to be performed during this operation.</param>
        public LongSynchronizedOperation(Action action)
            : base(action)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LongSynchronizedOperation"/> class.
        /// </summary>
        /// <param name="action">The action to be performed during this operation.</param>
        /// <param name="exceptionAction">The action to be performed if an exception is thrown from the action.</param>
        public LongSynchronizedOperation(Action action, Action<Exception> exceptionAction)
            : base(action, exceptionAction)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets whether or not the thread
        /// executing the action is a background thread.
        /// </summary>
        public bool IsBackground
        {
            get
            {
                return m_isBackground;
            }
            set
            {
                m_isBackground = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Executes the action on a separate thread.
        /// </summary>
        protected override void ExecuteActionAsync()
        {
            Thread actionThread = new Thread(() =>
            {
                while (ExecuteAction())
                {
                }
            });

            actionThread.IsBackground = m_isBackground;
            actionThread.Start();
        }

        #endregion
    }
}
