//******************************************************************************************************
//  DoubleBufferedQueue.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/13/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Threading;

namespace GSF.Collections
{
    /// <summary>
    /// A thread-safe double-buffered queue that allows for low-contention
    /// item processing in single-producer, single-consumer scenarios.
    /// </summary>
    /// <typeparam name="T">Type of items being queued.</typeparam>
    public class DoubleBufferedQueue<T>
    {
        #region [ Members ]

        // Fields
        private int m_listIndex;
        private List<T>[] m_lists;
        private SpinLock m_lock;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DoubleBufferedQueue"/> class.
        /// </summary>
        public DoubleBufferedQueue()
        {
            m_lists = new List<T>[2];
            m_lists[0] = new List<T>();
            m_lists[1] = new List<T>();
            m_lock = new SpinLock();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Enqueues a collection of items into the double-buffered queue.
        /// </summary>
        /// <param name="items">The collection of items to be enqueued.</param>
        public void Enqueue(IEnumerable<T> items)
        {
            bool lockTaken = false;

            try
            {
                m_lock.Enter(ref lockTaken);
                m_lists[m_listIndex].AddRange(items);
            }
            finally
            {
                if (lockTaken)
                    m_lock.Exit();
            }
        }

        /// <summary>
        /// Dequeues a collection of items from the queue.
        /// </summary>
        /// <returns>
        /// A collection of items that have previously been enqueued,
        /// or no items if none have been enqueued since last dequeue.
        /// </returns>
        public IEnumerable<T> Dequeue()
        {
            bool lockTaken = false;
            int listIndex;

            try
            {
                m_lock.Enter(ref lockTaken);

                listIndex = m_listIndex;
                m_listIndex = 1 - m_listIndex;
                m_lists[m_listIndex].Clear();

                return m_lists[listIndex];
            }
            finally
            {
                if (lockTaken)
                    m_lock.Exit();
            }
        }

        #endregion
    }
}
