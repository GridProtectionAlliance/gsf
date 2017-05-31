//******************************************************************************************************
//  IndexedArray.cs - Gbtc
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
//  06/29/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace GSF.Collections
{
    /// <summary>
    /// A self growing array of items. This class is thread safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class IndexedArray<T> : IEnumerable<T>
    {
        private T[] m_items;
        private object m_syncRoot;
        private T m_defaultValue;
        private bool m_defaultSet;

        /// <summary>
        /// Creates an <see cref="IndexedArray{T}"/>.
        /// </summary>
        public IndexedArray()
        {
            m_items = new T[32];
            m_syncRoot = new object();
            m_defaultSet = false;
            m_defaultValue = default(T);
        }

        /// <summary>
        /// Creates an <see cref="IndexedArray{T}"/>.
        /// </summary>
        public IndexedArray(T defaultValue)
        {
            m_defaultValue = defaultValue;
            m_items = new T[32];
            m_syncRoot = new object();
            m_defaultSet = true;
            for (int x = 0; x < 32; x++)
            {
                m_items[x] = defaultValue;
            }
        }

        /// <summary>
        /// Gets/Sets the items in this array. Returns the default(t) if the item does not exist, 
        /// or default(t) is the item in the list.
        /// </summary>
        /// <param name="index">the index position to lookup</param>
        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)m_items.Length)
                    return m_defaultValue;

                return m_items[index];
            }
            set
            {
                if ((uint)index >= (uint)m_items.Length)
                {
                    Grow(index);
                }

                m_items[index] = value;
            }
        }

        private void Grow(int index)
        {
            lock (m_syncRoot)
            {
                if (index < 0)
                    throw new Exception("Index cannot be negative.");
                while (index >= m_items.Length)
                {
                    T[] items = new T[m_items.Length * 2];

                    if (m_defaultSet)
                    {
                        for (int x = m_items.Length; x < items.Length; x++)
                        {
                            items[x] = m_defaultValue;
                        }
                    }
                    m_items.CopyTo(items, 0);
                    Thread.MemoryBarrier();
                    m_items = items;
                }
            }
        }

        /// <summary>
        /// Sets all items to their default value
        /// </summary>
        public void Clear()
        {
            lock (m_syncRoot)
            {
                if (m_defaultSet)
                {
                    for (int x = m_items.Length; x < m_items.Length; x++)
                    {
                        m_items[x] = m_defaultValue;
                    }
                }
                else
                {
                    Array.Clear(m_items, 0, m_items.Length);
                }
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)m_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_items.GetEnumerator();
        }
    }
}
