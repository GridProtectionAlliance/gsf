//******************************************************************************************************
//  FileBackedDictionary.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  10/28/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GSF.Collections
{
    /// <summary>
    /// Represents a lookup table of key/value pairs backed by a file, with very little memory overhead.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the lookup table.</typeparam>
    /// <typeparam name="TValue">The type of the values in the lookup table.</typeparam>
    public sealed class FileBackedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
        #region [ Members ]

        // Fields
        private FileBackedLookupTable<TKey, TValue> m_lookupTable;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> cannot be serialized.</exception>
        /// <remarks>
        /// This constructor uses the default equality comparer for file backed lookup tables,
        /// which is not the same as the default equality comparer for <typeparamref name="TKey"/>
        /// objects. This is because the default implementation of <see cref="object.GetHashCode"/>
        /// does not provide guarantees about consistency across platforms, or even implementations
        /// of the CLR. Instead, the default equality comparer uses a byte-for-byte comparison to
        /// determine equality between keys and a CRC-32 for its hash code implementation. This
        /// means the performance of the hashing function is dependent on the performance of the
        /// serialization function.
        /// </remarks>
        public FileBackedDictionary()
            : this((IEqualityComparer<TKey>)null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file used to store the lookup table.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> cannot be serialized.</exception>
        /// <remarks>
        /// This constructor uses the default equality comparer for file backed lookup tables,
        /// which is not the same as the default equality comparer for <typeparamref name="TKey"/>
        /// objects. This is because the default implementation of <see cref="object.GetHashCode"/>
        /// does not provide guarantees about consistency across platforms, or even implementations
        /// of the CLR. Instead, the default equality comparer uses a byte-for-byte comparison to
        /// determine equality between keys and a CRC-32 for its hash code implementation. This
        /// means the performance of the hashing function is dependent on the performance of the
        /// serialization function.
        /// </remarks>
        public FileBackedDictionary(string filePath)
            : this(filePath, (IEqualityComparer<TKey>)null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are copied to this dictionary.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> cannot be serialized.</exception>
        /// <remarks>
        /// This constructor uses the default equality comparer for file backed lookup tables,
        /// which is not the same as the default equality comparer for <typeparamref name="TKey"/>
        /// objects. This is because the default implementation of <see cref="object.GetHashCode"/>
        /// does not provide guarantees about consistency across platforms, or even implementations
        /// of the CLR. Instead, the default equality comparer uses a byte-for-byte comparison to
        /// determine equality between keys and a CRC-32 for its hash code implementation. This
        /// means the performance of the hashing function is dependent on the performance of the
        /// serialization function.
        /// </remarks>
        public FileBackedDictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="keyComparer">The equality comparer used to compare keys in the dictionary.</param>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> cannot be serialized.</exception>
        public FileBackedDictionary(IEqualityComparer<TKey> keyComparer)
            : this(Path.GetTempFileName(), keyComparer)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file used to store the lookup table.</param>
        /// <param name="dictionary">The dictionary whose elements are copied to this dictionary.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null or <paramref name="dictionary"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> cannot be serialized.</exception>
        /// <remarks>
        /// This constructor uses the default equality comparer for file backed lookup tables,
        /// which is not the same as the default equality comparer for <typeparamref name="TKey"/>
        /// objects. This is because the default implementation of <see cref="object.GetHashCode"/>
        /// does not provide guarantees about consistency across platforms, or even implementations
        /// of the CLR. Instead, the default equality comparer uses a byte-for-byte comparison to
        /// determine equality between keys and a CRC-32 for its hash code implementation. This
        /// means the performance of the hashing function is dependent on the performance of the
        /// serialization function.
        /// </remarks>
        public FileBackedDictionary(string filePath, IDictionary<TKey, TValue> dictionary)
            : this(filePath, dictionary, EqualityComparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file used to store the lookup table.</param>
        /// <param name="keyComparer">The equality comparer used to compare keys in the dictionary.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> cannot be serialized.</exception>
        public FileBackedDictionary(string filePath, IEqualityComparer<TKey> keyComparer)
        {
            m_lookupTable = new FileBackedLookupTable<TKey, TValue>(LookupTableType.Dictionary, filePath, keyComparer);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are copied to this dictionary.</param>
        /// <param name="keyComparer">The equality comparer used to compare keys in the dictionary.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> cannot be serialized.</exception>
        public FileBackedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer)
            : this(Path.GetTempFileName(), dictionary, keyComparer)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file used to store the lookup table.</param>
        /// <param name="dictionary">The dictionary whose elements are copied to this dictionary.</param>
        /// <param name="keyComparer">The equality comparer used to compare keys in the dictionary.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null or <paramref name="dictionary"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> cannot be serialized.</exception>
        public FileBackedDictionary(string filePath, IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer)
        {
            m_lookupTable = new FileBackedLookupTable<TKey, TValue>(LookupTableType.Dictionary, filePath, keyComparer);

            foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
                Add(kvp);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the path to the file backing this dictionary.
        /// </summary>
        /// <exception cref="ArgumentException">FilePath is set and is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <remarks>
        /// Changes to this property will cause the file to close if the file is already opened.
        /// Data will not be automatically written from the old file to the new file.
        /// </remarks>
        public string FilePath
        {
            get
            {
                return m_lookupTable.FilePath;
            }
            set
            {
                m_lookupTable.FilePath = value;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </returns>
        public int Count
        {
            get
            {
                return m_lookupTable.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="FileBackedDictionary{TKey, TValue}"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="FileBackedDictionary{TKey, TValue}"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                return m_lookupTable.IsReadOnly;
            }
        }

        /// <summary>
        /// Gets the default signature used by the <see cref="FileBackedDictionary{TKey, TValue}"/>
        /// if no user-defined signature is supplied.
        /// </summary>
        public byte[] DefaultSignature
        {
            get
            {
                return new Guid(FileBackedLookupTable<TKey, TValue>.DictionarySignature).ToRfcBytes();
            }
        }

        /// <summary>
        /// Gets or sets the signature of the file backing the lookup table.
        /// </summary>
        /// <exception cref="ArgumentNullException">Attempt is made to set Signature to a null value.</exception>
        /// <exception cref="ArgumentException">Attempt is made to set Signature to a value larger than the maximum allowed size.</exception>
        /// <exception cref="NotSupportedException">Attempt is made to modify Signature of a read-only lookup table.</exception>
        public byte[] Signature
        {
            get
            {
                return m_lookupTable.Signature;
            }
            set
            {
                m_lookupTable.Signature = value;
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
        /// <exception cref="NotSupportedException">The property is set and the <see cref="FileBackedDictionary{TKey, TValue}"/> is read-only.</exception>
        public TValue this[TKey key]
        {
            get
            {
                return m_lookupTable[key];
            }
            set
            {
                m_lookupTable[key] = value;
            }
        }

        /// <summary>
        /// Gets an <see cref="FileBackedDictionary{TKey, TValue}"/> containing the keys of the <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="FileBackedDictionary{TKey, TValue}"/> containing the keys of the object that implements <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </returns>
        public ICollection<TKey> Keys
        {
            get
            {
                return GetKeys().ToList();
            }
        }

        /// <summary>
        /// Gets an <see cref="FileBackedDictionary{TKey, TValue}"/> containing the values in the <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="FileBackedDictionary{TKey, TValue}"/> containing the values in the object that implements <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </returns>
        public ICollection<TValue> Values
        {
            get
            {
                return GetValues().ToList();
            }
        }

        /// <summary>
        /// Gets or sets the size of the cache used
        /// to store data from the file in memory.
        /// </summary>
        public long CacheSize
        {
            get
            {
                return m_lookupTable.CacheSize;
            }
            set
            {
                m_lookupTable.CacheSize = value;
            }
        }

        /// <summary>
        /// Gets the number of operations that fragment the
        /// lookup table that have occurred since the last
        /// time the lookup table was compacted.
        /// </summary>
        /// <remarks>
        /// This value is not stored in the file and may therefore
        /// be inaccurate if the lookup table has not been compacted
        /// since the last time it was opened.
        /// </remarks>
        public int FragmentationCount
        {
            get
            {
                return m_lookupTable.FragmentationCount;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Opens the file backing this dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">File is already open.</exception>
        public void Open()
        {
            m_lookupTable.Open();
        }

        /// <summary>
        /// Opens the file backing this hash set in read-only mode.
        /// </summary>
        /// <exception cref="InvalidOperationException">File is already open.</exception>
        public void OpenRead()
        {
            m_lookupTable.OpenRead();
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="FileBackedDictionary{TKey, TValue}"/>.</exception>
        /// <exception cref="NotSupportedException">The <see cref="FileBackedDictionary{TKey, TValue}"/> is read-only.</exception>
        public void Add(TKey key, TValue value)
        {
            m_lookupTable.Add(key, value);
        }

        /// <summary>
        /// Adds an item to the <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="FileBackedDictionary{TKey, TValue}"/>.</param>
        /// <exception cref="NotSupportedException">The <see cref="FileBackedDictionary{TKey, TValue}"/> is read-only.</exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.
        /// This method also returns false if <paramref name="key"/> was not
        /// found in the original <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="NotSupportedException">The <see cref="FileBackedDictionary{TKey, TValue}"/> is read-only.</exception>
        public bool Remove(TKey key)
        {
            return m_lookupTable.Remove(key);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="FileBackedDictionary{TKey, TValue}"/>;
        /// otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="FileBackedDictionary{TKey, TValue}"/>.</param>
        /// <exception cref="NotSupportedException">The <see cref="FileBackedDictionary{TKey, TValue}"/> is read-only.</exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        /// <summary>
        /// Determines whether the <see cref="FileBackedDictionary{TKey, TValue}"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="FileBackedDictionary{TKey, TValue}"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="FileBackedDictionary{TKey, TValue}"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return m_lookupTable.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the <see cref="FileBackedDictionary{TKey, TValue}"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="FileBackedDictionary{TKey, TValue}"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="FileBackedDictionary{TKey, TValue}"/>.</param>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="FileBackedDictionary{TKey, TValue}"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_lookupTable.TryGetValue(key, out value);
        }

        /// <summary>
        /// Removes all items from the <see cref="FileBackedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">The <see cref="FileBackedDictionary{TKey, TValue}"/> is read-only. </exception>
        public void Clear()
        {
            m_lookupTable.Clear();
        }

        /// <summary>
        /// Copies the elements of the <see cref="FileBackedDictionary{TKey, TValue}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="FileBackedDictionary{TKey, TValue}"/>. The <see cref="Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source <see cref="FileBackedDictionary{TKey, TValue}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if ((object)array == null)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (m_lookupTable.Count > array.Length - arrayIndex)
                throw new ArgumentException("Not enough available space in array to copy elements from dictionary");

            foreach (KeyValuePair<TKey, TValue> item in this)
                array[arrayIndex++] = item;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return m_lookupTable.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerable used to iterate only the keys in the dictionary.
        /// </summary>
        /// <returns>An enumerable used to iterate only the keys in the dictionary.</returns>
        public IEnumerable<TKey> GetKeys()
        {
            return m_lookupTable.GetKeys();
        }

        /// <summary>
        /// Gets an enumerable used to iterate only the values in the dictionary.
        /// </summary>
        /// <returns>An enumerable used to iterate only the values in the dictionary.</returns>
        public IEnumerable<TValue> GetValues()
        {
            return this.Select(kvp => kvp.Value);
        }

        /// <summary>
        /// Defragments the item section of the dictionary,
        /// which gets fragmented after removing keys or updating values.
        /// </summary>
        public void Compact()
        {
            m_lookupTable.Compact();
        }

        /// <summary>
        /// Closes the file backing this dictionary.
        /// </summary>
        public void Close()
        {
            m_lookupTable.Close();
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="FileBackedDictionary{TKey, TValue}"/> object.
        /// </summary>
        public void Dispose()
        {
            m_lookupTable.Dispose();
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

        #endregion
    }
}
