//******************************************************************************************************
//  FileBackedLookupTableBase.cs - Gbtc
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
//  12/02/2014 - Stephen C. Wills
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
    internal enum LookupTableType
    {
        /// <summary>
        /// Dictionary with keys and values in the item nodes.
        /// </summary>
        Dictionary,

        /// <summary>
        /// HashSet with lookup node markers for set operations.
        /// </summary>
        HashSet
    }

    internal class FileBackedLookupTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> 
    {
        #region [ Members ]

        // Nested Types

        protected class HeaderNode
        {
            public const int FixedSize = 4 * sizeof(long);

            public long Count;
            public long Capacity;
            public long ItemSectionPointer;
            public long EndOfFilePointer;
        }

        protected class JournalNode
        {
            public const int FixedSize = sizeof(int) + 3 * sizeof(long) + sizeof(int);

            public const int None = 0;
            public const int Set = 1;
            public const int Delete = 2;
            public const int GrowLookupSection = 3;
            public const int GrowCapacity = 4;
            public const int WriteItemNodePointers = 5;
            public const int Truncate = 6;

            public int Operation;
            public long LookupPointer;
            public long ItemPointer;
            public long Sync;
            public int Checksum;
        }

        protected class LookupNode
        {
            public long ItemPointer;
            public int Marker;
        }

        protected class ItemNode
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

        private class KeysEnumerable : IEnumerable<TKey>
        {
            private Func<IEnumerator<TKey>> m_getEnumeratorFunc;

            public KeysEnumerable(Func<IEnumerator<TKey>> getEnumeratorFunc)
            {
                m_getEnumeratorFunc = getEnumeratorFunc;
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                return m_getEnumeratorFunc();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        // Constants
        private const long CollisionOffset = 4294967311L;
        private const double MaximumLoadFactor = 0.5D;
        private const int MaximumCollisions = 9;

        // Fields
        private HeaderNode m_headerNode;
        private JournalNode m_journalNode;

        private FileStream m_fileStream;
        private BinaryWriter m_fileWriter;
        private BinaryReader m_fileReader;
        private BinaryFormatter m_formatter;

        private string m_filePath;
        private IEqualityComparer<TKey> m_keyComparer;

        private LookupTableType m_lookupTableType;
        private uint m_hashMod;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="lookupTableType">Type of the lookup table used to tweak the file format.</param>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> is not serializable.</exception>
        public FileBackedLookupTable(LookupTableType lookupTableType)
            : this(lookupTableType, EqualityComparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="lookupTableType">Type of the lookup table used to tweak the file format.</param>
        /// <param name="filePath">The path to the file used to store the lookup table.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> is not serializable.</exception>
        public FileBackedLookupTable(LookupTableType lookupTableType, string filePath)
            : this(lookupTableType, filePath, EqualityComparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="lookupTableType">Type of the lookup table used to tweak the file format.</param>
        /// <param name="keyComparer">The equality comparer used to compare keys in the dictionary.</param>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> is not serializable.</exception>
        public FileBackedLookupTable(LookupTableType lookupTableType, IEqualityComparer<TKey> keyComparer)
            : this(lookupTableType, Path.GetTempFileName(), keyComparer)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FileBackedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="lookupTableType">Type of the lookup table used to tweak the file format.</param>
        /// <param name="filePath">The path to the file used to store the lookup table.</param>
        /// <param name="keyComparer">The equality comparer used to compare keys in the dictionary.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Either <typeparamref name="TKey"/> or <typeparamref name="TValue"/> is not serializable.</exception>
        public FileBackedLookupTable(LookupTableType lookupTableType, string filePath, IEqualityComparer<TKey> keyComparer)
        {
            if (!typeof(TKey).IsSerializable)
                throw new InvalidOperationException("Unable to create FileBackedLookupTable with keys that are not serializable");

            if (lookupTableType == LookupTableType.Dictionary && !typeof(TValue).IsSerializable)
                throw new InvalidOperationException("Unable to create FileBackedLookupTable with values that are not serializable");

            if ((object)filePath == null)
                throw new ArgumentNullException("filePath");

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Path is zero-length or contains only whitespace", "filePath");

            m_lookupTableType = lookupTableType;

            FilePath = filePath;

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
        /// <exception cref="ArgumentException">FilePath contains one or more invalid characters as defined by <see cref="Path.GetInvalidPathChars"/>.</exception>
        /// <remarks>
        /// Changes to this property will cause the file to close if the file is already opened.
        /// Data will not be automatically written from the old file to the new file.
        /// </remarks>
        public string FilePath
        {
            get
            {
                return m_filePath;
            }
            set
            {
                char[] invalidPathChars;

                if (m_filePath != value)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        invalidPathChars = Path.GetInvalidPathChars();

                        if (value.Any(invalidPathChars.Contains))
                            throw new ArgumentException("Path contains one or more invalid characters", "value");
                    }

                    Close();
                    m_filePath = value;
                }
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
                itemNode.HashCode = m_keyComparer.GetHashCode(key);
                itemNode.Key = key;
                itemNode.Value = value;

                m_fileStream.Seek(m_headerNode.EndOfFilePointer, SeekOrigin.Begin);
                Write(itemNode);
                Set(lookupPointer, m_headerNode.EndOfFilePointer, count);
            }
        }

        private int LookupNodeSize
        {
            get
            {
                const int FixedSize = sizeof(long);
                int variableSize = (m_lookupTableType == LookupTableType.HashSet) ? sizeof(int) : 0;
                return FixedSize + variableSize;
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

            if (string.IsNullOrWhiteSpace(m_filePath))
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

                        case JournalNode.WriteItemNodePointers:
                            WriteItemNodePointers(m_journalNode.LookupPointer, m_journalNode.ItemPointer, m_journalNode.Sync);
                            break;

                        case JournalNode.Truncate:
                            Truncate(m_journalNode.ItemPointer);
                            break;
                    }
                }
                else
                {
                    // Create a new header node and write it to the start of the file
                    m_headerNode.Capacity = 10L;
                    m_headerNode.Count = 0L;
                    m_headerNode.ItemSectionPointer = HeaderNode.FixedSize + JournalNode.FixedSize + LookupNodeSize * m_headerNode.Capacity;
                    m_headerNode.EndOfFilePointer = m_headerNode.ItemSectionPointer;
                    Write(m_headerNode);

                    // Set the hash modifier to the largest prime
                    // that is at most as large as half the capacity
                    m_hashMod = 5u;

                    // Create a new journal node and write it after the header node
                    m_journalNode.Operation = JournalNode.None;
                    m_journalNode.LookupPointer = 0L;
                    m_journalNode.ItemPointer = 0L;
                    m_journalNode.Sync = 0L;
                    Write(m_journalNode);

                    // Create the lookup table section
                    m_fileStream.Seek(m_headerNode.Capacity * LookupNodeSize, SeekOrigin.Current);
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

            if (itemPointer >= m_headerNode.ItemSectionPointer)
                throw new ArgumentException("An element with the same key already exists");

            if (m_headerNode.Count + 1 > m_headerNode.Capacity * MaximumLoadFactor)
            {
                Grow();
                Find(key, out lookupPointer, out itemPointer);
            }

            itemNode = new ItemNode();
            itemNode.LookupPointer = lookupPointer;
            itemNode.HashCode = m_keyComparer.GetHashCode(key);
            itemNode.Key = key;
            itemNode.Value = value;

            m_fileStream.Seek(m_headerNode.EndOfFilePointer, SeekOrigin.Begin);
            Write(itemNode);
            Set(lookupPointer, m_headerNode.EndOfFilePointer, m_headerNode.Count + 1);
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <returns>True if the item was successfully added; false otherwise.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public bool TryAdd(TKey key, TValue value)
        {
            long lookupPointer;
            long itemPointer;
            ItemNode itemNode;

            if ((object)key == null)
                throw new ArgumentNullException("key");

            if (IsReadOnly)
                throw new NotSupportedException("Unable to modify read-only dictionary");

            Find(key, out lookupPointer, out itemPointer);

            if (itemPointer >= m_headerNode.ItemSectionPointer)
                return false;

            if (m_headerNode.Count + 1 > m_headerNode.Capacity * MaximumLoadFactor)
            {
                Grow();
                Find(key, out lookupPointer, out itemPointer);
            }

            itemNode = new ItemNode();
            itemNode.LookupPointer = lookupPointer;
            itemNode.HashCode = m_keyComparer.GetHashCode(key);
            itemNode.Key = key;
            itemNode.Value = value;

            m_fileStream.Seek(m_headerNode.EndOfFilePointer, SeekOrigin.Begin);
            Write(itemNode);
            Set(lookupPointer, m_headerNode.EndOfFilePointer, m_headerNode.Count + 1);

            return true;
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
        /// Attempts to find the lookup node for the corresponding
        /// key and, if found, marks the lookup node.
        /// </summary>
        /// <param name="key"></param>
        public bool TryMark(TKey key)
        {
            long lookupPointer;
            long itemPointer;

            if (m_lookupTableType == LookupTableType.HashSet)
            {
                Find(key, out lookupPointer, out itemPointer);

                if (itemPointer >= m_headerNode.ItemSectionPointer)
                {
                    m_fileStream.Seek(lookupPointer + sizeof(long), SeekOrigin.Begin);
                    m_fileWriter.Write(1);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether all occupied lookup nodes are marked.
        /// </summary>
        /// <returns>True if all occupied lookup nodes are marked; false otherwise.</returns>
        public bool AllMarked()
        {
            long lookupPointer;
            LookupNode lookupNode;

            if (m_lookupTableType == LookupTableType.HashSet)
            {
                lookupPointer = HeaderNode.FixedSize + JournalNode.FixedSize;
                lookupNode = new LookupNode();

                for (int i = 0; i < m_headerNode.Capacity; i++)
                {
                    m_fileStream.Seek(lookupPointer, SeekOrigin.Begin);
                    Read(lookupNode);

                    if (lookupNode.ItemPointer >= m_headerNode.ItemSectionPointer && lookupNode.Marker == 0)
                        return false;

                    lookupPointer += LookupNodeSize;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes all unmarked nodes from the hash set.
        /// </summary>
        public void RemoveMarked()
        {
            long lookupPointer;
            int mark;

            if (m_lookupTableType == LookupTableType.HashSet)
            {
                lookupPointer = HeaderNode.FixedSize + JournalNode.FixedSize;

                for (int i = 0; i < m_headerNode.Capacity; i++)
                {
                    m_fileStream.Seek(lookupPointer + sizeof(long), SeekOrigin.Begin);
                    mark = m_fileReader.ReadInt32();

                    if (mark != 0)
                        Delete(lookupPointer, m_headerNode.Count - 1);

                    lookupPointer += LookupNodeSize;
                }
            }
        }

        /// <summary>
        /// Removes all unmarked nodes from the hash set.
        /// </summary>
        public void RemoveUnmarked()
        {
            long lookupPointer;
            LookupNode lookupNode;

            if (m_lookupTableType == LookupTableType.HashSet)
            {
                lookupPointer = HeaderNode.FixedSize + JournalNode.FixedSize;
                lookupNode = new LookupNode();

                for (int i = 0; i < m_headerNode.Capacity; i++)
                {
                    m_fileStream.Seek(lookupPointer, SeekOrigin.Begin);
                    Read(lookupNode);

                    if (lookupNode.ItemPointer >= m_headerNode.ItemSectionPointer && lookupNode.Marker == 0)
                        Delete(lookupPointer, m_headerNode.Count - 1);

                    lookupPointer += LookupNodeSize;
                }
            }
        }

        /// <summary>
        /// Unmarks all lookup nodes in the hash set.
        /// </summary>
        public void UnmarkAll()
        {
            long lookupPointer;

            if (m_lookupTableType == LookupTableType.HashSet)
            {
                lookupPointer = HeaderNode.FixedSize + JournalNode.FixedSize;

                for (int i = 0; i < m_headerNode.Capacity; i++)
                {
                    m_fileStream.Seek(lookupPointer + sizeof(long), SeekOrigin.Begin);
                    m_fileWriter.Write(0);
                    lookupPointer += LookupNodeSize;
                }
            }
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

                lookupPointer += LookupNodeSize;
            }

            Truncate(m_headerNode.ItemSectionPointer);
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
                    // ReSharper disable once PossibleNullReferenceException
                    m_fileStream.Seek(itemPointer, SeekOrigin.Begin);
                    Read(itemNode);
                    yield return new KeyValuePair<TKey, TValue>(itemNode.Key, itemNode.Value);
                    count++;
                }

                lookupPointer += LookupNodeSize;
            }
        }

        /// <summary>
        /// Gets an enumerable used to iterate only the keys in the dictionary.
        /// </summary>
        /// <returns>An enumerable used to iterate only the keys in the dictionary.</returns>
        public IEnumerable<TKey> GetKeys()
        {
            return new KeysEnumerable(GetKeysEnumerator);
        }

        /// <summary>
        /// Defragments the item section of the dictionary,
        /// which gets fragmented after removing keys or updating values.
        /// </summary>
        public void Compact()
        {
            byte[] bytes = null;

            long item1;
            long lookup1;
            int length1;
            bool orphan1;

            long item2;
            long lookup2;
            int length2;
            bool orphan2;

            long item3;

            if (IsReadOnly)
                throw new NotSupportedException("Unable to modify read-only dictionary");

            if (m_headerNode.ItemSectionPointer == m_headerNode.EndOfFilePointer)
                return;

            // Initialize item1, lookup1, and item2 pointers
            item1 = m_headerNode.ItemSectionPointer;
            lookup1 = ReadLookupPointer(item1);
            item2 = m_fileReader.ReadInt64();

            // Determine whether item1 is an orphaned node
            orphan1 = ReadItemPointer(lookup1) != item1;

            while (item2 < m_headerNode.EndOfFilePointer)
            {
                // Update lookup2 and item3 pointers
                lookup2 = ReadLookupPointer(item2);
                item3 = m_fileReader.ReadInt64();

                // Determine whether item2 is an orphaned node
                orphan2 = ReadItemPointer(lookup2) != item2;

                if (!orphan1)
                {
                    // Skip nodes that are not orphaned at
                    // the beginning of the item section
                    item1 = item2;
                    lookup1 = lookup2;
                    orphan1 = orphan2;
                }
                else
                {
                    if (orphan2)
                    {
                        // Combine the orphaned nodes
                        WriteItemNodePointers(lookup1, item1, item3);
                    }
                    else
                    {
                        // Determine the length of the two nodes
                        length1 = (int)(item2 - item1);
                        length2 = (int)(item3 - item2);

                        // Allocate enough memory to move item2
                        if ((object)bytes == null || bytes.Length < length2)
                            bytes = new byte[length2];

                        // Read item2 into memory
                        m_fileStream.Seek(item2, SeekOrigin.Begin);
                        m_fileStream.Read(bytes, 0, length2);

                        // Check to see if item1 is at least large enough
                        // to fully contain item2 plus an orphaned node
                        if (length2 + 2 * sizeof(long) < length1)
                        {
                            // Write data to the body of the orphaned node
                            m_fileStream.Seek(item1 + 2 * sizeof(long), SeekOrigin.Begin);
                            m_fileStream.Write(bytes, 2 * sizeof(long), length2 - 2 * sizeof(long));

                            // Position nextItemPointer of the soon
                            // to be orphaned node, pointing at item2
                            m_fileWriter.Write(lookup1);
                            m_fileWriter.Write(item2);

                            // Update the pointers in the orphaned item node,
                            // then point the lookup node to the orphaned node
                            WriteItemNodePointers(lookup2, item1, item1 + length2);
                            Set(lookup2, item1, m_headerNode.Count);

                            // The left over empty space and the node at item2
                            // have become orphaned nodes so combine them
                            WriteItemNodePointers(lookup1, item1 + length2, item3);

                            // Update item1 for the next iteration
                            item1 += length2;
                        }
                        else
                        {
                            // Write item2 beyond the end of the item section
                            m_fileStream.Seek(m_headerNode.EndOfFilePointer, SeekOrigin.Begin);
                            m_fileStream.Write(bytes, 0, length2);
                            
                            // Fix the nextItemPointer
                            m_fileStream.Seek(m_headerNode.EndOfFilePointer + sizeof(long), SeekOrigin.Begin);
                            m_fileWriter.Write(m_headerNode.EndOfFilePointer + length2);

                            // Update the lookup node to point to the new location for item2
                            Set(lookup2, m_headerNode.EndOfFilePointer, m_headerNode.Count);

                            // Combine the orphaned item1 and the newly orphaned item2
                            WriteItemNodePointers(lookup1, item1, item3);
                        }
                    }
                }

                // Update item2 for the next iteration
                item2 = item3;
            }

            if (!orphan1)
                Truncate(item1);
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
            long lookupTableSize = newCapacity * LookupNodeSize;
            long minimumItemSectionPointer = HeaderNode.FixedSize + JournalNode.FixedSize + lookupTableSize;

            long lookupPointer;
            long itemPointer = m_headerNode.ItemSectionPointer;
            long copyPointer = m_headerNode.EndOfFilePointer;

            // If the lookup table is growing beyond the end of the file,
            // save time by writing a nextItemPointer beyond the end of the file.
            // This will allow us to begin copying item nodes directly to the new
            // item section so that we don't have to move item nodes multiple times
            if (minimumItemSectionPointer > m_headerNode.EndOfFilePointer + sizeof(long) * 2)
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
                copyPointer = m_headerNode.EndOfFilePointer;
            }

            // Use the GrowLookupSection operation to move the item section pointer,
            // then clear the data in the new half of the lookup section
            GrowLookupSection(itemPointer);

            m_fileStream.Seek(HeaderNode.FixedSize + JournalNode.FixedSize + LookupNodeSize * m_headerNode.Capacity, SeekOrigin.Begin);

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

                lookupPointer += LookupNodeSize;
            }

            // This will clear the existing half of the lookup section,
            // update item nodes to point back to the new lookup section,
            // and increase the capacity of the lookup tables
            GrowCapacity(newCapacity);
        }

        private void SeekToNewLookupChainEnd(int hashCode)
        {
            long position = (uint)hashCode % m_hashMod;
            long lookupPointer = GetLookupPointer(position) + m_headerNode.Capacity * LookupNodeSize;

            while (ReadItemPointer(lookupPointer) >= m_headerNode.ItemSectionPointer)
            {
                position = (position + CollisionOffset) % m_headerNode.Capacity;
                lookupPointer = GetLookupPointer(position) + m_headerNode.Capacity * LookupNodeSize;
            }

            m_fileStream.Seek(lookupPointer, SeekOrigin.Begin);
        }

        private void Find(TKey key, out long lookupPointer, out long itemPointer)
        {
            int hashCode = m_keyComparer.GetHashCode(key);
            long position = (uint)hashCode % m_hashMod;
            long lookup1 = GetLookupPointer(position);
            long lookup2 = lookup1 + (m_headerNode.Capacity / 2L) * LookupNodeSize;
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

                    position = (uint)hashCode % m_hashMod;
                    lookup1 = GetLookupPointer(position);
                    lookup2 = lookup1 + (m_headerNode.Capacity / 2L) * LookupNodeSize;
                    item1 = ReadItemPointer(lookup1);
                    item2 = ReadItemPointer(lookup2);
                }
                else
                {
                    // Update the position using the collision offset
                    // to find the next lookup node in the chain
                    position = GetPosition(position + CollisionOffset);
                    lookup1 = GetLookupPointer(position);
                    lookup2 = lookup1 + (m_headerNode.Capacity / 2L) * LookupNodeSize;
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
            long nextItemPointer;

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
                nextItemPointer = ReadNextItemPointer(itemPointer);

                if (nextItemPointer > m_headerNode.EndOfFilePointer)
                    m_headerNode.EndOfFilePointer = nextItemPointer;

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

                    m_fileStream.Seek(lookupPointer + LookupNodeSize, SeekOrigin.Begin);
                }

                m_headerNode.Capacity = capacity;
                Write(m_headerNode);

                // Set the hash modifier to the largest prime
                // that is at most as large as half the capacity
                UpdateHashMod();
            }

            // Clear the journal node
            m_journalNode.Operation = JournalNode.None;
            m_journalNode.LookupPointer = 0L;
            m_journalNode.ItemPointer = 0L;
            m_journalNode.Sync = 0;
            Write(m_journalNode);
        }

        private void WriteItemNodePointers(long lookupPointer, long itemPointer, long nextItemPointer)
        {
            if (itemPointer >= m_headerNode.ItemSectionPointer && nextItemPointer > itemPointer)
            {
                if (m_journalNode.Operation != JournalNode.WriteItemNodePointers)
                {
                    // Write the grow operation to the journal node
                    m_journalNode.Operation = JournalNode.WriteItemNodePointers;
                    m_journalNode.LookupPointer = lookupPointer;
                    m_journalNode.ItemPointer = itemPointer;
                    m_journalNode.Sync = nextItemPointer;
                    Write(m_journalNode);
                }

                // Perform the write operation
                m_fileStream.Seek(itemPointer, SeekOrigin.Begin);
                m_fileWriter.Write(lookupPointer);
                m_fileWriter.Write(nextItemPointer);
            }

            // Clear the journal node
            m_journalNode.Operation = JournalNode.None;
            m_journalNode.LookupPointer = 0L;
            m_journalNode.ItemPointer = 0L;
            m_journalNode.Sync = 0L;
            Write(m_journalNode);
        }

        private void Truncate(long itemPointer)
        {
            if (itemPointer >= m_headerNode.ItemSectionPointer && itemPointer < m_headerNode.EndOfFilePointer)
            {
                if (m_journalNode.Operation != JournalNode.Truncate)
                {
                    // Write the grow operation to the journal node
                    m_journalNode.Operation = JournalNode.Truncate;
                    m_journalNode.ItemPointer = itemPointer;
                    Write(m_journalNode);
                }

                // Perform the truncate operation
                m_headerNode.EndOfFilePointer = itemPointer;
                Write(m_headerNode);
                m_fileStream.SetLength(itemPointer);
            }

            // Clear the journal node
            m_journalNode.Operation = JournalNode.None;
            m_journalNode.LookupPointer = 0L;
            m_journalNode.ItemPointer = 0L;
            m_journalNode.Sync = 0L;
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
                + LookupNodeSize * position;
        }

        private bool IsValidLookupPointer(long lookupPointer)
        {
            long relativePointer = lookupPointer - HeaderNode.FixedSize - JournalNode.FixedSize;
            long remainder = relativePointer % LookupNodeSize;
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

            if (m_lookupTableType == LookupTableType.HashSet)
                m_fileWriter.Write(node.Marker);
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

            if (m_lookupTableType == LookupTableType.Dictionary)
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

            // Set the hash modifier to the largest prime
            // that is at most as large as half the capacity
            UpdateHashMod();
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

            if (m_lookupTableType == LookupTableType.HashSet)
                node.Marker = m_fileReader.ReadInt32();
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

        private long ReadLookupPointer(long itemPointer)
        {
            m_fileStream.Seek(itemPointer, SeekOrigin.Begin);
            return m_fileReader.ReadInt64();
        }

        private long ReadNextItemPointer(long itemPointer)
        {
            m_fileStream.Seek(itemPointer + sizeof(long), SeekOrigin.Begin);
            return m_fileReader.ReadInt64();
        }

        private TKey ReadKey()
        {
            return (TKey)m_formatter.Deserialize(m_fileStream);
        }

        private TValue ReadValue()
        {
            if (m_lookupTableType == LookupTableType.Dictionary)
                return (TValue)m_formatter.Deserialize(m_fileStream);

            return default(TValue);
        }

        private void UpdateHashMod()
        {
            long lookupCapacity = m_headerNode.Capacity / 2L;

            if (lookupCapacity >= uint.MaxValue)
            {
                m_hashMod = 4294967291u;
            }
            else
            {
                m_hashMod = (uint)(lookupCapacity - 1L | 1L);

                while (!IsPrime(m_hashMod))
                    m_hashMod -= 2;
            }
        }

        private bool IsPrime(long n)
        {
            long sqrt = (long)Math.Sqrt(n);

            for (long i = 2; i <= sqrt; i++)
            {
                if (n % i == 0)
                    return false;
            }

            return true;
        }

        private IEnumerator<TKey> GetKeysEnumerator()
        {
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
                    // ReSharper disable once PossibleNullReferenceException
                    m_fileStream.Seek(itemPointer + ItemNode.FixedSize, SeekOrigin.Begin);
                    yield return ReadKey();
                    count++;
                }

                lookupPointer += LookupNodeSize;
            }
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
