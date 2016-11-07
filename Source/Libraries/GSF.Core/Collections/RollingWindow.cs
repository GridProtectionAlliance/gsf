//******************************************************************************************************
//  RollingWindow.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  02/02/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using GSF.NumericalAnalysis;

namespace GSF.Collections
{
    /// <summary>
    /// Represents a rolling window of data with FIFO semantics that will
    /// automatically truncate the data when the window size is exceeded.
    /// </summary>
    /// <typeparam name="T">The type of objects to be stored in the rolling window.</typeparam>
    public class RollingWindow<T> : IList<T>
    {
        #region [ Members ]

        // Fields
        private int m_windowSize;
        private T[] m_window;
        private int m_start;
        private int m_count;

        private int m_version;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RollingWindow{T}"/> class.
        /// </summary>
        /// <param name="windowSize">The size of the window maintained by the collection.</param>
        public RollingWindow(int windowSize)
        {
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize), "windowSize must be at least 1");

            m_windowSize = windowSize;
            m_window = new T[windowSize];
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the number of elements contained in the <see cref="RollingWindow{T}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return m_count;
            }
        }

        /// <summary>
        /// Gets the size of the window maintained by the <see cref="RollingWindow{T}"/>.
        /// </summary>
        public int WindowSize
        {
            get
            {
                return m_windowSize;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="RollingWindow{T}"/> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="RollingWindow{T}"/>.</exception>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= m_count)
                    throw new ArgumentOutOfRangeException(nameof(index), "index must be greater than zero and less than the size of the collection");

                return m_window[WrapIndex(m_start + index)];
            }
            set
            {
                if (index < 0 || index >= m_count)
                    throw new ArgumentOutOfRangeException(nameof(index), "index must be greater than zero and less than the size of the collection");

                m_window[WrapIndex(m_start + index)] = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds an item to the <see cref="RollingWindow{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="RollingWindow{T}"/>.</param>
        public void Add(T item)
        {
            if (m_count < m_windowSize)
                m_count++;
            else
                m_start = WrapIndex(m_start + 1);

            this[m_count - 1] = item;

            m_version++;
        }

        /// <summary>
        /// Inserts an item to the <see cref="RollingWindow{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="RollingWindow{T}"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="RollingWindow{T}"/>.</exception>
        /// <exception cref="InvalidOperationException">Rolling window is full (<see cref="Count"/> = <see cref="WindowSize"/>.</exception>
        public void Insert(int index, T item)
        {
            if (index < 0 || index > m_count)
                throw new ArgumentOutOfRangeException(nameof(index), "index must be greater than zero and less than or equal to the size of the collection");

            if (m_count >= m_windowSize)
                throw new InvalidOperationException("Unable to insert item; rolling window is full");

            // Increase count first to
            // prevent out of range indexes
            m_count++;

            if (index <= m_windowSize / 2)
            {
                // Shift left to make room
                m_start = WrapIndex(m_start - 1);

                for (int i = 0; i < index; i++)
                    this[i] = this[i + 1];
            }
            else
            {
                // Shift right to make room
                for (int i = m_count - 1; i > index; i--)
                    this[i] = this[i - 1];
            }

            // Insert the item and
            // increment the version
            this[index] = item;
            m_version++;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="RollingWindow{T}"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="RollingWindow{T}"/>; otherwise, false.
        /// This method also returns false if <paramref name="item"/> is not found in the original <see cref="RollingWindow{T}"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Remove(T item)
        {
            int index = IndexOf(item);

            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the <see cref="RollingWindow{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="RollingWindow{T}"/>.</exception>
        public void RemoveAt(int index)
        {
            if (index <= m_windowSize / 2)
            {
                // Shift right to fill space
                for (int i = index; i > 0; i--)
                    this[i] = this[i - 1];

                this[0] = default(T);
                m_start++;
            }
            else
            {
                // Shift left to fill space
                for (int i = index; i < m_count - 1; i++)
                    this[i] = this[i + 1];

                this[m_count - 1] = default(T);
            }

            m_count--;
            m_version++;
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="RollingWindow{T}"/>.
        /// </summary>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="RollingWindow{T}"/>.</param>
        public int IndexOf(T item)
        {
            int i = 0;

            foreach (T obj in this)
            {
                if (Equals(obj, item))
                    return i;

                i++;
            }

            return -1;
        }

        /// <summary>
        /// Determines whether the <see cref="RollingWindow{T}"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="RollingWindow{T}"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="RollingWindow{T}"/>.</param>
        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        /// <summary>
        /// Removes all items from the <see cref="RollingWindow{T}"/>.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < m_windowSize; i++)
                m_window[i] = default(T);

            m_count = 0;
        }

        /// <summary>
        /// Copies the elements of the <see cref="RollingWindow{T}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="RollingWindow{T}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source <see cref="RollingWindow{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            int i;

            if ((object)array == null)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "arrayIndex must be greater than zero");

            if (m_count > array.Length - arrayIndex)
                throw new ArgumentException("Not enough available space in array to copy elements from rolling window");

            i = 0;

            foreach (T item in this)
            {
                array[i] = item;
                i++;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            int version = m_version;
            int count = 0;

            for (int i = m_start; i < m_windowSize; i++)
            {
                if (version != m_version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute");

                if (count >= m_count)
                    break;

                count++;

                yield return m_window[i];
            }

            for (int i = 0; i < m_start; i++)
            {
                if (version != m_version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute");

                if (count >= m_count)
                    break;

                count++;

                yield return m_window[i];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int WrapIndex(int index)
        {
            return (int)Euclidean.Mod(index, m_windowSize);
        }

        #endregion
    }
}
