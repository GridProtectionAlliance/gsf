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
//  09/13/2016 - J. Ritchie Carroll
//       Updated to include cancellation event and implicit conversion to system cancellation tokens.
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Represents a token that can be used to cancel an asynchronous operation.
    /// </summary>
    public class CancellationToken : ICancellationToken
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Event that will be raised when this <see cref="CancellationToken"/> is cancelled.
        /// </summary>
        public event EventHandler Cancelled;

        // Fields
        private int m_cancelled;
        
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a value that indicates whether the operation has been cancelled.
        /// </summary>
        public bool IsCancelled => Interlocked.CompareExchange(ref m_cancelled, 0, 0) != 0;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Cancels the operation.
        /// </summary>
        /// <returns><c>true</c> if the operation was cancelled; otherwise <c>false</c>.</returns>
        public bool Cancel()
        {
            bool result = Interlocked.Exchange(ref m_cancelled, 1) == 0;

            if (result && (object)Cancelled != null)
                Cancelled(this, EventArgs.Empty);

            return result;
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Implicitly converts a GSF cancellation token to an associated <see cref="System.Threading.CancellationToken"/>.
        /// </summary>
        /// <param name="token">Existing GSF cancellation token that is converted to an associated <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.CancellationToken"/> based on an existing GSF cancellation token.</returns>
        public static implicit operator System.Threading.CancellationToken(CancellationToken token)
        {
            return new CompatibleCancellationToken(token).GetToken();
        }

        /// <summary>
        /// Implicitly converts a GSF cancellation token to an associated <see cref="CancellationTokenSource"/>.
        /// </summary>
        /// <param name="token">Existing GSF cancellation token that is converted to an associated <see cref="CancellationTokenSource"/>.</param>
        /// <returns>A <see cref="CancellationToken"/> based on an existing GSF cancellation token.</returns>
        public static implicit operator CancellationTokenSource(CancellationToken token)
        {
            return new CompatibleCancellationToken(token).GetTokenSource();
        }

        #endregion
    }
}
