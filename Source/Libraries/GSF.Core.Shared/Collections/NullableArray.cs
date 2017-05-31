//******************************************************************************************************
//  NullableArray.cs - Gbtc
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

using System;

namespace GSF.Collections
{
    /// <summary>
    /// Creates an array of a struct type that uses bitmask for designating a null value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NullableArray<T>
        where T : struct
    {
        private int[] m_notNullBits;
        private T[] m_items;

        /// <summary>
        /// Creates a <see cref="NullableArray{T}"/>
        /// </summary>
        public NullableArray()
        {
            m_notNullBits = new int[1];
            m_items = new T[32];
        }

        /// <summary>
        /// Adds the provided item to an indexed position of the array, auto-growing the array
        /// if needed.
        /// </summary>
        /// <param name="index">the zero based index</param>
        /// <param name="item"></param>
        public void Add(int index, T item)
        {
            while (index >= m_items.Length)
                Grow();
            m_items[index] = item;
            m_notNullBits[index >> 5] |= 1 << (index & 31);
        }

        /// <summary>
        /// Attempts to get the provided item from the array if it has been added.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGet(int index, out T item)
        {
            if (index >= m_items.Length || ((m_notNullBits[index >> 5] >> (index & 31)) & 1) == 0)
            {
                item = default(T);
                return false;
            }
            item = m_items[index];
            return true;
        }


        private void Grow()
        {
            T[] items = new T[m_items.Length * 2];
            int[] notNullBits = new int[m_notNullBits.Length * 2];

            m_items.CopyTo(items, 0);
            m_notNullBits.CopyTo(notNullBits, 0);

            m_items = items;
            m_notNullBits = notNullBits;
        }

        /// <summary>
        /// Clears the array by setting all of the masked bits to false.
        /// </summary>
        public void Clear()
        {
            Array.Clear(m_notNullBits, 0, m_notNullBits.Length);
        }
    }
}
