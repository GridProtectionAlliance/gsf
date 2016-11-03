//******************************************************************************************************
//  ImmutableDictionary`2.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  10/24/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;

namespace GSF.Immutable
{
    /// <summary>
    /// A dictionary that can be modified until <see cref="ImmutableObjectBase{T}.IsReadOnly"/> is set to true. Once this occurs,
    /// the dictionary itself can no longer be modified.  Remember, this does not cause objects contained in this class to be Immutable 
    /// unless they implement <see cref="IImmutableObject"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ImmutableDictionary<TKey, TValue>
        : ImmutableObjectBase<ImmutableDictionary<TKey, TValue>>, IDictionary<TKey, TValue>
    {
        private readonly bool m_isISupportsReadonlyKeyType;
        private readonly bool m_isISupportsReadonlyValueType;
        private Dictionary<TKey, TValue> m_dictionary;

        /// <summary>
        /// Creates a new <see cref="ImmutableDictionary{TKey,TValue}"/>.
        /// </summary>
        public ImmutableDictionary()
        {
            m_isISupportsReadonlyKeyType = typeof(IImmutableObject).IsAssignableFrom(typeof(TKey));
            m_isISupportsReadonlyValueType = typeof(IImmutableObject).IsAssignableFrom(typeof(TValue));
            m_dictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Creates a new <see cref="ImmutableDictionary{TKey,TValue}"/>.
        /// </summary>
        public ImmutableDictionary(int capacity)
        {
            m_isISupportsReadonlyKeyType = typeof(IImmutableObject).IsAssignableFrom(typeof(TKey));
            m_isISupportsReadonlyValueType = typeof(IImmutableObject).IsAssignableFrom(typeof(TValue));
            m_dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Creates a new <see cref="ImmutableDictionary{TKey,TValue}"/>.
        /// </summary>
        public ImmutableDictionary(Dictionary<TKey, TValue> baseDictionary)
        {
            m_dictionary = baseDictionary;
            m_isISupportsReadonlyKeyType = typeof(IImmutableObject).IsAssignableFrom(typeof(TKey));
            m_isISupportsReadonlyValueType = typeof(IImmutableObject).IsAssignableFrom(typeof(TValue));
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return m_dictionary.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_dictionary).GetEnumerator();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            TestForEditable();
            ((ICollection<KeyValuePair<TKey, TValue>>)m_dictionary).Add(item);
        }

        /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
        public void Clear()
        {
            TestForEditable();
            m_dictionary.Clear();
        }

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)m_dictionary).Contains(item);
        }

        /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)m_dictionary).CopyTo(array, arrayIndex);
        }

        /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            TestForEditable();
            return ((ICollection<KeyValuePair<TKey, TValue>>)m_dictionary).Remove(item);
        }

        /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count
        {
            get
            {
                return m_dictionary.Count;
            }
        }

        /// <summary>
        /// SetMembersAsReadOnly
        /// </summary>
        protected override void SetMembersAsReadOnly()
        {
            if (m_isISupportsReadonlyKeyType || m_isISupportsReadonlyValueType)
            {
                foreach (var kvp in m_dictionary)
                {
                    if (m_isISupportsReadonlyKeyType)
                    {
                        var item = kvp.Key as IImmutableObject;
                        if (item != null)
                        {
                            item.IsReadOnly = true;
                        }
                    }
                    if (m_isISupportsReadonlyValueType)
                    {
                        var item = kvp.Value as IImmutableObject;
                        if (item != null)
                        {
                            item.IsReadOnly = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// CloneMembersAsEditable
        /// </summary>
        protected override void CloneMembersAsEditable()
        {
            if (m_isISupportsReadonlyKeyType || m_isISupportsReadonlyValueType)
            {
                var oldList = m_dictionary;
                m_dictionary = new Dictionary<TKey, TValue>();
                foreach (var kvp in oldList)
                {
                    var k = kvp.Key;
                    var v = kvp.Value;
                    if (m_isISupportsReadonlyKeyType)
                    {
                        var item = k as IImmutableObject;
                        if (item != null)
                        {
                            k = (TKey)item.CloneEditable();
                        }
                    }
                    if (m_isISupportsReadonlyValueType)
                    {
                        var item = k as IImmutableObject;
                        if (item != null)
                        {
                            v = (TValue)item.CloneEditable();
                        }
                    }
                    m_dictionary.Add(k, v);
                }
            }
            else
            {
                m_dictionary = new Dictionary<TKey, TValue>(m_dictionary);
            }
        }

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.</returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return m_dictionary.ContainsKey(key);
        }

        /// <summary>Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
        public void Add(TKey key, TValue value)
        {
            TestForEditable();
            m_dictionary.Add(key, value);
        }

        /// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
        public bool Remove(TKey key)
        {
            TestForEditable();
            return m_dictionary.Remove(key);
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <returns>true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_dictionary.TryGetValue(key, out value);
        }

        /// <summary>Gets or sets the element with the specified key.</summary>
        /// <returns>The element with the specified key.</returns>
        /// <param name="key">The key of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> is not found.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
        public TValue this[TKey key]
        {
            get
            {
                return m_dictionary[key];
            }
            set
            {
                TestForEditable();
                m_dictionary[key] = value;
            }
        }

        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        public ICollection<TKey> Keys => m_dictionary.Keys;

        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        public ICollection<TValue> Values => m_dictionary.Values;
    }
}