//******************************************************************************************************
//  SortedQueue.cs - Gbtc
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
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;

namespace GSF.Collection
{
    /// <summary>
    /// Creates a <see cref="SortedQueue{TKey,TValue}"/>.
    /// This class allows adding items in random order, but dequeuing based upon the sorting of <see ref="TKey"/>
    /// </summary>
    /// <remarks>
    /// Under the surface, it's using a <see cref="SortedList{TKey,TValue}"/> and may not be extremely fast
    /// for large item lists.
    /// </remarks>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SortedQueue<TKey, TValue>
    {
        private SortedList<TKey, TValue> m_list;

        /// <summary>
        /// Creates a <see cref="SortedQueue{TKey,TValue}"/>
        /// </summary>
        public SortedQueue()
        {
            m_list = new SortedList<TKey, TValue>();
        }

        /// <summary>
        /// Gets the first item in the queue. Throws an exception if it does not exist.
        /// </summary>
        public TValue Head => m_list.Values[0];

        /// <summary>
        /// Gets the number of items in the queue;
        /// </summary>
        public int Count => m_list.Count;

        /// <summary>
        /// Attempts to get the provided item.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_list.TryGetValue(key, out value);
        }

        /// <summary>
        /// Queues an item in the list.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Enqueue(TKey key, TValue value)
        {
            m_list.Add(key, value);
        }

        /// <summary>
        /// Removes the first item in the list.
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<TKey, TValue> Dequeue()
        {
            var item = m_list.First();
            m_list.RemoveAt(0);
            return item;
        }

        /// <summary>
        /// An indexer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                return m_list[key];
            }
            set
            {
                m_list[key] = value;
            }
        }
    }
}
