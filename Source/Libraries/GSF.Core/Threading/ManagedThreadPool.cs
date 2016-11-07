//******************************************************************************************************
//  ManagedThreadPool.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  01/25/2008 - J. Ritchie Carroll
//       Initial version of source generated.
//  09/11/2008 - J. Ritchie Carroll
//       Converted to C#.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Defines a managed thread pool
    /// </summary>
    /// <remarks>
    /// This class works like the normal thread pool but provides the benefit of automatic tracking
    /// of queued threads through the ManagedThreads collection, returns a reference to the
    /// queued thread with the ability to dequeue and/or abort, total thread runtime and the
    /// ability to run the queued thread in an alternate execution context
    /// </remarks>
    public static class ManagedThreadPool
    {
        /// <summary>
        /// Queues a work item for processing on the managed thread pool
        /// </summary>
        /// <param name="callback">A WaitCallback representing the method to execute.</param>
        /// <returns>Reference to queued thread</returns>
        /// <remarks>
        /// This differs from the normal thread pool QueueUserWorkItem function in that it does
        /// not return a success value determing if item was queued, but rather a reference to
        /// to the managed thread that was actually placed on the queue.
        /// </remarks>
        public static ManagedThread QueueUserWorkItem(ThreadStart callback)
        {
            if ((object)callback == null)
                throw (new ArgumentNullException(nameof(callback)));

            ManagedThread item = new ManagedThread(ThreadType.QueuedThread, callback, null, null);

            ManagedThreads.Queue(item);

            ThreadPool.QueueUserWorkItem(HandleItem);

            return item;
        }

        /// <summary>
        /// Queues a work item for processing on the managed thread pool
        /// </summary>
        /// <param name="callback">A WaitCallback representing the method to execute.</param>
        /// <returns>Reference to queued thread</returns>
        /// <remarks>
        /// This differs from the normal thread pool QueueUserWorkItem function in that it does
        /// not return a success value determing if item was queued, but rather a reference to
        /// to the managed thread that was actually placed on the queue.
        /// </remarks>
        public static ManagedThread QueueUserWorkItem(ParameterizedThreadStart callback)
        {
            return QueueUserWorkItem(callback, null);
        }

        /// <summary>
        /// Queues a work item for processing on the managed thread pool
        /// </summary>
        /// <param name="callback">A WaitCallback representing the method to execute.</param>
        /// <param name="state">An object containing data to be used by the method.</param>
        /// <returns>Reference to queued thread</returns>
        /// <remarks>
        /// This differs from the normal thread pool QueueUserWorkItem function in that it does
        /// not return a success value determing if item was queued, but rather a reference to
        /// to the managed thread that was actually placed on the queue.
        /// </remarks>
        public static ManagedThread QueueUserWorkItem(ParameterizedThreadStart callback, object state)
        {
            if ((object)callback == null)
                throw (new ArgumentNullException(nameof(callback)));

            ManagedThread item = new ManagedThread(ThreadType.QueuedThread, callback, state, null);

            ManagedThreads.Queue(item);

            ThreadPool.QueueUserWorkItem(HandleItem);

            return item;
        }

        /// <summary>
        /// Queues a work item for processing on the managed thread pool
        /// </summary>
        /// <param name="callback">A WaitCallback representing the method to execute.</param>
        /// <param name="ctx">Alternate execution context in which to run the thread.</param>
        /// <returns>Reference to queued thread</returns>
        /// <remarks>
        /// This differs from the normal thread pool QueueUserWorkItem function in that it does
        /// not return a success value determing if item was queued, but rather a reference to
        /// to the managed thread that was actually placed on the queue.
        /// </remarks>
        public static ManagedThread QueueUserWorkItem(ContextCallback callback, ExecutionContext ctx)
        {
            return QueueUserWorkItem(callback, null, ctx);
        }

        /// <summary>
        /// Queues a work item for processing on the managed thread pool
        /// </summary>
        /// <param name="callback">A WaitCallback representing the method to execute.</param>
        /// <param name="state">An object containing data to be used by the method.</param>
        /// <param name="ctx">Alternate execution context in which to run the thread.</param>
        /// <returns>Reference to queued thread</returns>
        /// <remarks>
        /// This differs from the normal thread pool QueueUserWorkItem function in that it does
        /// not return a success value determing if item was queued, but rather a reference to
        /// to the managed thread that was actually placed on the queue.
        /// </remarks>
        public static ManagedThread QueueUserWorkItem(ContextCallback callback, object state, ExecutionContext ctx)
        {
            if ((object)callback == null)
                throw (new ArgumentNullException(nameof(callback)));

            ManagedThread item = new ManagedThread(ThreadType.QueuedThread, callback, state, ctx);

            ManagedThreads.Queue(item);

            ThreadPool.QueueUserWorkItem(HandleItem);

            return item;
        }

        private static void HandleItem(object state)
        {
            // Get next queued item
            ManagedThread item = ManagedThreads.Pop();

            // Execute callback...
            if ((object)item != null)
                item.HandleItem();
        }
    }
}