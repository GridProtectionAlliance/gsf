//******************************************************************************************************
//  ActionExtensions.cs - Gbtc
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
//  02/02/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable AccessToModifiedClosure
// ReSharper disable PossibleNullReferenceException

using System;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Defines extension methods for actions.
    /// </summary>
    public static class ActionExtensions
    {
        // Cancellation token to cancel delayed operations.
        private class DelayCancellationToken : ICancellationToken
        {
            #region [ Members ]

            // Fields
            private const int Idle = 0;
            private const int Busy = 1;
            private const int Disposed = 2;

            private readonly ManualResetEvent WaitObj;
            private int m_state;

            #endregion

            #region [ Constructors ]

            public DelayCancellationToken(ManualResetEvent waitObj)
            {
                WaitObj = waitObj;
            }

            #endregion

            #region [ Properties ]

            public bool IsCancelled
            {
                get
                {
                    if (Interlocked.CompareExchange(ref m_state, 0, 0) != Idle)
                        return true;

                    try
                    {
                        return WaitObj.WaitOne(0);
                    }
                    catch
                    {
                        return true;
                    }
                }
            }

            #endregion

            #region [ Methods ]

            public bool Cancel()
            {
                // If the token is not idle, that means that either a Cancel
                // operation is in progress or the token has been disposed
                if (Interlocked.CompareExchange(ref m_state, Busy, Idle) != Idle)
                    return false;

                try
                {
                    // Determine whether the wait
                    // handle has already been set
                    if (WaitObj.WaitOne(0))
                        return false;

                    // Set the wait handle and return a value indicating
                    // that the token was not previously cancelled
                    WaitObj.Set();
                    return true;
                }
                finally
                {
                    // Set the state back to idle and dispose of the wait
                    // handle if Dispose() was called while the token was busy
                    if (Interlocked.CompareExchange(ref m_state, Idle, Busy) == Disposed)
                        WaitObj.Dispose();
                }
            }

            public void Dispose()
            {
                // Set state to Disposed, but do not dispose
                // of the wait handle if the token is busy
                if (Interlocked.Exchange(ref m_state, Disposed) != Idle)
                    return;

                // Set the wait handle
                // and dispose of it
                WaitObj.Set();
                WaitObj.Dispose();
            }

            #endregion
        }

        /// <summary>
        /// Attempts to execute an action and processes exceptions using the given exception handler.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="exceptionHandler">The handler to be called in the event of an error.</param>
        /// <returns>True if the action was executed without errors; false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> or <paramref name="exceptionHandler"/> is null</exception>
        public static bool TryExecute(this Action action, Action<Exception> exceptionHandler)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            if (exceptionHandler is null)
                throw new ArgumentNullException(nameof(exceptionHandler));

            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                exceptionHandler(ex);
                return false;
            }
        }

        /// <summary>
        /// Execute an action on the thread pool after a specified number of milliseconds.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="waitObj">The wait handle to be used to cancel execution.</param>
        /// <param name="delay">The amount of time to wait before execution, in milliseconds.</param>
        public static void DelayAndExecute(this Action action, WaitHandle waitObj, int delay)
        {
            object waitHandleLock = new();
            RegisteredWaitHandle waitHandle = null;

            void callback(object state, bool timeout)
            {
                if (Interlocked.Exchange(ref waitHandleLock, null) is null)
                    waitHandle.Unregister(null);

                if (!timeout)
                    return;

                action();
            }

            waitHandle = ThreadPool.RegisterWaitForSingleObject(waitObj, callback, null, delay, true);

            if (Interlocked.Exchange(ref waitHandleLock, null) is null)
                waitHandle.Unregister(null);
        }

        /// <summary>
        /// Execute an action on the thread pool after a specified number of milliseconds.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="delay">The amount of time to wait before execution, in milliseconds.</param>
        /// <returns>A cancellation token that can be used to cancel the operation.</returns>
        public static ICancellationToken DelayAndExecute(this Action action, int delay)
        {
            object waitHandleLock = new();
            ManualResetEvent waitObj = new(false);
            DelayCancellationToken cancellationToken = new(waitObj);
            RegisteredWaitHandle waitHandle = null;

            void callback(object state, bool timeout)
            {
                // Even if the callback timed out, another thread may cancel
                // the cancellation token before we are able to dispose of it
                // so we explicitly cancel the token in order to be sure
                timeout = timeout && cancellationToken.Cancel();

                // Both the callback thread and the caller thread will
                // attempt to set the wait handle lock to null, and the
                // last one to do so has to unregister and dispose
                if (Interlocked.Exchange(ref waitHandleLock, null) is null)
                {
                    waitHandle.Unregister(null);
                    cancellationToken.Dispose();
                }

                // If we didn't time out, then the action
                // was cancelled by another thread
                if (!timeout)
                    return;

                action();
            }

            waitHandle = ThreadPool.RegisterWaitForSingleObject(waitObj, callback, null, delay, true);

            // Both the callback thread and the caller thread will
            // attempt to set the wait handle lock to null, and the
            // last one to do so has to unregister and dispose
            if (Interlocked.Exchange(ref waitHandleLock, null) is null)
            {
                waitHandle.Unregister(null);
                cancellationToken.Dispose();
            }

            return cancellationToken;
        }
    }
}
