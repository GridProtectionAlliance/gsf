//******************************************************************************************************
//  OrderedTreeSet.cs - Gbtc
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
//  12/09/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;

namespace GSF.Collections
{
    /// <summary>
    /// Represents an ordered set of data.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    [Serializable]
    public class OrderedTreeSet<T> : IOrderedSet<T>, ISerializable
    {
        #region [ Members ]

        // Nested Types
        private class TreeSetItem : IComparable<TreeSetItem>
        {
            public T Value;
            public int InsertValue;
            public int LeftCount;
            public int RightCount;

            public int DescendantCount
            {
                get
                {
                    return LeftCount + RightCount;
                }
            }

            public int CompareTo(TreeSetItem other)
            {
                if (InsertValue <= LeftCount)
                    return -1;

                InsertValue -= LeftCount + 1;

                return 1;
            }
        }

        private class TreeSetNode : RedBlackNode<TreeSetItem>
        {
            #region [ Constructors ]

            public TreeSetNode(TreeSetItem item)
                : base(item)
            {
            }

            #endregion

            #region [ Properties ]

            public new TreeSetNode Root
            {
                get
                {
                    return (TreeSetNode)base.Root;
                }
            }

            public new TreeSetNode Parent
            {
                get
                {
                    return (TreeSetNode)base.Parent;
                }
            }

            public new TreeSetNode Left
            {
                get
                {
                    return (TreeSetNode)base.Left;
                }
            }

            public new TreeSetNode Right
            {
                get
                {
                    return (TreeSetNode)base.Right;
                }
            }

            public int Index
            {
                get
                {
                    int index = 0;

                    if ((object)Parent != null && this == Parent.Right)
                        index = Parent.Index + 1;

                    return index + Item.LeftCount;
                }
            }

            public TreeSetNode this[int index]
            {
                get
                {
                    if (index == 0 && Item.LeftCount == 0)
                        return this;

                    if (index <= Item.LeftCount)
                        return Left[index];

                    return Right[index - Item.LeftCount - 1];
                }
            }

            #endregion

            #region [ Methods ]

            protected override void ChildInserted()
            {
                TreeSetNode node = this;

                while ((object)node != null)
                {
                    node.UpdateDescendantCounts();
                    node = node.Parent;
                }
            }

            protected override void ChildRemoved()
            {
                TreeSetNode node = this;

                while ((object)node != null)
                {
                    node.UpdateDescendantCounts();
                    node = node.Parent;
                }
            }

            protected override void RotatedLeft()
            {
                Item.RightCount = ((object)Right != null) ? Right.Item.DescendantCount + 1 : 0;
                Parent.Item.LeftCount = Item.DescendantCount + 1;
            }

            protected override void RotatedRight()
            {
                Item.LeftCount = ((object)Left != null) ? Left.Item.DescendantCount + 1 : 0;
                Parent.Item.RightCount = Item.DescendantCount + 1;
            }

            protected override void Swapped(RedBlackNode<TreeSetItem> swappedNode)
            {
                UpdateDescendantCounts();
            }

            private void UpdateDescendantCounts()
            {
                Item.LeftCount = ((object)Left != null) ? Left.Item.DescendantCount + 1 : 0;
                Item.RightCount = ((object)Right != null) ? Right.Item.DescendantCount + 1 : 0;
            }

            #endregion
        }

        // Fields
        private IEqualityComparer<T> m_equalityComparer;
        private IDictionary<T, TreeSetNode> m_nodeLookup;
        private TreeSetNode m_root; 

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedTreeSet{T}"/> class that is empty and uses the default equality
        /// comparer for the set type.
        /// </summary>
        public OrderedTreeSet()
            : this(Enumerable.Empty<T>(), EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedTreeSet{T}"/> class that uses the default equality comparer for
        /// the set type, contains elements copied from the specified <paramref name="collection"/>, and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
        public OrderedTreeSet(IEnumerable<T> collection)
            : this (collection, EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedTreeSet{T}"/> class that is empty and uses the specified
        /// equalilty <paramref name="comparer"/> for the set type.
        /// </summary>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or <c>null</c> to
        /// use the default <see cref="IEqualityComparer{T}"/> implementation for the set type.
        /// </param>
        public OrderedTreeSet(IEqualityComparer<T> comparer)
            : this(Enumerable.Empty<T>(), comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedTreeSet{T}"/> class that uses the specified equality
        /// <paramref name="comparer"/> for the set type, contains elements copied from the specified <paramref name="collection"/>,
        /// and has sufficient capacity to accommodate the number of elements copied. 
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new set.</param>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or <c>null</c> to
        /// use the default <see cref="IEqualityComparer{T}"/> implementation for the set type.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
        public OrderedTreeSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            m_equalityComparer = comparer;
            m_nodeLookup = new Dictionary<T, TreeSetNode>(comparer);

            foreach (T item in collection)
                Add(item);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedTreeSet{T}"/> class with serialized data.
        /// </summary>
        /// <param name="info">
        /// A <see cref="SerializationInfo"/> object that contains the information required to serialize the
        /// <see cref="OrderedTreeSet{T}"/> object.
        /// </param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> structure that contains the source and destination of the serialized stream
        /// associated with the <see cref="OrderedTreeSet{T}"/> object.
        /// </param>
        protected OrderedTreeSet(SerializationInfo info, StreamingContext context)
        {
            m_equalityComparer = (IEqualityComparer<T>)info.GetValue("equalityComparer", typeof(IEqualityComparer<T>));
            m_nodeLookup = new Dictionary<T, TreeSetNode>(m_equalityComparer);

            // Deserialize ordered list
            for (int x = 0; x < info.GetInt32("orderedCount"); x++)
                Add((T)info.GetValue("orderedItem" + x, typeof(T)));
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="OrderedTreeSet{T}"/>.</exception>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                return m_root[index].Item.Value;
            }
            set
            {
                TreeSetNode node;

                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                node = m_root[index];
                m_nodeLookup.Remove(node.Item.Value);
                node.Item.Value = value;
                m_nodeLookup.Add(node.Item.Value, node);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="OrderedTreeSet{T}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="OrderedTreeSet{T}"/>.
        /// </returns>
        public int Count
        {
            get
            {
                return m_nodeLookup.Count;
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
                return m_equalityComparer;
            }
        }

        // Gets "false", indicating the set is not read-only.
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
            return Insert(Count, item);
        }

        // Adds an element according to ICollection<T> interface specification
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="OrderedTreeSet{T}"/> object.
        /// </summary>
        public void Clear()
        {
            m_nodeLookup.Clear();
            m_root = null;
        }

        /// <summary>
        /// Determines whether the <see cref="OrderedTreeSet{T}"/> object contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in the <see cref="OrderedTreeSet{T}"/> object.</param>
        /// <returns>
        /// <c>true</c> if <see cref="OrderedTreeSet{T}"/> object contains the specified element; otherwise,
        /// <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            if ((object)item == null)
                return false;

            return m_nodeLookup.ContainsKey(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="OrderedTreeSet{T}"/> to an <see cref="Array"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
        /// <see cref="OrderedTreeSet{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        /// <summary>
        /// Copies the elements of the <see cref="OrderedTreeSet{T}"/> to an <see cref="Array"/>, starting at the
        /// specified <paramref name="arrayIndex"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
        /// <see cref="OrderedTreeSet{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="arrayIndex"/> is greater than the the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex, Count);
        }

        /// <summary>
        /// Copies the specified number of elements of the <see cref="OrderedTreeSet{T}"/> to an <see cref="Array"/>,
        /// starting at the specified <paramref name="arrayIndex"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
        /// <see cref="OrderedTreeSet{T}"/>. The <see cref="Array"/> must have zero-based indexing.
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
            int index;

            if ((object)array == null)
                throw new ArgumentNullException("array");

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (arrayIndex >= array.Length)
                throw new ArgumentException("arrayIndex must be less than the length of the array.");

            if (count <= array.Length - arrayIndex)
                throw new ArgumentException("count must be less than the available space in the array.");

            index = 0;

            foreach (T item in this)
            {
                if (index >= count)
                    break;

                array[arrayIndex + index] = item;
                index++;
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
                Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="OrderedTreeSet{T}"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the <see cref="OrderedTreeSet{T}"/>.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            RedBlackNode<TreeSetItem> node;

            if ((object)m_root != null)
            {
                node = m_root.First;

                while ((object)node != null)
                {
                    yield return node.Item.Value;
                    node = node.Next;
                }
            }
        }

        // Gets a non-generic enumerator
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Implements the <see cref="ISerializable"/> interface and populates the <paramref name="info"/> object with 
        /// the data needed to serialize the serialize this <see cref="OrderedTreeSet{T}"/> object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int index = 0;

            info.AddValue("equalityComparer", m_equalityComparer, typeof(IEqualityComparer<T>));
            info.AddValue("orderedCount", Count);

            foreach (T item in this)
            {
                info.AddValue("orderedItem" + index, item, typeof(T));
                index++;
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="OrderedTreeSet{T}"/>.
        /// </summary>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="OrderedTreeSet{T}"/>.</param>
        public int IndexOf(T item)
        {
            TreeSetNode node;

            if ((object)item == null)
                throw new ArgumentNullException("item");

            if (m_nodeLookup.TryGetValue(item, out node))
                return node.Index;

            return -1;
        }

        /// <summary>
        /// Inserts an item to the <see cref="OrderedTreeSet{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="OrderedTreeSet{T}"/>.</param>
        /// <returns>True if the item was inserted; false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="OrderedTreeSet{T}"/>.</exception>
        public bool Insert(int index, T item)
        {
            TreeSetNode node;

            if ((object)item == null)
                throw new ArgumentNullException("item");

            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException("index");

            if (m_nodeLookup.ContainsKey(item))
                return false;

            node = new TreeSetNode(new TreeSetItem()
            {
                Value = item,
                InsertValue = index
            });

            m_nodeLookup.Add(item, node);

            if ((object)m_root == null)
            {
                m_root = node;
            }
            else
            {
                m_root.Insert(node);
                m_root = m_root.Root;
            }

            return true;
        }

        /// <summary>
        /// Inserts an item to the <see cref="OrderedTreeSet{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="OrderedTreeSet{T}"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="OrderedTreeSet{T}"/>.</exception>
        void IList<T>.Insert(int index, T item)
        {
            Insert(index, item);
        }

        /// <summary>
        /// Modifies the current set so that it contains only elements that are also in a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void IntersectWith(IEnumerable<T> other)
        {
            IEnumerable<int> indexes;
            bool[] keep;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            indexes = other.Where(Contains).Select(IndexOf);
            keep = new bool[Count];

            foreach (int index in indexes)
                keep[index] = true;

            for (int i = Count; i >= 0; i--)
            {
                if (!keep[i])
                    RemoveAt(i);
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
            bool[] present;
            bool containsAll;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            present = new bool[Count];
            containsAll = true;

            foreach (T item in other)
            {
                if (Contains(item))
                    present[IndexOf(item)] = true;
                else
                    containsAll = false;
            }

            return present.All(p => p) && !containsAll;
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
            bool[] present;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            present = new bool[Count];

            foreach (T item in other)
            {
                if (!Contains(item))
                    return false;

                present[IndexOf(item)] = true;
            }

            return !present.All(p => p);
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
            bool[] present;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            present = new bool[Count];

            foreach (T item in other)
            {
                if (Contains(item))
                    present[IndexOf(item)] = true;
            }

            return present.All(p => p);
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
            if ((object)other == null)
                throw new ArgumentNullException("other");

            return other.All(Contains);
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

            return other.Any(Contains);
        }

        /// <summary>
        /// Removes the the specified element from the <see cref="OrderedTreeSet{T}"/> object.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was successfully removed from the set; otherwise, <c>false</c>.
        /// This method also returns <c>false</c> if <paramref name="item"/> is not found in the 
        /// <see cref="OrderedTreeSet{T}"/> object.
        /// </returns>
        /// <param name="item">The object to remove from the set.</param>
        public bool Remove(T item)
        {
            TreeSetNode node;
            TreeSetNode relative;

            if ((object)item == null)
                throw new ArgumentNullException("item");

            if (m_nodeLookup.TryGetValue(item, out node))
            {
                relative = node.Parent ?? node.Left ?? node.Right;
                m_nodeLookup.Remove(item);
                node.Remove();

                if ((object)relative != null)
                    m_root = relative.Root;
                else
                    m_root = null;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the <see cref="OrderedTreeSet{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="OrderedTreeSet{T}"/>.</exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");

            Remove(m_root[index].Item.Value);
        }

        /// <summary>
        /// Removes all elements that match the conditions defined by the specified predicate from the
        /// <see cref="OrderedTreeSet{T}"/> object.
        /// </summary>
        /// <returns>
        /// The number of elements that were removed from the the <see cref="OrderedTreeSet{T}"/> object.
        /// </returns>
        /// <param name="match">
        /// The <see cref="Predicate{T}" /> delegate that defines the conditions of the elements to remove.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public int RemoveWhere(Predicate<T> match)
        {
            RedBlackNode<TreeSetItem> node;
            RedBlackNode<TreeSetItem> nextNode;
            int itemsRemoved;

            if ((object)match == null)
                throw new ArgumentNullException("match");

            node = m_root.First;
            itemsRemoved = 0;

            while ((object)node != null)
            {
                nextNode = node.Next;

                if (match(node.Item.Value))
                {
                    m_nodeLookup.Remove(node.Item.Value);
                    m_root.Remove();
                    itemsRemoved++;
                }

                node = nextNode;
            }

            return itemsRemoved;
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
            bool[] present;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            present = new bool[Count];

            foreach (T item in other)
            {
                if (!Contains(item))
                    return false;

                present[IndexOf(item)] = true;
            }

            return present.All(p => p);
        }

        /// <summary>
        /// Modifies the current set so that it contains only elements that are present either in the
        /// current set or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            bool[] remove;

            if ((object)other == null)
                throw new ArgumentNullException("other");

            remove = new bool[Count];

            foreach (T item in other)
            {
                if (!Contains(item))
                {
                    Add(item);
                }
                else
                {
                    int index = IndexOf(item);

                    if (index < remove.Length)
                        remove[index] = true;
                }
            }

            for (int i = remove.Length; i >= 0; i--)
            {
                if (remove[i])
                    RemoveAt(i);
            }
        }

        /// <summary>
        /// Modifies the current set so that it contains all elements that are present in either the
        /// current set or the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public void UnionWith(IEnumerable<T> other)
        {
            if ((object)other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other)
                Add(item);
        }

        #endregion
    }
}
