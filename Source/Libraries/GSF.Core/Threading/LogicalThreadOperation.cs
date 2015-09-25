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
//
//******************************************************************************************************

using System;

namespace GSF.Threading
{
    /// <summary>
    /// Synchronized operation that executes on a logical thread.
    /// </summary>
    public class LogicalThreadOperation : SynchronizedOperationBase
    {
        #region [ Members ]

        // Fields
        private LogicalThread m_thread;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="LogicalThreadOperation"/> class.
        /// </summary>
        /// <param name="thread">The thread on which to execute the operation's action.</param>
        /// <param name="action">The action to be executed.</param>
        public LogicalThreadOperation(LogicalThread thread, Action action)
            : this(thread, action, ex => { })
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LogicalThreadOperation"/> class.
        /// </summary>
        /// <param name="thread">The thread on which to execute the operation's action.</param>
        /// <param name="action">The action to be executed.</param>
        /// <param name="exceptionAction">The action to be executed to process unhandled exceptions.</param>
        public LogicalThreadOperation(LogicalThread thread, Action action, Action<Exception> exceptionAction)
            : base(action, exceptionAction)
        {
            m_thread = thread;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Executes the action on a separate thread.
        /// </summary>
        protected override void ExecuteActionAsync()
        {
            m_thread.Push(() =>
            {
                if (ExecuteAction())
                    ExecuteActionAsync();
            });
        }

        #endregion
    }
}
