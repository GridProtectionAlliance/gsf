//******************************************************************************************************
//  OrderedSet.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  12/04/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;

namespace GSF.Collections
{
    /// <summary>
    /// Represents an ordered set of data.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    [Serializable]
    public class OrderedSet<T> : IOrderedSet<T>, ISerializable, IDeserializationCallback
    {
        #region [ Members ]

        // Fields
        private readonly OrderedDictionary<T, object> m_hashSet;    // Just interested in keys - all values will be null

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedSet{T}"/> class that is empty and uses the default equality
        /// comparer for the set type.
        /// </summary>
        public OrderedSet()
        {
            m_hashSet = new OrderedDictionary<T, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedSet{T}"/> class that uses the default equality comparer for
        /// the set type, contains elements copied from the specified <paramref name="collection"/>, and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
        public OrderedSet(IEnumerable<T> collection)
        {
            m_hashSet = new OrderedDictionary<T, object>(collection.ToDictionary(t => t, v => (object)null));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedSet{T}"/> class that is empty and uses the specified
        /// equality <paramref name="comparer"/> for the set type.
        /// </summary>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or <c>null</c> to
        /// use the default <see cref="IEqualityComparer{T}"/> implementation for the set type.
        /// </param>
        public OrderedSet(IEqualityComparer<T> comparer)
        {
            m_hashSet = new OrderedDictionary<T, object>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedSet{T}"/> class that uses the specified equality
        /// <paramref name="comparer"/> for the set type, contains elements copied from the specified <paramref name="collection"/>,
        /// and has sufficient capacity to accommodate the number of elements copied. 
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or <c>null</c> to
        /// use the default <see cref="IEqualityComparer{T}"/> implementation for the set type.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
        public OrderedSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            m_hashSet = new OrderedDictionary<T, object>(collection.ToDictionary(t => t, v => (object)null), comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedSet{T}"/> class with serialized data.
        /// </summary>
        /// <param name="info">
        /// A <see cref="SerializationInfo"/> object that contains the information required to serialize the
        /// <see cref="OrderedSet{T}"/> object.
        /// </param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> structure that contains the source and destination of the serialized stream
        /// associated with the <see cref="OrderedSet{T}"/> object.
        /// </param>
        protected OrderedSet(SerializationInfo info, StreamingContext context)
        {
            // Setup call for protected deserialization constructor
            Type[] parameterTypes = new[] { typeof(SerializationInfo), typeof(StreamingContext) };

            ConstructorInfo constructor = typeof(OrderedDictionary<T, object>).GetConstructor(
                BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);

            // Construct hast set and start deserialization process
            m_hashSet = constructor.Invoke(new object[] { info, context }) as OrderedDictionary<T, object>;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="OrderedSet{T}"/>.</exception>
        public T this[int index]
        {
            get
            {
                return m_hashSet[index].Key;
            }
            set
            {
                m_hashSet[index] = new KeyValuePair<T, object>(value, null);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="OrderedSet{T}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="OrderedSet{T}"/>.
        /// </returns>
        public int Count
        {
            get
            {
                return m_hashSet.Count;
            }
        }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{T}" /> object that is used to determine equality for the values in the set.
        /// </summary>
        /// <returns>
        /// The <see cref="IEqualityComparer{T}" /> object that is used to determine equality for the values in the set.
        /// </returns>
        public IEqualityComparer<T> Comparer
        {
            get
            {
                return m_hashSet.Comparer;
            }
        }

        // Gets "false", indicating the hash-set is not read-only.
        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds an element to the current set and returns a value to indicate if the element was successfully added. 
        /// </summary>
        /// <returns>
        /// <c>true</c> if the element is added to the set; <c>false</c> if the element is already in the set.
        /// </returns>
        /// <param name="item">The element to add to the set.</param>
        public bool Add(T item)
        {
            try
            {
                m_hashSet.Add(item, null);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        // Adds an element according to ICollection<T> interface specification
        void ICollection<T>.Add(T item)
        {
            m_hashSet.Add(item, null);
        }

        /// <summary>
        /// Removes all items from the <see cref="OrderedSet{T}"/> object.
        /// </summary>
        public void Clear()
        {
            m_hashSet.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="OrderedSet{T}"/> object contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in the <see cref="OrderedSet{T}"/> object.</param>
        /// <returns>
        /// <c>true</c> if <see cref="OrderedSet{T}"/> object contains the specified element; otherwise,
        /// <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            return m_hashSet.ContainsKey(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="OrderedSet{T}"/> to an <see cref="Array"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
        /// <see cref="OrderedSet{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        public void CopyTo(T[] array)
        {
            m_hashSet.Keys.CopyTo(array, 0);
        }

        /// <summary>
        /// Copies the elements of the <see cref="OrderedSet{T}"/> to an <see cref="Array"/>, starting at the
        /// specified <paramref name="arrayIndex"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
        /// <see cref="OrderedSet{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="arrayIndex"/> is greater than the the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            m_hashSet.Keys.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies the specified number of elements of the <see cref="OrderedSet{T}"/> to an <see cref="Array"/>,
        /// starting at the specified <paramref name="arrayIndex"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
        /// <see cref="OrderedSet{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <param name="count">The number of elements to copy to <paramref name="array"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0 -or - <paramref name="count"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="arrayIndex"/> is greater than the the destination <paramref name="array"/> -or-
        /// <paramref name="count"/> is greater than the available space from the <paramref name="arrayIndex"/>
        /// to the end of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            int i = 0;

            foreach (T item in m_hashSet.Keys.Take(count))
            {
                if (i++ < count)
                {
                    array[arrayIndex + i] = item;
                }
                else
                    break;
            }
        }

        /// <summary>
        /// Removes all elements in the specified collection from the current set.
        /// </summary>
        /// <param name="other">The collection of items to remove from the set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void ExceptWith(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other)
            {
                if ((object)item != null)
                    m_hashSet.Remove(item);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="OrderedSet{T}"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the <see cref="OrderedSet{T}"/>.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return m_hashSet.Keys.GetEnumerator();
        }

        // Gets a non-generic enumerator
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_hashSet.Keys).GetEnumerator();
        }

        /// <summary>
        /// Implements the <see cref="ISerializable"/> interface and populates the <paramref name="info"/> object with 
        /// the data needed to serialize the serialize this <see cref="OrderedSet{T}"/> object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            m_hashSet.GetObjectData(info, context);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="OrderedSet{T}"/>.
        /// </summary>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="OrderedSet{T}"/>.</param>
        public int IndexOf(T item)
        {
            return m_hashSet.IndexOfKey(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="OrderedSet{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="OrderedSet{T}"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="OrderedSet{T}"/>.</exception>
        public void Insert(int index, T item)
        {
            m_hashSet.Insert(index, item, null);
        }

        /// <summary>
        /// Modifies the current set so that it contains only elements that are also in a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void IntersectWith(IEnumerable<T> other)
        {
            List<int> removeIndices = new List<int>();

            foreach (T item in other)
            {
                if (m_hashSet.ContainsKey(item))
                    removeIndices.Add(m_hashSet.IndexOfKey(item));
            }

            for (int i = removeIndices.Count - 1; i >= 0; i--)
            {
                m_hashSet.RemoveAt(i);
            }
        }

        /// <summary>
        /// Determines whether the current set is a proper (strict) subset of a specified collection.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current set is a proper subset of <paramref name="other"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            ISet<T> otherSet;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            otherSet = other as ISet<T>;

            if ((object)otherSet == null)
                otherSet = new HashSet<T>(other);

            return (Count < otherSet.Count) && this.All(item => otherSet.Contains(item));
        }

        /// <summary>
        /// Determines whether the current set is a proper (strict) superset of a specified collection.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current set is a proper superset of <paramref name="other"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            int otherCount = 0;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other.Distinct())
            {
                if (Count <= otherCount || !m_hashSet.ContainsKey(item))
                    return false;

                otherCount++;
            }

            return true;
        }

        /// <summary>
        /// Determines whether a set is a subset of a specified collection.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current set is a subset of <paramref name="other"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            ISet<T> otherSet;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            otherSet = other as ISet<T>;

            if ((object)otherSet == null)
                otherSet = new HashSet<T>(other);

            return (Count <= otherSet.Count) && this.All(item => otherSet.Contains(item));
        }

        /// <summary>
        /// Determines whether the current set is a superset of a specified collection.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current set is a superset of <paramref name="other"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            int otherCount = 0;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other.Distinct())
            {
                if (Count < otherCount || !m_hashSet.ContainsKey(item))
                    return false;

                otherCount++;
            }

            return true;
        }

        /// <summary>
        /// Implements the <see cref="IDeserializationCallback"/> interface and raises the deserialization event when
        /// the deserialization is complete.
        /// </summary>
        /// <param name="sender">The source of the deserialization event.</param>
        public void OnDeserialization(object sender)
        {
            // Complete hash set deserialization process
            ((IDeserializationCallback)m_hashSet).OnDeserialization(sender);
        }

        /// <summary>
        /// Determines whether the current set overlaps with the specified collection.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current set and <paramref name="other"/> share at least one common element; otherwise,
        /// <c>false</c>.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool Overlaps(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other)
            {
                if (m_hashSet.ContainsKey(item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the the specified element from the <see cref="OrderedSet{T}"/> object.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was successfully removed from the set; otherwise, <c>false</c>.
        /// This method also returns <c>false</c> if <paramref name="item"/> is not found in the 
        /// <see cref="OrderedSet{T}"/> object.
        /// </returns>
        /// <param name="item">The object to remove from the set.</param>
        public bool Remove(T item)
        {
            return m_hashSet.Remove(item);
        }

        /// <summary>
        /// Removes the <see cref="OrderedSet{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="OrderedSet{T}"/>.</exception>
        public void RemoveAt(int index)
        {
            m_hashSet.RemoveAt(index);
        }

        /// <summary>
        /// Removes all elements that match the conditions defined by the specified predicate from the
        /// <see cref="OrderedSet{T}"/> object.
        /// </summary>
        /// <returns>
        /// The number of elements that were removed from the the <see cref="OrderedSet{T}"/> object.
        /// </returns>
        /// <param name="match">
        /// The <see cref="Predicate{T}" /> delegate that defines the conditions of the elements to remove.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public int RemoveWhere(Predicate<T> match)
        {
            KeyValuePair<T, object>[] itemsToRemove = m_hashSet.Where(item => match(item.Key)).ToArray();

            foreach (KeyValuePair<T, object> item in itemsToRemove)
            {
                m_hashSet.Remove(item);
            }

            return itemsToRemove.Length;
        }

        /// <summary>
        /// Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current set is equal to <paramref name="other"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool SetEquals(IEnumerable<T> other)
        {
            int otherCount = 0;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other.Distinct())
            {
                if (!m_hashSet.ContainsKey(item))
                    return false;

                otherCount++;
            }

            return Count == otherCount;
        }

        /// <summary>
        /// Modifies the current set so that it contains only elements that are present either in the
        /// current set or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other.Distinct())
            {
                if (m_hashSet.ContainsKey(item))
                    m_hashSet.Remove(item);
                else
                    m_hashSet.Add(item, null);
            }
        }

        /// <summary>
        /// Sets the capacity of this <see cref="OrderedSet{T}"/> object to the actual number of
        /// elements it contains, rounded up to a nearby, implementation-specific value.
        /// </summary>
        public void TrimExcess()
        {
            m_hashSet.TrimExcess();
        }

        /// <summary>
        /// Modifies the current set so that it contains all elements that are present in either the
        /// current set or the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void UnionWith(IEnumerable<T> other)
        {
            foreach (T item in other)
                m_hashSet.Add(item, null);
        }

        #endregion
    }
}
