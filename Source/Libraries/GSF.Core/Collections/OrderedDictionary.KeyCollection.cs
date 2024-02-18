//******************************************************************************************************
//  OrderedDictionary.KeyCollection.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/18/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the NOTICE.txt file in the project root for more information.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace GSF.Collections;

public partial class OrderedDictionary<TKey, TValue>
{
    /// <summary>
    /// Represents the collection of keys in a <see cref="OrderedDictionary{TKey,TValue}" />. This class cannot be inherited.
    /// </summary>
    [DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public sealed class KeyCollection : IList<TKey>, IReadOnlyList<TKey>
    {
        private readonly OrderedDictionary<TKey, TValue> m_orderedDictionary;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="OrderedDictionary{TKey,TValue}.KeyCollection" />.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="OrderedDictionary{TKey,TValue}.KeyCollection" />.</returns>
        public int Count => m_orderedDictionary.Count;

        /// <summary>
        /// Gets the key at the specified index as an O(1) operation.
        /// </summary>
        /// <param name="index">The zero-based index of the key to get.</param>
        /// <returns>The key at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="OrderedDictionary{TKey,TValue}.KeyCollection.Count" />.</exception>
        public TKey this[int index] => ((IList<KeyValuePair<TKey, TValue>>)m_orderedDictionary)[index].Key;

        TKey IList<TKey>.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");
        }

        bool ICollection<TKey>.IsReadOnly => true;

        internal KeyCollection(OrderedDictionary<TKey, TValue> orderedDictionary)
        {
            m_orderedDictionary = orderedDictionary;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="OrderedDictionary{TKey,TValue}.KeyCollection" />.
        /// </summary>
        /// <returns>A <see cref="OrderedDictionary{TKey,TValue}.KeyCollection.Enumerator" /> for the <see cref="OrderedDictionary{TKey,TValue}.KeyCollection" />.</returns>
        public Enumerator GetEnumerator() => new(m_orderedDictionary);

        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IList<TKey>.IndexOf(TKey item) => m_orderedDictionary.IndexOf(item);

        void IList<TKey>.Insert(int index, TKey item) => throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");

        void IList<TKey>.RemoveAt(int index) => throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");

        void ICollection<TKey>.Add(TKey item) => throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");

        void ICollection<TKey>.Clear() => throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");

        bool ICollection<TKey>.Contains(TKey item) => m_orderedDictionary.ContainsKey(item!);

        void ICollection<TKey>.CopyTo(TKey[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            
            if ((uint)arrayIndex > (uint)array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Non-negative number required.");
            
            int count = Count;
            
            if (array.Length - arrayIndex < count)
                throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");

            Entry[] entries = m_orderedDictionary.m_entries;
            
            for (int i = 0; i < count; ++i)
                array[i + arrayIndex] = entries[i].Key;
        }

        bool ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");

        /// <summary>
        /// Enumerates the elements of a <see cref="OrderedDictionary{TKey,TValue}.KeyCollection" />.
        /// </summary>
        public struct Enumerator : IEnumerator<TKey>
        {
            private readonly OrderedDictionary<TKey, TValue> m_orderedDictionary;
            private readonly int m_version;
            private int m_index;
            private TKey m_current;

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the <see cref="OrderedDictionary{TKey,TValue}.KeyCollection" /> at the current position of the enumerator.</returns>
            public TKey Current => m_current;

            object IEnumerator.Current => m_current;

            internal Enumerator(OrderedDictionary<TKey, TValue> orderedDictionary)
            {
                m_orderedDictionary = orderedDictionary;
                m_version = orderedDictionary.m_version;
                m_index = 0;
                m_current = default;
            }

            /// <summary>
            /// Releases all resources used by the <see cref="OrderedDictionary{TKey,TValue}.KeyCollection.Enumerator" />.
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="OrderedDictionary{TKey,TValue}.KeyCollection" />.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            public bool MoveNext()
            {
                if (m_version != m_orderedDictionary.m_version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                if (m_index < m_orderedDictionary.Count)
                {
                    m_current = m_orderedDictionary.m_entries[m_index].Key;
                    ++m_index;
                    
                    return true;
                }
                
                m_current = default;
                
                return false;
            }

            void IEnumerator.Reset()
            {
                if (m_version != m_orderedDictionary.m_version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                m_index = 0;
                m_current = default;
            }
        }
    }
}