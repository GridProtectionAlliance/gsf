//******************************************************************************************************
//  OrderedDictionary.cs - Gbtc
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
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;

namespace GSF.Collections
{
    /// <summary>
    /// Represents a generic ordered collection of key/value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
    /// <remarks>
    /// Instances of this class using a <typeparamref name="TKey"/> of type <see cref="Int32"/> will cause ambiguity between
    /// accessing items by key and accessing items by index. If an integer is absolutely needed for the key, one could try
    /// using an <see cref="Int64"/> as the <typeparamref name="TKey"/> type instead.
    /// </remarks>
    public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>, ISerializable, IDeserializationCallback
    {
        #region [ Members ]

        // Nested Types

        // Defines a dictionary enumerator that will return items in order
        private class OrderedDictionaryEnumerator : IDictionaryEnumerator
        {
            #region [ Members ]

            // Fields
            private List<KeyValuePair<TKey, TValue>>.Enumerator m_enumerator;

            #endregion

            #region [ Constructors ]

            // Creates a new OrderedDictionaryEnumerator
            internal OrderedDictionaryEnumerator(List<KeyValuePair<TKey, TValue>> array)
            {
                m_enumerator = array.GetEnumerator();
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            /// <returns>
            /// The current element in the collection.
            /// </returns>
            public object Current
            {
                get
                {
                    return Entry;
                }
            }

            /// <summary>
            /// Gets both the key and the value of the current dictionary entry.
            /// </summary>
            /// <returns>
            /// A <see cref="DictionaryEntry"/> containing both the key and the value of the current dictionary entry.
            /// </returns>
            /// <exception cref="InvalidOperationException">The <see cref="IDictionaryEnumerator"/> is positioned before the first entry of the dictionary or after the last entry.</exception>
            public DictionaryEntry Entry
            {
                get
                {
                    KeyValuePair<TKey, TValue> item = m_enumerator.Current;
                    return new DictionaryEntry(item.Key, item.Value);
                }
            }

            /// <summary>
            /// Gets the key of the current dictionary entry.
            /// </summary>
            /// <returns>
            /// The key of the current element of the enumeration.
            /// </returns>
            /// <exception cref="InvalidOperationException">The <see cref="IDictionaryEnumerator"/> is positioned before the first entry of the dictionary or after the last entry.</exception>
            public object Key
            {
                get
                {
                    return Entry.Key;
                }
            }

            /// <summary>
            /// Gets the value of the current dictionary entry.
            /// </summary>
            /// <returns>
            /// The value of the current element of the enumeration.
            /// </returns>
            /// <exception cref="InvalidOperationException">The <see cref="IDictionaryEnumerator"/> is positioned before the first entry of the dictionary or after the last entry.</exception>
            public object Value
            {
                get
                {
                    return Entry.Value;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the enumerator was successfully advanced to the next element; <c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            public bool MoveNext()
            {
                return m_enumerator.MoveNext();
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            public void Reset()
            {
                ((IEnumerator)m_enumerator).Reset();
            }

            #endregion
        }

        // Fields
        private readonly Dictionary<TKey, TValue> m_dictionary;
        private readonly List<KeyValuePair<TKey, TValue>> m_list;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class that is empty and uses the default equality
        /// comparer for the ordered dictionary.
        /// </summary>
        public OrderedDictionary()
        {
            m_dictionary = new Dictionary<TKey, TValue>();
            m_list = new List<KeyValuePair<TKey, TValue>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class that uses the default equality comparer for
        /// the ordered dictionary, contains elements copied from the specified <paramref name="dictionary"/>, and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are copied to the new ordered dictionary.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <c>null</c>.</exception>
        public OrderedDictionary(IDictionary<TKey, TValue> dictionary)
        {
            m_dictionary = new Dictionary<TKey, TValue>(dictionary);
            m_list = new List<KeyValuePair<TKey, TValue>>(m_dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class that is empty and uses the specified
        /// equality <paramref name="comparer"/> for the ordered dictionary.
        /// </summary>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the dictionary, or <c>null</c> to
        /// use the default <see cref="IEqualityComparer{T}"/> implementation for the ordered dictionary.
        /// </param>
        public OrderedDictionary(IEqualityComparer<TKey> comparer)
        {
            m_dictionary = new Dictionary<TKey, TValue>(comparer);
            m_list = new List<KeyValuePair<TKey, TValue>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class that uses the specified equality
        /// <paramref name="comparer"/> for the ordered dictionary, contains elements copied from the specified <paramref name="dictionary"/>,
        /// and has sufficient capacity to accommodate the number of elements copied. 
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are copied to the new ordered dictionary.</param>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the dictionary, or <c>null</c> to
        /// use the default <see cref="IEqualityComparer{T}"/> implementation for the ordered dictionary.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <c>null</c>.</exception>
        public OrderedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            m_dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
            m_list = new List<KeyValuePair<TKey, TValue>>(m_dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class with serialized data.
        /// </summary>
        /// <param name="info">
        /// A <see cref="SerializationInfo"/> object that contains the information required to serialize the
        /// <see cref="OrderedDictionary{TKey, TValue}"/> object.
        /// </param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> structure that contains the source and destination of the serialized stream
        /// associated with the <see cref="OrderedDictionary{TKey, TValue}"/> object.
        /// </param>
        protected OrderedDictionary(SerializationInfo info, StreamingContext context)
        {
            // Setup call for protected deserialization constructor for dictionary
            Type[] parameterTypes = new[] { typeof(SerializationInfo), typeof(StreamingContext) };

            ConstructorInfo constructor = typeof(Dictionary<TKey, TValue>).GetConstructor(
                BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);

            // Construct dictionary and start deserialization process
            m_dictionary = constructor.Invoke(new object[] { info, context }) as Dictionary<TKey, TValue>;

            // Deserialize ordered list
            for (int x = 0; x < info.GetInt32("orderedCount"); x++)
            {
                Add((KeyValuePair<TKey, TValue>)info.GetValue("orderedItem" + x, typeof(KeyValuePair<TKey, TValue>)));
            }
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </exception>
        public KeyValuePair<TKey, TValue> this[int index]
        {
            get
            {
                return m_list[index];
            }
            set
            {
                if (index == Count)
                {
                    Add(value);
                }
                else
                {
                    TKey key = m_list[index].Key;
                    m_list[index] = value;
                    m_dictionary[key] = value.Value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception>
        public TValue this[TKey key]
        {
            get
            {
                return m_dictionary[key];
            }
            set
            {
                if (ContainsKey(key))
                {
                    int index = IndexOf(new KeyValuePair<TKey, TValue>(key, m_dictionary[key]));
                    m_list[index] = new KeyValuePair<TKey, TValue>(key, value);
                    m_dictionary[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </returns>
        public int Count
        {
            get
            {
                return m_list.Count;
            }
        }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{T}" /> object that is used to determine equality for the values in the dictionary.
        /// </summary>
        /// <returns>
        /// The <see cref="IEqualityComparer{T}" /> object that is used to determine equality for the values in the dictionary.
        /// </returns>
        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                return m_dictionary.Comparer;
            }
        }

        /// <summary>
        /// Gets a <see cref="ICollection{TKey}"/> containing the keys of the <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="ICollection{TKey}"/> containing the keys of the <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </returns>
        public ICollection<TKey> Keys
        {
            get
            {
                return m_dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a <see cref="ICollection{TValue}"/> containing the ordered values in the <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="ICollection{TValue}"/> containing the values in the <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </returns>
        public ICollection<TValue> Values
        {
            get
            {
                return m_list.Select(kvp => kvp.Value).ToList();
            }
        }

        #region [ Explicit Properties ]

        object IDictionary.this[object key]
        {
            get
            {
                return this[(TKey)key];
            }
            set
            {
                this[(TKey)key] = (TValue)value;
            }
        }

        object IOrderedDictionary.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (KeyValuePair<TKey, TValue>)value;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return m_dictionary.Keys;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return m_list;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        #endregion

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey, TValue}"/>.</exception>
        public void Add(TKey key, TValue value)
        {
            if (m_dictionary.ContainsKey(key))
                throw new ArgumentException("An element with the same key already exists.");

            m_dictionary.Add(key, value);
            m_list.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Adds a <see cref="KeyValuePair{TKey, TValue}"/> to the <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="OrderedDictionary{TKey, TValue}"/>.</param>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey, TValue}"/>.</exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (m_dictionary.ContainsKey(item.Key))
                throw new ArgumentException("An element with the same key already exists.");

            m_dictionary.Add(item.Key, item.Value);
            m_list.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        public void Clear()
        {
            m_dictionary.Clear();
            m_list.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="OrderedDictionary{TKey, TValue}"/> contains a specific <see cref="KeyValuePair{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> is found in the <see cref="OrderedDictionary{TKey, TValue}"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TValue}"/> to locate in the <see cref="OrderedDictionary{TKey, TValue}"/>.</param>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return m_dictionary.ContainsKey(item.Key);
        }

        /// <summary>
        /// Determines whether the <see cref="OrderedDictionary{TKey, TValue}"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the <see cref="OrderedDictionary{TKey, TValue}"/> contains an element with the <paramref name="key"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey, TValue}"/>.</param>
        public bool ContainsKey(TKey key)
        {
            return m_dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the <see cref="OrderedDictionary{TKey, TValue}"/> contains an element with the specified value.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the <see cref="OrderedDictionary{TKey, TValue}"/> contains an element with the <paramref name="value"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="value">The value to locate in the <see cref="OrderedDictionary{TKey, TValue}"/>.</param>
        public bool ContainsValue(TValue value)
        {
            return m_dictionary.ContainsValue(value);
        }

        /// <summary>
        /// Copies the elements of the <see cref="OrderedDictionary{TKey, TValue}"/> to an <see cref="Array"/>, starting at a particular index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="OrderedDictionary{TKey, TValue}"/> is greater than the available space from
        /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            m_list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        /// <summary>
        /// Implements the <see cref="ISerializable"/> interface and populates the <paramref name="info"/> object with 
        /// the data needed to serialize the serialize this <see cref="OrderedDictionary{TKey, TValue}"/> object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize dictionary
            m_dictionary.GetObjectData(info, context);

            // Serialize ordered list
            info.AddValue("orderedCount", Count);

            for (int x = 0; x < Count; x++)
            {
                info.AddValue("orderedItem" + x, m_list[x], typeof(KeyValuePair<TKey, TValue>));
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TValue}"/> to locate in the <see cref="OrderedDictionary{TKey, TValue}"/>.</param>
        public int IndexOf(KeyValuePair<TKey, TValue> item)
        {
            return m_list.IndexOf(item);
        }

        /// <summary>
        /// Searches for the specified key and returns the zero-based index within the entire <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey, TValue}"/>.</param>
        /// <returns>The zero-based index of key within the entire <see cref="OrderedDictionary{TKey, TValue}"/>, if found; otherwise, -1.</returns>
        public int IndexOfKey(TKey key)
        {
            IEqualityComparer<TKey> comparer = m_dictionary.Comparer;

            for (int i = 0; i < m_list.Count; i++)
            {
                if (comparer.Equals(m_list[i].Key, key))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Searches for the specified value and returns the zero-based index of the first occurrence within the entire <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="OrderedDictionary{TKey, TValue}"/>. The value can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire <see cref="OrderedDictionary{TKey, TValue}"/>, if found; otherwise, -1.</returns>
        public int IndexOfValue(TValue value)
        {
            IEqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;

            for (int i = 0; i < m_list.Count; i++)
            {
                if (comparer.Equals(m_list[i].Value, value))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Inserts a new entry into the <see cref="OrderedDictionary{TKey,TValue}"/> with the specified key and value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the element should be inserted.</param>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. The value can be <null/> if the type of the values in the dictionary is a reference type.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than -or- <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey,TValue}"/>.</exception>
        public void Insert(int index, TKey key, TValue value)
        {
            if (m_dictionary.ContainsKey(key))
                throw new ArgumentException("An element with the same key already exists.");

            m_list.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
            m_dictionary.Add(key, value);
        }

        /// <summary>
        /// Inserts a <see cref="KeyValuePair{TKey, TValue}"/> into the <see cref="OrderedDictionary{TKey,TValue}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TValue}"/> to insert into the <see cref="OrderedDictionary{TKey,TValue}"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="OrderedDictionary{TKey,TValue}"/>.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey,TValue}"/>.</exception>
        public void Insert(int index, KeyValuePair<TKey, TValue> item)
        {
            if (m_dictionary.ContainsKey(item.Key))
                throw new ArgumentException("An element with the same key already exists.");

            m_list.Insert(index, item);
            m_dictionary.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the element is successfully removed; otherwise, <c>false</c>. This method also returns
        /// <c>false</c> if <paramref name="key"/> was not found in the original <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool Remove(TKey key)
        {
            if (m_dictionary.ContainsKey(key))
            {
                KeyValuePair<TKey, TValue> item = new KeyValuePair<TKey, TValue>(key, m_dictionary[key]);
                m_dictionary.Remove(key);
                return m_list.Remove(item);
            }

            return false;
        }

        /// <summary>
        /// Removes the specific <see cref="KeyValuePair{TKey, TValue}"/> from the <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was successfully removed from the <see cref="OrderedDictionary{TKey,TValue}"/>; otherwise, <c>false</c>.
        /// This method also returns false if <paramref name="item"/> is not found in the original <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </returns>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TValue}"/> to remove from the <see cref="OrderedDictionary{TKey,TValue}"/>.</param>
        /// <exception cref="ArgumentNullException"><see cref="KeyValuePair{TKey, TValue}.Key"/> is null.</exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (m_dictionary.ContainsKey(item.Key))
            {
                m_dictionary.Remove(item.Key);
                return m_list.Remove(item);
            }

            return false;
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="OrderedDictionary{TKey,TValue}"/>.</exception>
        public void RemoveAt(int index)
        {
            KeyValuePair<TKey, TValue> item = m_list[index];

            m_list.RemoveAt(index);
            m_dictionary.Remove(item.Key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the <see cref="OrderedDictionary{TKey,TValue}"/> contains an element with the specified key; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of
        /// the <paramref name="value"/> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_dictionary.TryGetValue(key, out value);
        }

        #region [ Explicit Methods ]

        void IDictionary.Add(object key, object value)
        {
            Add((TKey)key, (TValue)value);
        }

        bool IDictionary.Contains(object key)
        {
            return ContainsKey((TKey)key);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)m_list).CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            return new OrderedDictionaryEnumerator(m_list);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new OrderedDictionaryEnumerator(m_list);
        }

        void IOrderedDictionary.Insert(int index, object key, object value)
        {
            Insert(index, (TKey)key, (TValue)value);
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            // Complete dictionary deserialization process
            m_dictionary.OnDeserialization(sender);
        }

        void IDictionary.Remove(object key)
        {
            Remove((TKey)key);
        }

        #endregion

        #endregion
    }
}
