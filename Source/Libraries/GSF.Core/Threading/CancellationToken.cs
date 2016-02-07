//******************************************************************************************************
//  CancellationToken.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  01/29/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Represents a token that can be used
    /// to cancel an asynchronous operation.
    /// </summary>
    public class CancellationToken : ICancellationToken
    {
        #region [ Members ]

        // Fields
        private int m_cancelled;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a value that indicates whether
        /// the operation has been cancelled.
        /// </summary>
        public bool Cancelled
        {
            get
            {
                return Interlocked.CompareExchange(ref m_cancelled, 0, 0) != 0;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Cancels the operation.
        /// </summary>
        /// <returns>True if the operation was previously cancelled; otherwise false.</returns>
        public bool Cancel()
        {
            return Interlocked.Exchange(ref m_cancelled, 1) != 0;
        }

        #endregion
    }
}
