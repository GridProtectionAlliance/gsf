//******************************************************************************************************
//  ListCollection.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  05/09/2014 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GSF.Collections
{
    /// <summary>
    /// A faster and functionally equivalent implementation of <see cref="Collection{T}"/> 
    /// </summary>
    /// <typeparam name="T">The type of the element in the collection</typeparam>
    /// <remarks>
    /// <para>
    /// <see cref="Collection{T}"/> is based upon an <see cref="IList{T}"/>. This means any simple call
    /// to the class is a function call that cannot be in-lined. This implementation forces the underlying
    /// item to be <see cref="List{T}"/> and shadows many of the methods to call <see cref="List{T}"/> 
    /// instead of <see cref="IList{T}"/>. 
    /// </para>
    /// <para>
    /// Since this class references the same underlying <see cref="List{T}"/> object, it can be 
    /// successfully implemented as a <see cref="ListCollection{T}"/> or casted it its underlying type
    /// <see cref="Collection{T}"/>.
    /// </para>
    /// <para>
    /// Profiling this class yield a ForEach loop and For loop that executes between 
    /// 2-4 times faster than <see cref="Collection{T}"/>. This depends on the number of items in the
    /// list. The fewer the faster. Other operations such as Add/Insert/Remove are closer to 50% faster.
    /// Count is now in-lined (~20 times faster).
    /// </para>
    /// <para>
    /// This performance is negated if accessing this class via the IList interface. When possible, use
    /// only strongly typed names.
    /// </para>
    /// </remarks>
    // Note that explicit re-declaration of interfaces is required here because of shadowed methods
    public class ListCollection<T> : Collection<T>, IList<T>, IList, IReadOnlyList<T>
    {
        #region [ Members ]

        // Fields
        private readonly List<T> m_list;    // Strongly typed reference to Items collection

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a <see cref="ListCollection{T}"/>
        /// </summary>
        /// <param name="list">a list to wrap this class around.</param>
        public ListCollection(List<T> list)
            : base(list)
        {
            m_list = (List<T>)base.Items;
        }

        /// <summary>
        /// Creates a <see cref="ListCollection{T}"/>
        /// </summary>
        public ListCollection()
            : this(new List<T>())
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="ICollection{T}"/>.
        /// </returns>
        public new int Count
        {
            get
            {
                return m_list.Count;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="IList{T}"/>.</exception>
        /// <exception cref="NotSupportedException">The property is set and the <see cref="IList{T}"/> is read-only.</exception>
        public new T this[int index]
        {
            get
            {
                return m_list[index];
            }
            set
            {
                if (index < 0 || index >= m_list.Count)
                    ThrowOutOfRangeException();

                SetItem(index, value);
            }
        }

        /// <summary>
        /// Gets a <see cref="IList{T}"/> wrapper around the <see cref="Collection{T}"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="IList{T}"/> wrapper around the <see cref="Collection{T}"/>.
        /// </returns>
        protected new List<T> Items
        {
            get
            {
                return m_list;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
        /// <exception cref="NotSupportedException">The <see cref="ICollection{T}"/> is read-only.</exception>
        public new void Add(T item)
        {
            InsertItem(m_list.Count, item);
        }

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">The <see cref="ICollection{T}"/> is read-only. </exception>
        public new void Clear()
        {
            ClearItems();
        }

        /// <summary>
        /// Removes all elements from the <see cref="Collection{T}"/>.
        /// </summary>
        protected override void ClearItems()
        {
            m_list.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="ICollection{T}"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="ICollection{T}"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="ICollection{T}"/>.</param>
        public new bool Contains(T item)
        {
            return m_list.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ICollection{T}"/>. 
        /// The <see cref="Array"/> must have zero-based indexing.</param><param name="index">The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source <see cref="ICollection{T}"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>.</exception>
        public new void CopyTo(T[] array, int index)
        {
            m_list.CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="Collection{T}"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> for the <see cref="Collection{T}"/>.
        /// </returns>
        public new List<T>.Enumerator GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="IList{T}"/>.
        /// </summary>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="IList{T}"/>.</param>
        public new int IndexOf(T item)
        {
            return m_list.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="IList{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="IList{T}"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="IList{T}"/>.</exception>
        /// <exception cref="NotSupportedException">The <see cref="IList{T}"/> is read-only.</exception>
        public new void Insert(int index, T item)
        {
            if (index < 0 || index > m_list.Count)
                ThrowOutOfRangeException();

            InsertItem(index, item);
        }

        /// <summary>
        /// Inserts an element into the <see cref="Collection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.-or-<paramref name="index"/> is greater than <see cref="Collection{T}.Count"/>.</exception>
        protected override void InsertItem(int index, T item)
        {
            m_list.Insert(index, item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="ICollection{T}"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="ICollection{T}"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
        /// <exception cref="NotSupportedException">The <see cref="ICollection{T}"/> is read-only.</exception>
        public new bool Remove(T item)
        {
            int index = m_list.IndexOf(item);

            if (index < 0)
                return false;

            RemoveItem(index);

            return true;
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.-or-<paramref name="index"/> is equal to or greater than <see cref="Collection{T}.Count"/>.</exception>
        public new void RemoveAt(int index)
        {
            if (index < 0 || index >= m_list.Count)
                ThrowOutOfRangeException();

            RemoveItem(index);
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.-or-<paramref name="index"/> is equal to or greater than <see cref="Collection{T}.Count"/>.</exception>
        protected override void RemoveItem(int index)
        {
            m_list.RemoveAt(index);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.-or-<paramref name="index"/> is greater than <see cref="Collection{T}.Count"/>.</exception>
        protected override void SetItem(int index, T item)
        {
            m_list[index] = item;
        }

        private static void ThrowOutOfRangeException()
        {
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("index");
        }

        #endregion
    }
}
