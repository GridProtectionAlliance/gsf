//******************************************************************************************************
//  CompatibleCancellationToken.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/13/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Represents a compatible implementation of the <see cref="ICancellationToken"/> interface
    /// that interoperates with the <see cref="System.Threading.CancellationToken"/> and can be
    /// used to cancel an asynchronous operation.
    /// </summary>
    public class CompatibleCancellationToken : ICancellationToken
    {
        #region [ Members ]

        // Fields
        private readonly CancellationTokenSource m_source;
        private readonly System.Threading.CancellationToken m_token;
        private readonly bool m_sourceIsLocal;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a <see cref="CompatibleCancellationToken"/> based on an existing system cancellation token source.
        /// </summary>
        /// <param name="source">Existing system cancellation token source.</param>
        public CompatibleCancellationToken(CancellationTokenSource source)
        {
            m_source = source;
            m_token = source.Token;

            // Source is not owned, skip finalize
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a <see cref="CompatibleCancellationToken"/> based on an existing system cancellation token.
        /// </summary>
        /// <param name="token">Existing system token.</param>
        /// <remarks>
        /// A <see cref="CompatibleCancellationToken"/> created from an existing system cancellation token
        /// cannot be cancelled since the source for the provided token will not be available.
        /// </remarks>
        public CompatibleCancellationToken(System.Threading.CancellationToken token)
        {
            // Although you could reflect and get cancellation token source, this could be dangerous on 
            // multiple levels. Code in control of a cancellation token source may not be designed to
            // expect cancellation outside of its knowledge, as a result when creating a compatible
            // cancellation token from an existing system cancellation token, any calls to "Cancel" for
            // the compatible cancellation token will simply throw an exception.
            m_token = token;

            // There is no source, skip finalize
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a <see cref="CompatibleCancellationToken"/> based on an existing GSF cancellation token.
        /// </summary>
        /// <param name="token">Existing GSF token.</param>
        public CompatibleCancellationToken(CancellationToken token)
        {
            m_source = new CancellationTokenSource();
            m_token = m_source.Token;
            m_sourceIsLocal = true;

            token.Cancelled += (sender, e) => m_source?.Cancel();
        }

        /// <summary>
        /// Handle disposing of locally allocated resources, if any.
        /// </summary>
        ~CompatibleCancellationToken()
        {
            if (m_sourceIsLocal)
                m_source?.Dispose();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a value that indicates whether the operation has been cancelled.
        /// </summary>
        public virtual bool IsCancelled => m_token.IsCancellationRequested;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Cancels the operation.
        /// </summary>
        /// <returns><c>true</c> if the operation was cancelled; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// The token can only be cancelled if a <see cref="CancellationTokenSource"/> was provided.
        /// </remarks>
        /// <exception cref="InvalidOperationException"><see cref="CancellationTokenSource"/> not available, token cannot be cancelled.</exception>
        public virtual bool Cancel()
        {
            if ((object)m_source == null)
                throw new InvalidOperationException("Token cannot be cancelled.");

            if (m_token.IsCancellationRequested)
                return false;

            m_source.Cancel();
            return true;
        }

        /// <summary>
        /// Gets a <see cref="CancellationTokenSource" /> associated with this <see cref="CompatibleCancellationToken"/>.
        /// </summary>
        /// <returns>A <see cref="CancellationTokenSource" /> associated with this <see cref="CompatibleCancellationToken"/>.</returns>
        /// <remarks>
        /// This function will return <c>null</c> when the <see cref="CompatibleCancellationToken"/> was created
        /// from an existing system cancellation token and not its source or a GSF cancellation token.
        /// </remarks>
        public CancellationTokenSource GetTokenSource()
        {
            return m_source;
        }

        /// <summary>
        /// Gets a <see cref="System.Threading.CancellationToken" /> associated with this <see cref="CompatibleCancellationToken"/>.
        /// </summary>
        /// <returns>A <see cref="System.Threading.CancellationToken" /> associated with this <see cref="CompatibleCancellationToken"/>.</returns>
        public System.Threading.CancellationToken GetToken()
        {
            // Get a new token if source is available, otherwise return existing token
            return m_source?.Token ?? m_token;
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Implicitly converts existing system cancellation token to a <see cref="CompatibleCancellationToken"/>.
        /// </summary>
        /// <param name="token">Existing system cancellation token that is converted to a <see cref="CompatibleCancellationToken"/>.</param>
        /// <returns>A <see cref="CompatibleCancellationToken"/> based on an existing system cancellation token.</returns>
        public static implicit operator CompatibleCancellationToken(System.Threading.CancellationToken token)
        {
            return new CompatibleCancellationToken(token);
        }

        /// <summary>
        /// Implicitly converts existing system cancellation token source to a <see cref="CompatibleCancellationToken"/>.
        /// </summary>
        /// <param name="source">Existing system cancellation token source that is converted to a <see cref="CompatibleCancellationToken"/>.</param>
        /// <returns>A <see cref="CompatibleCancellationToken"/> based on an existing system cancellation token source.</returns>
        public static implicit operator CompatibleCancellationToken(CancellationTokenSource source)
        {
            return new CompatibleCancellationToken(source);
        }

        /// <summary>
        /// Implicitly converts a <see cref="CompatibleCancellationToken"/> to an associated <see cref="System.Threading.CancellationToken"/>.
        /// </summary>
        /// <param name="token">Existing GSF cancellation token that is converted to an associated <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.CancellationToken"/> based on an existing GSF cancellation token.</returns>
        public static implicit operator System.Threading.CancellationToken(CompatibleCancellationToken token)
        {
            return token.GetToken();
        }

        /// <summary>
        /// Implicitly converts a <see cref="CompatibleCancellationToken"/> to an associated <see cref="CancellationTokenSource"/>.
        /// </summary>
        /// <param name="token">Existing GSF cancellation token that is converted to an associated <see cref="CancellationTokenSource"/>.</param>
        /// <returns>A <see cref="CancellationToken"/> based on an existing GSF cancellation token.</returns>
        public static implicit operator CancellationTokenSource(CompatibleCancellationToken token)
        {
            return token.GetTokenSource();
        }
        #endregion
    }
}
