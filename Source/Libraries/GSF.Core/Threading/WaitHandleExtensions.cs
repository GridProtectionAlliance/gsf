//******************************************************************************************************
//  WaitHandleExtensions.cs - Gbtc
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
//  12/05/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using GSF.Collections;

namespace GSF.Threading
{
    /// <summary>
    /// Defines extension functions related to manipulation wait handle objects.
    /// </summary>
    public static class WaitHandleExtensions
    {
        /// <summary>
        /// Waits for all the specified <see cref="ManualResetEventSlim"/> elements to receive a signal.
        /// </summary>
        /// <param name="resetEvents">Collection of <see cref="ManualResetEventSlim"/> elements to operate on.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe, if any.</param>
        /// <returns><c>true</c> when every <see cref="ManualResetEventSlim"/> element has received a signal; otherwise the method never returns unless cancelled.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="resetEvents"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Using <see cref="WaitHandle.WaitAll(WaitHandle[])"/> will cause all <see cref="ManualResetEventSlim"/> elements
        /// to be upgraded to a standard <see cref="ManualResetEvent"/>, these overloads allow similar functionality without
        /// incurring unconditional inflation of the underlying <see cref="ManualResetEvent"/>.
        /// </remarks>
        public static bool WaitAll(this IEnumerable<ManualResetEventSlim> resetEvents, CancellationToken cancellationToken = null)
        {
            if ((object)resetEvents == null)
                throw new ArgumentNullException(nameof(resetEvents));

            if ((object)cancellationToken == null)
                return resetEvents.AllParallel(resetEvent => { resetEvent.Wait(); return true; } );

            return resetEvents.WaitAll(new CompatibleCancellationToken(cancellationToken));
        }

        /// <summary>
        /// Waits for all the specified <see cref="ManualResetEventSlim"/> elements to receive a signal.
        /// </summary>
        /// <param name="resetEvents">Collection of <see cref="ManualResetEventSlim"/> elements to operate on.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> to observe.</param>
        /// <returns><c>true</c> when every <see cref="ManualResetEventSlim"/> element has received a signal; otherwise the method never returns unless cancelled.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="resetEvents"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Using <see cref="WaitHandle.WaitAll(WaitHandle[])"/> will cause all <see cref="ManualResetEventSlim"/> elements
        /// to be upgraded to a standard <see cref="ManualResetEvent"/>, these overloads allow similar functionality without
        /// incurring unconditional inflation of the underlying <see cref="ManualResetEvent"/>.
        /// </remarks>
        public static bool WaitAll(this IEnumerable<ManualResetEventSlim> resetEvents, System.Threading.CancellationToken cancellationToken)
        {
            if ((object)resetEvents == null)
                throw new ArgumentNullException(nameof(resetEvents));

            return resetEvents.AllParallel(resetEvent => { resetEvent.Wait(cancellationToken); return !cancellationToken.IsCancellationRequested; });
        }

        /// <summary>
        /// Waits for all the specified <see cref="ManualResetEventSlim"/> elements to receive a signal, using an integer value to specify the maximum time interval,in milliseconds, to wait.
        /// </summary>
        /// <param name="resetEvents">Collection of <see cref="ManualResetEventSlim"/> elements to operate on.</param>
        /// <param name="timeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe, if any.</param>
        /// <returns><c>true</c> when every <see cref="ManualResetEventSlim"/> element has received a signal; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="resetEvents"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Using <see cref="WaitHandle.WaitAll(WaitHandle[])"/> will cause all <see cref="ManualResetEventSlim"/> elements
        /// to be upgraded to a standard <see cref="ManualResetEvent"/>, these overloads allow similar functionality without
        /// incurring unconditional inflation of the underlying <see cref="ManualResetEvent"/>.
        /// </remarks>
        public static bool WaitAll(this IEnumerable<ManualResetEventSlim> resetEvents, int timeout, CancellationToken cancellationToken = null)
        {
            if ((object)resetEvents == null)
                throw new ArgumentNullException(nameof(resetEvents));

            if ((object)cancellationToken == null)
                return resetEvents.AllParallel(resetEvent => resetEvent.Wait(timeout));

            return resetEvents.WaitAll(timeout, new CompatibleCancellationToken(cancellationToken));
        }

        /// <summary>
        /// Waits for all the specified <see cref="ManualResetEventSlim"/> elements to receive a signal, using an integer value to specify the maximum time interval,in milliseconds, to wait.
        /// </summary>
        /// <param name="resetEvents">Collection of <see cref="ManualResetEventSlim"/> elements to operate on.</param>
        /// <param name="timeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> to observe.</param>
        /// <returns><c>true</c> when every <see cref="ManualResetEventSlim"/> element has received a signal; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="resetEvents"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Using <see cref="WaitHandle.WaitAll(WaitHandle[])"/> will cause all <see cref="ManualResetEventSlim"/> elements
        /// to be upgraded to a standard <see cref="ManualResetEvent"/>, these overloads allow similar functionality without
        /// incurring unconditional inflation of the underlying <see cref="ManualResetEvent"/>.
        /// </remarks>
        public static bool WaitAll(this IEnumerable<ManualResetEventSlim> resetEvents, int timeout, System.Threading.CancellationToken cancellationToken)
        {
            if ((object)resetEvents == null)
                throw new ArgumentNullException(nameof(resetEvents));

            return resetEvents.AllParallel(resetEvent => resetEvent.Wait(timeout, cancellationToken));
        }

        /// <summary>
        /// Waits for all the specified <see cref="ManualResetEventSlim"/> elements to receive a signal, using a <see cref="TimeSpan"/> value to specify the maximum time interval to wait.
        /// </summary>
        /// <param name="resetEvents">Collection of <see cref="ManualResetEventSlim"/> elements to operate on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds, to wait indefinitely.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe, if any.</param>
        /// <returns><c>true</c> when every <see cref="ManualResetEventSlim"/> element has received a signal; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="resetEvents"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Using <see cref="WaitHandle.WaitAll(WaitHandle[])"/> will cause all <see cref="ManualResetEventSlim"/> elements
        /// to be upgraded to a standard <see cref="ManualResetEvent"/>, these overloads allow similar functionality without
        /// incurring unconditional inflation of the underlying <see cref="ManualResetEvent"/>.
        /// </remarks>
        public static bool WaitAll(this IEnumerable<ManualResetEventSlim> resetEvents, TimeSpan timeout, CancellationToken cancellationToken = null)
        {
            if ((object)resetEvents == null)
                throw new ArgumentNullException(nameof(resetEvents));

            if ((object)cancellationToken == null)
                return resetEvents.AllParallel(resetEvent => resetEvent.Wait(timeout));

            return resetEvents.WaitAll(timeout, new CompatibleCancellationToken(cancellationToken));
        }

        /// <summary>
        /// Waits for all the specified <see cref="ManualResetEventSlim"/> elements to receive a signal, using a <see cref="TimeSpan"/> value to specify the maximum time interval to wait.
        /// </summary>
        /// <param name="resetEvents">Collection of <see cref="ManualResetEventSlim"/> elements to operate on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds, to wait indefinitely.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> to observe.</param>
        /// <returns><c>true</c> when every <see cref="ManualResetEventSlim"/> element has received a signal; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="resetEvents"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Using <see cref="WaitHandle.WaitAll(WaitHandle[])"/> will cause all <see cref="ManualResetEventSlim"/> elements
        /// to be upgraded to a standard <see cref="ManualResetEvent"/>, these overloads allow similar functionality without
        /// incurring unconditional inflation of the underlying <see cref="ManualResetEvent"/>.
        /// </remarks>
        public static bool WaitAll(this IEnumerable<ManualResetEventSlim> resetEvents, TimeSpan timeout, System.Threading.CancellationToken cancellationToken)
        {
            if ((object)resetEvents == null)
                throw new ArgumentNullException(nameof(resetEvents));

            return resetEvents.AllParallel(resetEvent => resetEvent.Wait(timeout, cancellationToken));
        }
    }
}
