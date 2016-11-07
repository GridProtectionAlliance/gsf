//******************************************************************************************************
//  ManagedThreads.cs - Gbtc
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
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Maintains a reference to all managed threads
    /// </summary>
    public static class ManagedThreads
    {
        private static readonly LinkedList<ManagedThread> s_queuedThreads;
        private static readonly LinkedList<ManagedThread> s_activeThreads;

        static ManagedThreads()
        {
            s_queuedThreads = new LinkedList<ManagedThread>();
            s_activeThreads = new LinkedList<ManagedThread>();
        }

        /// <summary>
        /// Add an item to the active thread list
        /// </summary>
        /// <remarks>
        /// Typically only used by standard threads when user calls "Start"
        /// </remarks>
        internal static void Add(ManagedThread item)
        {
            // Standard threads are simply added to the active thread list when started
            lock (s_queuedThreads)
            {
                item.Status = ThreadStatus.Started;
                s_activeThreads.AddLast(item);
            }
        }

        /// <summary>
        /// Remove completed thread from active thread list
        /// </summary>
        internal static void Remove(ManagedThread item)
        {
            lock (s_queuedThreads)
            {
                s_activeThreads.Remove(item);
            }
        }

        /// <summary>
        /// Queue thread for processing
        /// </summary>
        /// <remarks>
        /// Typically only used by queued threads to add work items to the queue
        /// </remarks>
        internal static void Queue(ManagedThread item)
        {
            lock (s_queuedThreads)
            {
                s_queuedThreads.AddLast(item);
            }
        }

        /// <summary>
        /// Removes first item from the queue and transfers the item to the active thread list
        /// </summary>
        /// <returns>Next item to be processed</returns>
        internal static ManagedThread Pop()
        {
            ManagedThread item = null;

            // Transfer next queued thread to the active thread list
            lock (s_queuedThreads)
            {
                if (s_queuedThreads.Count > 0)
                {
                    item = s_queuedThreads.First.Value;
                    s_queuedThreads.RemoveFirst();
                }

                if ((object)item != null)
                {
                    // Capture current thread (this is owned by .NET ThreadPool)
                    item.Thread = Thread.CurrentThread;
                    item.Status = ThreadStatus.Started;
                    s_activeThreads.AddLast(item);
                }
            }

            return item;
        }

        /// <summary>
        /// Returns a descriptive status of all queued and active mananged threads
        /// </summary>
        public static string ActiveThreadStatus
        {
            get
            {
                StringBuilder status = new StringBuilder();
                ManagedThread[] items = QueuedThreads;
                int index = 0;

                // Managed Thread Count: 1
                //
                // Thread 1 - Completed in 25 seconds
                //      Type: Standard Thread
                //      Name: GSF.Service.CalculatedMeasurementInitialization.Initialize()

                status.AppendFormat("Managed Thread Count: {0}{1}", items.Length, Environment.NewLine);
                status.AppendLine();

                foreach (ManagedThread item in items)
                {
                    index++;
                    status.AppendFormat("Thread {0} - {1}{2}", index, ThreadStatusText(item), Environment.NewLine);
                    status.AppendFormat("     Type: {0}{1}", Enum.GetName(typeof(ThreadType), item.Type), Environment.NewLine);
                    status.AppendFormat("     Name: {0}{1}", item.Name, Environment.NewLine);
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        private static string ThreadStatusText(ManagedThread item)
        {
            string runtime = item.RunTime.ToString();

            switch (item.Status)
            {
                case ThreadStatus.Unstarted:
                    return "Not Started";
                case ThreadStatus.Queued:
                    return "Queued";
                case ThreadStatus.Executing:
                    return "Executing for " + runtime;
                case ThreadStatus.Completed:
                    return "Completed in " + runtime;
                case ThreadStatus.Aborted:
                    return "Aborted, ran for " + runtime;
                default:
                    return "Status Unknown";
            }
        }

        /// <summary>
        /// Returns a copy of the currently queued and active threads
        /// </summary>
        public static ManagedThread[] QueuedThreads
        {
            get
            {
                List<ManagedThread> threads = new List<ManagedThread>();

                lock (s_queuedThreads)
                {
                    threads.AddRange(s_queuedThreads);
                    threads.AddRange(s_activeThreads);
                }

                return threads.ToArray();
            }
        }

        /// <summary>
        /// Removes a queued thread from thread pool if still queued, if allowAbort is True
        /// aborts the thread if executing (standard or queued)
        /// </summary>
        /// <param name="item">Thread to cancel</param>
        /// <param name="allowAbort">Set to True to abort thread if executing</param>
        /// <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted.</param>
        public static void Cancel(ManagedThread item, bool allowAbort, object stateInfo)
        {
            if ((object)item == null)
                throw new ArgumentNullException(nameof(item));

            LinkedListNode<ManagedThread> node;

            lock (s_queuedThreads)
            {
                // Change thread status to aborted
                item.Status = ThreadStatus.Aborted;

                // See if item is still queued for execution in thread pool
                node = s_queuedThreads.Find(item);

                // Handle abort or dequeue
                if ((object)node == null)
                {
                    if (allowAbort)
                    {
                        // Started items may be aborted, even if running in thread pool
                        if ((object)stateInfo == null)
                            item.Thread.Abort();
                        else
                            item.Thread.Abort(stateInfo);
                    }
                }
                else
                {
                    // Remove item from queue if queued thread has yet to start
                    s_queuedThreads.Remove(node);
                }
            }
        }
    }
}