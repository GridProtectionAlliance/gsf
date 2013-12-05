//******************************************************************************************************
//  ObservableSet.cs - Gbtc
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
//  11/15/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;

namespace GSF.Collections
{
    /// <summary>
    /// Represents a set of data that provides notifications when items get added, removed or when the whole set is refreshed.
    /// </summary>
    /// <remarks>
    /// This is simply a clean wrapper of <see cref="HashSet{T}"/> that implements <see cref="INotifyCollectionChanged"/>.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the hash set.</typeparam>
    [Serializable]
    public class ObservableSet<T> : ISet<T>, INotifyCollectionChanged, ISerializable, IDeserializationCallback
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when the <see cref="ObservableSet{T}"/> changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        // Fields
        private readonly HashSet<T> m_hashSet;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableSet{T}"/> class that is empty and uses the default equality
        /// comparer for the set type.
        /// </summary>
        public ObservableSet()
        {
            m_hashSet = new HashSet<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableSet{T}"/> class that uses the default equality comparer for
        /// the set type, contains elements copied from the specified <paramref name="collection"/>, and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
        public ObservableSet(IEnumerable<T> collection)
        {
            m_hashSet = new HashSet<T>(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableSet{T}"/> class that is empty and uses the specified
        /// equality <paramref name="comparer"/> for the set type.
        /// </summary>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or <c>null</c> to
        /// use the default <see cref="IEqualityComparer{T}"/> implementation for the set type.
        /// </param>
        public ObservableSet(IEqualityComparer<T> comparer)
        {
            m_hashSet = new HashSet<T>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableSet{T}"/> class that uses the specified equality
        /// <paramref name="comparer"/> for the set type, contains elements copied from the specified <paramref name="collection"/>,
        /// and has sufficient capacity to accommodate the number of elements copied. 
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or <c>null</c> to
        /// use the default <see cref="IEqualityComparer{T}"/> implementation for the set type.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
        public ObservableSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            m_hashSet = new HashSet<T>(collection, comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableSet{T}"/> class with serialized data.
        /// </summary>
        /// <param name="info">
        /// A <see cref="SerializationInfo"/> object that contains the information required to serialize the
        /// <see cref="ObservableSet{T}"/> object.
        /// </param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> structure that contains the source and destination of the serialized stream
        /// associated with the <see cref="ObservableSet{T}"/> object.
        /// </param>
        protected ObservableSet(SerializationInfo info, StreamingContext context)
        {
            // Setup call for protected deserialization constructor
            Type[] parameterTypes = new[] { typeof(SerializationInfo), typeof(StreamingContext) };

            ConstructorInfo constructor = typeof(HashSet<T>).GetConstructor(
                BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);

            // Construct hast set and start deserialization process
            m_hashSet = constructor.Invoke(new object[] { info, context }) as HashSet<T>;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ObservableSet{T}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="ObservableSet{T}"/>.
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
            return ExecuteAndNotify(() => m_hashSet.Add(item));
        }

        // Adds an element according to ICollection<T> interface specification
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="ObservableSet{T}"/> object.
        /// </summary>
        public void Clear()
        {
            // Determine if set contains data before clear
            bool setHadData = (m_hashSet.Count > 0);

            m_hashSet.Clear();

            if (setHadData)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Determines whether the <see cref="ObservableSet{T}"/> object contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in the <see cref="ObservableSet{T}"/> object.</param>
        /// <returns>
        /// <c>true</c> if <see cref="ObservableSet{T}"/> object contains the specified element; otherwise,
        /// <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            return m_hashSet.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ObservableSet{T}"/> to an <see cref="Array"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
        /// <see cref="ObservableSet{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        public void CopyTo(T[] array)
        {
            m_hashSet.CopyTo(array);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ObservableSet{T}"/> to an <see cref="Array"/>, starting at the
        /// specified <paramref name="arrayIndex"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
        /// <see cref="ObservableSet{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="arrayIndex"/> is greater than the the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            m_hashSet.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies the specified number of elements of the <see cref="ObservableSet{T}"/> to an <see cref="Array"/>,
        /// starting at the specified <paramref name="arrayIndex"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
        /// <see cref="ObservableSet{T}"/>. The <see cref="Array"/> must have zero-based indexing.
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
            m_hashSet.CopyTo(array, arrayIndex, count);
        }

        /// <summary>
        /// Removes all elements in the specified collection from the current set.
        /// </summary>
        /// <param name="other">The collection of items to remove from the set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void ExceptWith(IEnumerable<T> other)
        {
            ExecuteAndNotify(() => m_hashSet.ExceptWith(other));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ObservableSet{T}"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the <see cref="ObservableSet{T}"/>.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return m_hashSet.GetEnumerator();
        }

        // Gets a non-generic enumerator
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_hashSet).GetEnumerator();
        }

        /// <summary>
        /// Implements the <see cref="ISerializable"/> interface and populates the <paramref name="info"/> object with 
        /// the data needed to serialize the serialize this <see cref="ObservableSet{T}"/> object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            m_hashSet.GetObjectData(info, context);
        }

        /// <summary>
        /// Modifies the current set so that it contains only elements that are also in a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void IntersectWith(IEnumerable<T> other)
        {
            ExecuteAndNotify(() => m_hashSet.IntersectWith(other));
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
            return m_hashSet.IsProperSubsetOf(other);
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
            return m_hashSet.IsProperSupersetOf(other);
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
            return m_hashSet.IsSubsetOf(other);
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
            return m_hashSet.IsSupersetOf(other);
        }

        /// <summary>
        /// Implements the <see cref="IDeserializationCallback"/> interface and raises the deserialization event when
        /// the deserialization is complete.
        /// </summary>
        /// <param name="sender">The source of the deserialization event.</param>
        public void OnDeserialization(object sender)
        {
            // Complete hash set deserialization process
            m_hashSet.OnDeserialization(sender);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
            return m_hashSet.Overlaps(other);
        }

        /// <summary>
        /// Removes the the specified element from the <see cref="ObservableSet{T}"/> object.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was successfully removed from the set; otherwise, <c>false</c>.
        /// This method also returns <c>false</c> if <paramref name="item"/> is not found in the 
        /// <see cref="ObservableSet{T}"/> object.
        /// </returns>
        /// <param name="item">The object to remove from the set.</param>
        public bool Remove(T item)
        {
            return ExecuteAndNotify(() => m_hashSet.Remove(item));
        }

        /// <summary>
        /// Removes all elements that match the conditions defined by the specified predicate from the
        /// <see cref="ObservableSet{T}"/> object.
        /// </summary>
        /// <returns>
        /// The number of elements that were removed from the the <see cref="ObservableSet{T}"/> object.
        /// </returns>
        /// <param name="match">
        /// The <see cref="Predicate{T}" /> delegate that defines the conditions of the elements to remove.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public int RemoveWhere(Predicate<T> match)
        {
            return ExecuteAndNotify(() => m_hashSet.RemoveWhere(match));
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
            return m_hashSet.SetEquals(other);
        }

        /// <summary>
        /// Modifies the current set so that it contains only elements that are present either in the
        /// current set or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            ExecuteAndNotify(() => m_hashSet.SymmetricExceptWith(other));
        }

        /// <summary>
        /// Sets the capacity of this <see cref="ObservableSet{T}"/> object to the actual number of
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
            ExecuteAndNotify(() => m_hashSet.UnionWith(other));
        }

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event for this <see cref="ObservableSet{T}"/> object.
        /// </summary>
        /// <param name="e">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> object to send to the <see cref="CollectionChanged"/> event.
        /// </param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if ((object)CollectionChanged != null)
                CollectionChanged(this, e);
        }

        // Executes action and raises OnCollectionChanged event if items were added or removed as a result of action
        private void ExecuteAndNotify(Action action)
        {
            int preCount, postCount;

            // Get count before action
            preCount = m_hashSet.Count;

            // Execute action
            action();

            // Get count after action
            postCount = m_hashSet.Count;

            // Raise collection changed notification based on if items were added or removed
            if (preCount < postCount)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
            else if (preCount > postCount)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
        }

        // Executes function and raises OnCollectionChanged event if items were added or removed as a result of function
        private TResult ExecuteAndNotify<TResult>(Func<TResult> function)
        {
            TResult result;
            int preCount, postCount;

            // Get count before function
            preCount = m_hashSet.Count;

            // Execute function
            result = function();

            // Get count after function
            postCount = m_hashSet.Count;

            // Raise collection changed notification based on if items were added or removed
            if (preCount < postCount)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
            else if (preCount > postCount)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));

            return result;
        }

        #endregion
    }
}
