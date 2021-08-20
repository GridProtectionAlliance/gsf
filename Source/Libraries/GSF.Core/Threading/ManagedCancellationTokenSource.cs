//******************************************************************************************************
//  ManagedCancellationTokenSource.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
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
//  08/20/2021 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Implements a reference counter for <see cref="System.Threading.CancellationTokenSource"/> to
    /// provide thread safety around <see cref="CancellationTokenSource.Dispose()"/>.
    /// </summary>
    public class ManagedCancellationTokenSource : IDisposable
    {
        #region [ Members ]

        // Nested Types
        private class DisposeWrapper : IDisposable
        {
            private Action DisposeAction { get; }

            public DisposeWrapper(Action disposeAction) =>
                DisposeAction = disposeAction;

            public void Dispose() =>
                DisposeAction();
        }

        // Constants
        private const int NotDisposed = 0;
        private const int Disposing = 1;
        private const int Disposed = 2;

        // Fields
        private int m_referenceCount = 1;
        private int m_disposeState = NotDisposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ManagedCancellationTokenSource"/> class.
        /// </summary>
        public ManagedCancellationTokenSource()
            : this(new CancellationTokenSource())
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManagedCancellationTokenSource"/> class.
        /// </summary>
        /// <param name="cancellationTokenSourceFactory">Factory function for instantiating the underlying <see cref="System.Threading.CancellationTokenSource"/>.</param>
        public ManagedCancellationTokenSource(Func<CancellationTokenSource> cancellationTokenSourceFactory)
            : this(cancellationTokenSourceFactory())
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManagedCancellationTokenSource"/> class.
        /// </summary>
        /// <param name="underlyingTokenSource">The <see cref="System.Threading.CancellationTokenSource"/> to be managed.</param>
        public ManagedCancellationTokenSource(CancellationTokenSource underlyingTokenSource) =>
            CancellationTokenSource = underlyingTokenSource;

        #endregion

        #region [ Properties ]

        private CancellationTokenSource CancellationTokenSource { get; }

        private bool IsCanceled =>
            Interlocked.CompareExchange(ref m_disposeState, 0, 0) != NotDisposed;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Retrieves the <see cref="System.Threading.CancellationToken"/> used to
        /// check the state of cancellation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A disposable object used to control the lifetime of the <paramref name="cancellationToken"/>.</returns>
        public IDisposable RetrieveToken(out System.Threading.CancellationToken cancellationToken)
        {
            if (IsCanceled)
            {
                cancellationToken = new System.Threading.CancellationToken(true);
                return null;
            }

            Interlocked.Increment(ref m_referenceCount);

            if (IsCanceled)
            {
                ReleaseReference();
                cancellationToken = new System.Threading.CancellationToken(true);
                return null;
            }

            cancellationToken = CancellationTokenSource.Token;
            return new DisposeWrapper(ReleaseReference);
        }

        /// <summary>
        /// Cancels the underlying <see cref="System.Threading.CancellationTokenSource"/>
        /// and schedules it for disposal.
        /// </summary>
        public void Dispose()
        {
            int disposeState = Interlocked.CompareExchange(ref m_disposeState, Disposing, NotDisposed);

            if (disposeState == NotDisposed)
            {
                CancellationTokenSource.Cancel();
                ReleaseReference();
            }
        }

        private void ReleaseReference()
        {
            int referenceCount = Interlocked.Decrement(ref m_referenceCount);

            if (referenceCount > 0)
                return;

            int disposeState = Interlocked.CompareExchange(ref m_disposeState, Disposed, Disposing);

            if (disposeState == Disposing)
                CancellationTokenSource.Dispose();
        }

        #endregion
    }
}
