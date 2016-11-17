//******************************************************************************************************
//  DynamicObjectPool`1.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  11/17/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GSF.Diagnostics;
using GSF.Threading;

namespace GSF.Collections
{
    /// <summary>
    /// Provides a thread safe queue that acts as a buffer pool. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class DynamicObjectPool<T>
        where T : class
    {
        private static readonly LogPublisher Log = Logger.CreatePublisher(typeof(DynamicObjectPool<T>), MessageClass.Component);

        private readonly ScheduledTask m_collection;
        private readonly ConcurrentQueue<T> m_queue;
        private readonly Func<T> m_instanceObject;
        private readonly Queue<int> m_countHistory;
        private readonly int m_targetCount;

        private int m_objectsCreated;

        /// <summary>
        /// Creates a new Resource Queue.
        /// </summary>
        /// <param name="instance">A delegate that will return the necessary queue.</param>
        /// <param name="targetCount">the ideal number of objects that are always pending on the queue.</param>
        public DynamicObjectPool(Func<T> instance, int targetCount)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            m_targetCount = targetCount;
            m_countHistory = new Queue<int>(100);
            m_instanceObject = instance;
            m_queue = new ConcurrentQueue<T>();
            m_collection = new ScheduledTask();
            m_collection.Running += CollectionRunning;
            m_collection.Start(1000);
        }

        private void CollectionRunning(object sender, EventArgs<ScheduledTaskRunningReason> e)
        {
            m_collection.Start(1000);

            m_countHistory.Enqueue(m_queue.Count);
            if (m_countHistory.Count >= 60)
            {
                int objectsCreated = Interlocked.Exchange(ref m_objectsCreated, 0);
                //if there were ever more than the target items in the queue over the past 60 seconds
                //remove some items.
                //However, don't remove items if the pool ever got to 0 and had objects that had to be created.
                int min = m_countHistory.Min();
                m_countHistory.Clear();

                if (objectsCreated > 0)
                {
                    Log.Publish(MessageLevel.Info, "Items Created since last collection cycle.", (objectsCreated).ToString());
                }

                if (min > m_targetCount && objectsCreated == 0)
                {
                    Log.Publish(MessageLevel.Info, "Removing items", (min - m_targetCount).ToString());
                }

                while (min > m_targetCount && objectsCreated == 0)
                {
                    T item;
                    if (m_queue.TryDequeue(out item))
                    {
                        (item as IDisposable)?.Dispose();
                    }
                    else
                    {
                        return;
                    }
                    min--;
                }
            }
        }

        /// <summary>
        /// Removes an item from the queue. If one does not exist, one is created.
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            T item;
            if (m_queue.TryDequeue(out item))
            {
                return item;
            }

            Interlocked.Increment(ref m_objectsCreated);
            return m_instanceObject();
        }

        /// <summary>
        /// Adds an item back to the queue.
        /// </summary>
        /// <param name="resource">The resource to queue.</param>
        public void Enqueue(T resource)
        {
            m_queue.Enqueue(resource);

        }
    }
}