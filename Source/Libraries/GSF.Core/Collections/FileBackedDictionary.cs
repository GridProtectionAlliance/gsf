//******************************************************************************************************
//  FileBackedHashSet.cs - Gbtc
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
//  10/28/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using GSF.IO.Checksums;

namespace GSF.Collections
{
    /// <summary>
    /// Represents a lookup table of key/value pairs backed by a file, with very little memory overhead.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the lookup table.</typeparam>
    /// <typeparam name="TValue">The type of the values in the lookup table.</typeparam>
    public class FileBackedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
        #region [ Members ]

        // Nested Types

        private class HeaderNode
        {
            public const int FixedSize = 4 * sizeof(long);

            public long Count;
            public long Capacity;
            public long ItemSectionPointer;
            public long EndOfFilePointer;
        }

        private class JournalNode
        {
            public const int FixedSize = sizeof(int) + 3 * sizeof(long) + sizeof(int);

            public const int None = 0;
            public const int Set = 1;
            public const int Delete = 2;
            public const int GrowLookupSection = 3;
            public const int GrowCapacity = 4;

            public int Operation;
            public long LookupPointer;
            public long ItemPointer;
            public long Sync;
            public int Checksum;
        }

        private class LookupNode
        {
            public const int FixedSize = sizeof(long);

            public long ItemPointer;
        }

        private class ItemNode
        {
            // Size of only the fixed size portion of
            // the item node (not including key and value)
            public const int FixedSize = 2 * sizeof(long) + sizeof(int);

            public long LookupPointer;
            public long NextItemPointer;
            public int HashCode;
            public TKey Key;
            public TValue Value;
        }

        // Constants
        private const long CollisionOffset = 4294967311L;
        private const double MaximumLoadFactor = 0.7D;
        private const int MaximumCollisions = 9;

        // Fields
        private HeaderNode m_headerNode;
        private JournalNode m_journalNode;

        private FileStream m_fileStream;
        private BinaryWriter m_fileWriter;
        private BinaryReader m_fileReader;
        private BinaryFormatter m_formatter;

        private IEqualityComparer<TKey> m_keyComparer;

        private string m_filePath;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> is not serializable.</exception>
        public FileBackedDictionary()
            : this(EqualityComparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are copied to this dictionary.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> is not serializable.</exception>
        public FileBackedDictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, EqualityComparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="keyComparer">The equality comparer used to compare keys in the dictionary.</param>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> is not serializable.</exception>
        public FileBackedDictionary(IEqualityComparer<TKey> keyComparer)
        {
            if (!typeof(TKey).IsSerializable)
                throw new InvalidOperationException("Unable to create FileBackedDictionary with keys that are not serializable");

            if (!typeof(TValue).IsSerializable)
                throw new InvalidOperationException("Unable to create FileBackedDictionary with values that are not serializable");

            if ((object)keyComparer != null)
                m_keyComparer = keyComparer;
            else
                m_keyComparer = EqualityComparer<TKey>.Default;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are copied to this dictionary.</param>
        /// <param name="keyComparer">The equality comparer used to compare keys in the dictionary.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> is not serializable.</exception>
        public FileBackedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer)
        {
            if ((object)dictionary == null)
                throw new ArgumentNullException("dictionary");

            if (!typeof(TKey).IsSerializable)
                throw new InvalidOperationException("Unable to create FileBackedDictionary with keys that are not serializable");

            if (!typeof(TValue).IsSerializable)
                throw new InvalidOperationException("Unable to create FileBackedDictionary with values that are not serializable");

            foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
                Add(kvp);

            if ((object)keyComparer != null)
                m_keyComparer = keyComparer;
            else
                m_keyComparer = EqualityComparer<TKey>.Default;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the path to the file backing this dictionary.
        /// </summary>
        public string FilePath
        {
            get
            {
                return m_filePath;
            }
            set
            {
                m_filePath = value;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get
            {
                if ((object)m_fileStream == null)
                    Open();

                return (int)m_headerNode.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                if ((object)m_fileStream == null)
                    Open();

                return (object)m_fileWriter == null;
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public TValue this[TKey key]
        {
            get
            {
                TValue value;

                if ((object)key == null)
                    throw new ArgumentNullException("key");

                if (!TryGetValue(key, out value))
                    throw new KeyNotFoundException("Item with the given key was not found in the dictionary");

                return value;
            }
            set
            {
                long lookupPointer;
                long itemPointer;
                ItemNode itemNode;

                long count;

                if ((object)key == null)
                    throw new ArgumentNullException("key");

                if (IsReadOnly)
                    throw new NotSupportedException("Unable to modify read-only dictionary");

                Find(key, out lookupPointer, out itemPointer);

                if (itemPointer >= m_headerNode.ItemSectionPointer)
                    count = m_headerNode.Count;
                else
                    count = m_headerNode.Count + 1;

                itemNode = new ItemNode();
                itemNode.LookupPointer = lookupPointer;
                itemNode.HashCode = key.GetHashCode();
                itemNode.Key = key;
                itemNode.Value = value;

                m_fileStream.Seek(m_headerNode.EndOfFilePointer, SeekOrigin.Begin);
                Write(itemNode);
                Set(lookupPointer, m_headerNode.EndOfFilePointer, count);
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TKey> Keys
        {
            get
            {
                ICollection<TKey> collection = new List<TKey>();

                long lookupPointer;
                long itemPointer;
                long count;

                if ((object)m_fileStream == null)
                    Open();

                lookupPointer = HeaderNode.FixedSize + JournalNode.FixedSize;
                count = 0L;

                while (count < m_headerNode.Count)
                {
                    itemPointer = ReadItemPointer(lookupPointer);

                    if (itemPointer >= m_headerNode.ItemSectionPointer)
                    {
                        m_fileStream.Seek(itemPointer + ItemNode.FixedSize, SeekOrigin.Begin);
                        collection.Add(ReadKey());
                    }

                    lookupPointer += LookupNode.FixedSize;
                    count++;
                }

                return collection;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TValue> Values
        {
            get
            {
                return this.Select(kvp => kvp.Value).ToList();
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
            if ((object)m_fileStream != null)
                throw new InvalidOperationException("File is already open");

            m_headerNode = new HeaderNode();
            m_journalNode = new JournalNode();

            if ((object)m_filePath == null)
                m_filePath = Path.GetTempFileName();

            try
            {
                // Open the file in read/write mode
                m_fileStream = new FileStream(m_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                m_fileWriter = new BinaryWriter(m_fileStream);
                m_fileReader = new BinaryReader(m_fileStream);
                m_formatter = new BinaryFormatter();

                if (m_fileStream.Length > HeaderNode.FixedSize + JournalNode.FixedSize)
                {
                    // Read the header node and the journal
                    // node from the start of the file
                    Read(m_headerNode);
                    Read(m_journalNode);

                    // If the journal node has an indicates that an operation was interrupted
                    // the last time this file was open, replay that operation to return the
                    // file to a consistent state
                    switch (m_journalNode.Operation)
                    {
                        case JournalNode.Set:
                            Set(m_journalNode.LookupPointer, m_journalNode.ItemPointer, m_journalNode.Sync);
                            break;

                        case JournalNode.Delete:
                            Delete(m_journalNode.LookupPointer, m_journalNode.Sync);
                            break;

                        case JournalNode.GrowLookupSection:
                            GrowLookupSection(m_journalNode.ItemPointer);
                            break;

                        case JournalNode.GrowCapacity:
                            GrowCapacity(m_journalNode.Sync);
                            break;
                    }
                }
                else
                {
                    // Create a new header node and write it to the start of the file
                    m_headerNode.Capacity = 10L;
                    m_headerNode.Count = 0L;
                    m_headerNode.ItemSectionPointer = HeaderNode.FixedSize + JournalNode.FixedSize + LookupNode.FixedSize * m_headerNode.Capacity;
                    m_headerNode.EndOfFilePointer = m_headerNode.ItemSectionPointer;
                    Write(m_headerNode);

                    // Create a new journal node and write it after the header node
                    m_journalNode.Operation = JournalNode.None;
                    m_journalNode.LookupPointer = 0L;
                    m_journalNode.ItemPointer = 0L;
                    m_journalNode.Sync = 0L;
                    Write(m_journalNode);

                    // Create the lookup table section
                    m_fileStream.Seek(m_headerNode.Capacity * LookupNode.FixedSize, SeekOrigin.Current);
                    m_fileWriter.Write(0L);
                    m_fileWriter.Write(0L);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Open the file in read-only mode
                m_fileStream = new FileStream(m_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                m_fileReader = new BinaryReader(m_fileStream);
                m_formatter = new BinaryFormatter();

                // Validate that the file format is valid
                if (m_fileStream.Length <= HeaderNode.FixedSize + JournalNode.FixedSize)
                    throw new FormatException("The format of the file is invalid");

                // Read the header node from the start of the file
                Read(m_headerNode);
            }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(TKey key, TValue value)
        {
            long lookupPointer;
            long itemPointer;
            ItemNode itemNode;

            if ((object)key == null)
                throw new ArgumentNullException("key");

            if (IsReadOnly)
                throw new NotSupportedException("Unable to modify read-only dictionary");

            Find(key, out lookupPointer, out itemPointer);

            if (itemPointer > m_headerNode.ItemSectionPointer)
                throw new ArgumentException("An element with the same key already exists");

            if (m_headerNode.Count + 1 > m_headerNode.Capacity * MaximumLoadFactor)
            {
                Grow();
                Find(key, out lookupPointer, out itemPointer);
            }

            itemNode = new ItemNode();
            itemNode.LookupPointer = lookupPointer;
            itemNode.HashCode = key.GetHashCode();
            itemNode.Key = key;
            itemNode.Value = value;

            m_fileStream.Seek(m_headerNode.EndOfFilePointer, SeekOrigin.Begin);
            Write(itemNode);
            Set(lookupPointer, m_headerNode.EndOfFilePointer, m_headerNode.Count + 1);
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.
        /// This method also returns false if <paramref name="key"/> was not
        /// found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public bool Remove(TKey key)
        {
            long lookupPointer;
            long itemPointer;

            if ((object)key == null)
                throw new ArgumentNullException("key");

            if (IsReadOnly)
                throw new NotSupportedException("Unable to modify read-only dictionary");

            Find(key, out lookupPointer, out itemPointer);

            if (itemPointer >= m_headerNode.ItemSectionPointer)
            {
                Delete(lookupPointer, m_headerNode.Count - 1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>;
        /// otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(TKey key)
        {
            long lookupPointer;
            long itemPointer;

            if ((object)key == null)
                throw new ArgumentNullException("key");

            if ((object)m_fileStream == null)
                Open();

            Find(key, out lookupPointer, out itemPointer);

            return (itemPointer >= m_headerNode.ItemSectionPointer);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            long lookupPointer;
            long itemPointer;

            if ((object)key == null)
                throw new ArgumentNullException("key");

            if ((object)m_fileStream == null)
                Open();

            Find(key, out lookupPointer, out itemPointer);

            if (itemPointer >= m_headerNode.ItemSectionPointer)
            {
                value = ReadValue();
                return true;
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            long lookupPointer;

            if (IsReadOnly)
                throw new NotSupportedException("Unable to modify read-only dictionary");

            lookupPointer = HeaderNode.FixedSize + JournalNode.FixedSize;

            while (m_headerNode.Count > 0L)
            {
                if (ReadItemPointer(lookupPointer) >= m_headerNode.ItemSectionPointer)
                    Delete(lookupPointer, m_headerNode.Count - 1);

                lookupPointer += LookupNode.FixedSize;
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if ((object)array == null)
                throw new ArgumentNullException("array");

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");

            if (m_headerNode.Count > array.Length - arrayIndex)
                throw new ArgumentException("Not enough available space in array to copy elements from dictionary");

            foreach (KeyValuePair<TKey, TValue> item in this)
                array[arrayIndex++] = item;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            ItemNode itemNode;

            long lookupPointer;
            long itemPointer;
            long count;

            if ((object)m_fileStream == null)
                Open();

            itemNode = new ItemNode();
            lookupPointer = HeaderNode.FixedSize + JournalNode.FixedSize;
            count = 0L;

            while (count < m_headerNode.Count)
            {
                itemPointer = ReadItemPointer(lookupPointer);

                if (itemPointer >= m_headerNode.ItemSectionPointer)
                {
                    m_fileStream.Seek(itemPointer, SeekOrigin.Begin);
                    Read(itemNode);
                    yield return new KeyValuePair<TKey, TValue>(itemNode.Key, itemNode.Value);
                    count++;
                }

                lookupPointer += LookupNode.FixedSize;
            }
        }

        /// <summary>
        /// Closes the file backing this dictionary.
        /// </summary>
        public void Close()
        {
            using (m_fileStream)
            using (m_fileWriter)
            using (m_fileReader)
            {
                m_fileStream = null;
                m_fileWriter = null;
                m_fileReader = null;
                m_formatter = null;
            }
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="FileBackedDictionary{TKey, TValue}"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FileBackedDictionary{TKey, TValue}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Close();
        }

        private void Grow()
        {
            LookupNode emptyNode = new LookupNode();
            LookupNode lookupNode = new LookupNode();
            ItemNode itemNode = new ItemNode();

            long newCapacity = 2 * m_headerNode.Capacity;
            long lookupTableSize = newCapacity * LookupNode.FixedSize;
            long minimumItemSectionPointer = HeaderNode.FixedSize + JournalNode.FixedSize + lookupTableSize;

            long lookupPointer;
            long itemPointer = m_headerNode.ItemSectionPointer;
            long copyPointer = m_headerNode.EndOfFilePointer;

            // If the lookup table is growing beyond the end of the file,
            // save time by writing a nextItemPointer beyond the end of the file.
            // This will allow us to begin copying item nodes directly to the new
            // item section so that we don't have to move item nodes multiple times
            if (minimumItemSectionPointer > m_headerNode.EndOfFilePointer)
            {
                m_fileStream.Seek(m_headerNode.EndOfFilePointer, SeekOrigin.Begin);
                m_fileWriter.Write(0L);
                m_fileWriter.Write(minimumItemSectionPointer);
                copyPointer = minimumItemSectionPointer;
            }

            // Begin copying item nodes to the end of the file while performing Set
            // operations to update the lookup nodes to point to these new item nodes
            while (itemPointer < minimumItemSectionPointer)
            {
                // Read the two pointers at the start of the item node
                m_fileStream.Seek(itemPointer, SeekOrigin.Begin);
                itemNode.LookupPointer = m_fileReader.ReadInt64();
                itemNode.NextItemPointer = m_fileReader.ReadInt64();
                itemNode.HashCode = m_fileReader.ReadInt32();

                // If the lookup node referenced by this item node does not point back
                // to the item node, then this is an orphaned node and may be skipped
                if (ReadItemPointer(itemNode.LookupPointer) != itemPointer)
                {
                    itemPointer = itemNode.NextItemPointer;
                    continue;
                }

                // Jump back to the item node and read the key and value,
                // then update the item pointer to point to the next item after this one
                m_fileStream.Seek(itemPointer + ItemNode.FixedSize, SeekOrigin.Begin);
                itemNode.Key = ReadKey();
                itemNode.Value = ReadValue();
                itemPointer = itemNode.NextItemPointer;

                // Write the item node to the end of the file, perform the
                // Set operation, and then update the copy pointer to point
                // to the next available location at the end of the file
                m_fileStream.Seek(copyPointer, SeekOrigin.Begin);
                Write(itemNode);
                Set(itemNode.LookupPointer, copyPointer, m_headerNode.Count);
                copyPointer = itemNode.NextItemPointer;
            }

            // Use the GrowLookupSection operation to move the item section pointer,
            // then clear the data in the new half of the lookup section
            GrowLookupSection(minimumItemSectionPointer);

            m_fileStream.Seek(HeaderNode.FixedSize + JournalNode.FixedSize + LookupNode.FixedSize * m_headerNode.Capacity, SeekOrigin.Begin);

            for (int i = 0; i < m_headerNode.Capacity; i++)
                Write(emptyNode);

            // Copy lookup nodes from the existing half of the lookup
            // section into the new half of the lookup section
            lookupPointer = HeaderNode.FixedSize + JournalNode.FixedSize;

            for (int i = 0; i < m_headerNode.Capacity; i++)
            {
                m_fileStream.Seek(lookupPointer, SeekOrigin.Begin);
                Read(lookupNode);

                if (lookupNode.ItemPointer >= m_headerNode.ItemSectionPointer)
                {
                    m_fileStream.Seek(lookupNode.ItemPointer + 2 * sizeof(long), SeekOrigin.Begin);
                    SeekToNewLookupChainEnd(m_fileReader.ReadInt32());
                    Write(lookupNode);
                }

                lookupPointer += LookupNode.FixedSize;
            }

            // This will clear the existing half of the lookup section,
            // update item nodes to point back to the new lookup section,
            // and increase the capacity of the lookup tables
            GrowCapacity(newCapacity);
        }

        private void SeekToNewLookupChainEnd(int hashCode)
        {
            long position = (uint)hashCode % m_headerNode.Capacity;
            long lookupPointer = GetLookupPointer(position) + m_headerNode.Capacity * LookupNode.FixedSize;

            while (ReadItemPointer(lookupPointer) >= m_headerNode.ItemSectionPointer)
            {
                position = (position + CollisionOffset) % m_headerNode.Capacity;
                lookupPointer = GetLookupPointer(position) + m_headerNode.Capacity * LookupNode.FixedSize;
            }

            m_fileStream.Seek(lookupPointer, SeekOrigin.Begin);
        }

        private void Find(TKey key, out long lookupPointer, out long itemPointer)
        {
            int hashCode = m_keyComparer.GetHashCode(key);
            long position = GetPosition((uint)hashCode);
            long lookup1 = GetLookupPointer(position);
            long lookup2 = lookup1 + (m_headerNode.Capacity / 2L) * LookupNode.FixedSize;
            long item1 = ReadItemPointer(lookup1);
            long item2 = ReadItemPointer(lookup2);

            long emptyPointer = 0L;
            int collisions = 0;

            // Loop until the item pointer
            // is pointing to the header node
            while (item1 > 0L || item2 > 0L)
            {
                if (item1 >= m_headerNode.ItemSectionPointer)
                {
                    // Determine if the item pointed to by
                    // lookup1 is the one we are trying to find
                    m_fileStream.Seek(item1 + ItemNode.FixedSize, SeekOrigin.Begin);

                    if (m_keyComparer.Equals(key, ReadKey()))
                    {
                        lookupPointer = lookup1;
                        itemPointer = item1;
                        return;
                    }
                }
                else if (emptyPointer == 0L)
                {
                    // Because the item pointer is not pointing to the
                    // item section of the file, this is an empty slot
                    // that may be used for a new item
                    emptyPointer = lookup1;
                }

                if (item2 >= m_headerNode.ItemSectionPointer)
                {
                    // Determine if the item pointed to by
                    // lookup2 is the one we are trying to find
                    m_fileStream.Seek(item2 + ItemNode.FixedSize, SeekOrigin.Begin);

                    if (m_keyComparer.Equals(key, ReadKey()))
                    {
                        lookupPointer = lookup2;
                        itemPointer = item2;
                        return;
                    }
                }
                else if (emptyPointer == 0L)
                {
                    // Because the item pointer is not pointing to the
                    // item section of the file, this is an empty slot
                    // that may be used for a new item
                    emptyPointer = lookup2;
                }

                // If the second lookup has never been occupied,
                // it is the end of the chain
                if (item2 == 0L)
                    break;

                // Track the number of collisions that
                // have occurred during this Find operation
                collisions++;

                if (collisions > MaximumCollisions)
                {
                    // Handle edge case where chains have grown too large
                    // because of frequent collisions and remove operations
                    Grow();

                    emptyPointer = 0L;
                    collisions = 0;

                    position = GetPosition((uint)hashCode);
                    lookup1 = GetLookupPointer(position);
                    lookup2 = lookup1 + (m_headerNode.Capacity / 2L) * LookupNode.FixedSize;
                    item1 = ReadItemPointer(lookup1);
                    item2 = ReadItemPointer(lookup2);
                }
                else
                {
                    // Update the position using the collision offset
                    // to find the next lookup node in the chain
                    position = GetPosition(position + CollisionOffset);
                    lookup1 = GetLookupPointer(position);
                    lookup2 = lookup1 + (m_headerNode.Capacity / 2L) * LookupNode.FixedSize;
                    item1 = ReadItemPointer(lookup1);
                    item2 = ReadItemPointer(lookup2);
                }
            }

            // If we made it here, it means that the item wasn't found in
            // the lookup tables and that both lookup1 and lookup2 are empty
            //
            // If the chain is not empty, an empty node should have been found in it;
            // otherwise, set lookupPointer to lookup1 as the first node in the chain
            if (emptyPointer > 0L)
                lookupPointer = emptyPointer;
            else
                lookupPointer = lookup1;

            // Set itemPointer to point to the header
            // node to indicate the item was not found
            itemPointer = 0L;
        }

        private void Set(long lookupPointer, long itemPointer, long count)
        {
            if ((count == m_headerNode.Count + 1 || count == m_headerNode.Count) && itemPointer >= m_headerNode.ItemSectionPointer && IsValidLookupPointer(lookupPointer))
            {
                if (m_journalNode.Operation != JournalNode.Set)
                {
                    // Write the set operation to the journal node
                    m_journalNode.Operation = JournalNode.Set;
                    m_journalNode.LookupPointer = lookupPointer;
                    m_journalNode.ItemPointer = itemPointer;
                    m_journalNode.Sync = count;
                    Write(m_journalNode);
                }

                // Perform the set operation
                WriteItemPointer(lookupPointer, itemPointer);
                m_headerNode.Count = count;
                m_headerNode.EndOfFilePointer = m_fileStream.Length;
                Write(m_headerNode);
            }

            // Clear the journal node
            m_journalNode.Operation = JournalNode.None;
            m_journalNode.LookupPointer = 0L;
            m_journalNode.ItemPointer = 0L;
            m_journalNode.Sync = 0;
            Write(m_journalNode);
        }

        private void Delete(long lookupPointer, long count)
        {
            if ((count == m_headerNode.Count - 1 || count == m_headerNode.Count) && IsValidLookupPointer(lookupPointer))
            {
                if (m_journalNode.Operation != JournalNode.Delete)
                {
                    // Write the delete operation to the journal node
                    m_journalNode.Operation = JournalNode.Delete;
                    m_journalNode.LookupPointer = lookupPointer;
                    m_journalNode.Sync = count;
                    Write(m_journalNode);
                }

                // Perform the delete operation
                WriteItemPointer(lookupPointer, 1L);
                m_headerNode.Count = count;
                Write(m_headerNode);
            }

            // Clear the journal node
            m_journalNode.Operation = JournalNode.None;
            m_journalNode.LookupPointer = 0L;
            m_journalNode.ItemPointer = 0L;
            m_journalNode.Sync = 0;
            Write(m_journalNode);
        }

        private void GrowLookupSection(long itemPointer)
        {
            if (itemPointer >= m_headerNode.ItemSectionPointer)
            {
                if (m_journalNode.Operation != JournalNode.GrowLookupSection)
                {
                    // Write the grow operation to the journal node
                    m_journalNode.Operation = JournalNode.GrowLookupSection;
                    m_journalNode.ItemPointer = itemPointer;
                    Write(m_journalNode);
                }

                // Perform the grow operation
                m_headerNode.ItemSectionPointer = itemPointer;
                Write(m_headerNode);
            }

            // Clear the journal node
            m_journalNode.Operation = JournalNode.None;
            m_journalNode.LookupPointer = 0L;
            m_journalNode.ItemPointer = 0L;
            m_journalNode.Sync = 0;
            Write(m_journalNode);
        }

        private void GrowCapacity(long capacity)
        {
            LookupNode emptyNode;
            long lookupPointer;
            long itemPointer;

            if (capacity == m_headerNode.Capacity * 2 || capacity == m_headerNode.Capacity)
            {
                if (m_journalNode.Operation != JournalNode.GrowCapacity)
                {
                    // Write the grow operation to the journal node
                    m_journalNode.Operation = JournalNode.GrowCapacity;
                    m_journalNode.Sync = capacity;
                    Write(m_journalNode);
                }

                // Perform the grow operation
                emptyNode = new LookupNode();
                m_fileStream.Seek(HeaderNode.FixedSize + JournalNode.FixedSize, SeekOrigin.Begin);

                for (int i = 0; i < capacity / 2; i++)
                    Write(emptyNode);

                for (int i = 0; i < capacity / 2; i++)
                {
                    lookupPointer = m_fileStream.Position;
                    itemPointer = ReadItemPointer(lookupPointer);

                    if (itemPointer >= m_headerNode.ItemSectionPointer)
                        WriteLookupPointer(lookupPointer, itemPointer);

                    m_fileStream.Seek(lookupPointer + LookupNode.FixedSize, SeekOrigin.Begin);
                }

                m_headerNode.Capacity = capacity;
                Write(m_headerNode);
            }

            // Clear the journal node
            m_journalNode.Operation = JournalNode.None;
            m_journalNode.LookupPointer = 0L;
            m_journalNode.ItemPointer = 0L;
            m_journalNode.Sync = 0;
            Write(m_journalNode);
        }

        private long GetPosition(long hashCode)
        {
            return hashCode % (m_headerNode.Capacity / 2L);
        }

        private long GetLookupPointer(long position)
        {
            return HeaderNode.FixedSize
                + JournalNode.FixedSize
                + LookupNode.FixedSize * position;
        }

        private bool IsValidLookupPointer(long lookupPointer)
        {
            long relativePointer = lookupPointer - HeaderNode.FixedSize - JournalNode.FixedSize;
            long remainder = relativePointer % LookupNode.FixedSize;
            return (remainder == 0L);
        }

        private void Write(HeaderNode node)
        {
            m_fileStream.Seek(0, SeekOrigin.Begin);
            m_fileWriter.Write(node.Count);
            m_fileWriter.Write(node.Capacity);
            m_fileWriter.Write(node.ItemSectionPointer);
            m_fileWriter.Write(node.EndOfFilePointer);
        }

        private void Write(JournalNode node)
        {
            Crc32 checksum = new Crc32();

            checksum.Update(node.Operation);
            checksum.Update((int)(node.LookupPointer >> sizeof(int)));
            checksum.Update((int)node.LookupPointer);
            checksum.Update((int)(node.ItemPointer >> sizeof(int)));
            checksum.Update((int)node.ItemPointer);
            checksum.Update((int)(node.Sync >> sizeof(int)));
            checksum.Update((int)node.Sync);
            node.Checksum = (int)checksum.Value;

            m_fileStream.Seek(HeaderNode.FixedSize, SeekOrigin.Begin);
            m_fileWriter.Write(node.Operation);
            m_fileWriter.Write(node.LookupPointer);
            m_fileWriter.Write(node.ItemPointer);
            m_fileWriter.Write(node.Sync);
            m_fileWriter.Write(node.Checksum);
            m_fileStream.Flush(true);
        }

        private void Write(LookupNode node)
        {
            m_fileWriter.Write(node.ItemPointer);
        }

        private void Write(ItemNode node)
        {
            long start;
            long end;

            start = m_fileStream.Position;
            m_fileWriter.Write(node.LookupPointer);
            m_fileStream.Seek(sizeof(long), SeekOrigin.Current);
            m_fileWriter.Write(node.HashCode);
            m_formatter.Serialize(m_fileStream, node.Key);
            m_formatter.Serialize(m_fileStream, node.Value);

            end = m_fileStream.Position;
            m_fileStream.Seek(start + sizeof(long), SeekOrigin.Begin);
            m_fileWriter.Write(end);
            m_fileStream.Seek(end, SeekOrigin.Begin);
        }

        private void WriteItemPointer(long lookupPointer, long itemPointer)
        {
            m_fileStream.Seek(lookupPointer, SeekOrigin.Begin);
            m_fileWriter.Write(itemPointer);
        }

        private void WriteLookupPointer(long lookupPointer, long itemPointer)
        {
            m_fileStream.Seek(itemPointer, SeekOrigin.Begin);
            m_fileWriter.Write(lookupPointer);
        }

        private void Read(HeaderNode node)
        {
            node.Count = m_fileReader.ReadInt64();
            node.Capacity = m_fileReader.ReadInt64();
            node.ItemSectionPointer = m_fileReader.ReadInt64();
            node.EndOfFilePointer = m_fileReader.ReadInt64();
        }

        private void Read(JournalNode node)
        {
            Crc32 checksum = new Crc32();

            node.Operation = m_fileReader.ReadInt32();
            node.LookupPointer = m_fileReader.ReadInt64();
            node.ItemPointer = m_fileReader.ReadInt64();
            node.Sync = m_fileReader.ReadInt64();
            node.Checksum = m_fileReader.ReadInt32();

            checksum.Update(node.Operation);
            checksum.Update((int)(node.LookupPointer >> sizeof(int)));
            checksum.Update((int)node.LookupPointer);
            checksum.Update((int)(node.ItemPointer >> sizeof(int)));
            checksum.Update((int)node.ItemPointer);
            checksum.Update((int)(node.Sync >> sizeof(int)));
            checksum.Update((int)node.Sync);

            if (node.Checksum != (int)checksum.Value)
            {
                node.Operation = JournalNode.None;
                node.Sync = 0;
                node.LookupPointer = 0L;
                node.ItemPointer = 0L;
                Write(node);
            }
        }

        private void Read(LookupNode node)
        {
            node.ItemPointer = m_fileReader.ReadInt64();
        }

        private void Read(ItemNode node)
        {
            node.LookupPointer = m_fileReader.ReadInt64();
            node.NextItemPointer = m_fileReader.ReadInt64();
            node.HashCode = m_fileReader.ReadInt32();
            node.Key = ReadKey();
            node.Value = ReadValue();
        }

        private long ReadItemPointer(long lookupPointer)
        {
            if (lookupPointer >= m_headerNode.ItemSectionPointer)
                return 0L;

            m_fileStream.Seek(lookupPointer, SeekOrigin.Begin);
            return m_fileReader.ReadInt64();
        }

        private TKey ReadKey()
        {
            return (TKey)m_formatter.Deserialize(m_fileStream);
        }

        private TValue ReadValue()
        {
            return (TValue)m_formatter.Deserialize(m_fileStream);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
