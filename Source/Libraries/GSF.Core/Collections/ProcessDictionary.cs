//******************************************************************************************************
//  ProcessDictionary.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  11/28/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace GSF.Collections
{
    /// <summary>
    /// Represents a thread-safe (via locking) keyed collection of items, based on <see cref="DictionaryList{TKey,TValue}"/>, that get processed on independent threads with a consumer provided function.
    /// </summary>
    /// <typeparam name="TKey">Type of keys used to reference process items.</typeparam>
    /// <typeparam name="TValue">Type of values to process.</typeparam>
    /// <remarks>
    /// <para>This class acts as a strongly-typed sorted dictionary of objects to be processed.</para>
    /// <para>Note that the <see cref="ProcessDictionary{TKey,TValue}"/> will not start processing until the Start method is called.</para>
    /// <para>Because this <see cref="ProcessDictionary{TKey,TValue}"/> represents a dictionary style collection, all keys must be unique.</para>
    /// <para>
    /// Be aware that this class is based on a <see cref="DictionaryList{TKey,TValue}"/> (i.e., a <see cref="SortedList{TKey,TValue}"/>
    /// that implements <see cref="IList{T}"/>), and since items in this kind of list are automatically sorted, items will be processed
    /// in "sorted" order regardless of the order in which they are added to the list.
    /// </para>
    /// <para>
    /// Important note about using an "Integer" as the key for this class: because the <see cref="ProcessDictionary{TKey,TValue}"/> base class must
    /// implement IList, a normal dictionary cannot be used for the base class. IDictionary implementations
    /// do not normally implement the IList interface because of ambiguity that is caused when implementing
    /// an integer key. For example, if you implement this class with a key of type "Integer," you will not
    /// be able to access items in the <see cref="ProcessDictionary{TKey,TValue}"/> by index without "casting" the 
    /// <see cref="ProcessDictionary{TKey,TValue}"/> as IList. This is because the Item property in both the IDictionary and IList would
    /// have the same parameters (see the <see cref="DictionaryList{TKey,TValue}"/> class for more details.).
    /// </para>
    /// </remarks>
    public class ProcessDictionary<TKey, TValue> : ProcessQueue<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
    {
        #region [ Members ]

        // Delegates

        /// <summary>
        /// Function signature that defines a method to process a key and value one at a time.
        /// </summary>
        /// <param name="key">key to be processed.</param>
        /// <param name="value">value to be processed.</param>
        /// <remarks>
        /// <para>Required unless <see cref="ProcessDictionary{TKey,TValue}.ProcessItemsFunction"/> is implemented.</para>
        /// <para>Used when creating a <see cref="ProcessDictionary{TKey,TValue}"/> to process one item at a time.</para>
        /// <para>Asynchronous <see cref="ProcessDictionary{TKey,TValue}"/> will process individual items on multiple threads</para>
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public new delegate void ProcessItemFunctionSignature(TKey key, TValue value);

        /// <summary>
        /// Function signature that determines if a key and value can be currently processed.
        /// </summary>
        /// <param name="key">key to be checked for processing availability.</param>
        /// <param name="value">value to be checked for processing availability.</param>
        /// <returns>True, if key and value can be processed.</returns>
        /// <remarks>
        /// <para>Implementation of this function is optional. It will be assumed that an item can be processed if this
        /// function is not defined</para>
        /// <para>Items must eventually get to a state where they can be processed or they will remain in the <see cref="ProcessDictionary{TKey,TValue}"/>
        /// indefinitely.</para>
        /// <para>
        /// Note that when this function is implemented and <see cref="QueueProcessingStyle"/> = ManyAtOnce (i.e., 
        /// <see cref="ProcessDictionary{TKey,TValue}.ProcessItemsFunction"/> is defined), then each item presented
        /// for processing must evaluate as "CanProcessItem = True" before any items are processed.
        /// </para>
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public new delegate bool CanProcessItemFunctionSignature(TKey key, TValue value);

        // Fields
        private ProcessItemFunctionSignature m_processItemFunction;
        private CanProcessItemFunctionSignature m_canProcessItemFunction;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a <see cref="ProcessDictionary{TKey, TValue}"/> based on the generic <see cref="DictionaryList{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="processItemFunction">A delegate <see cref="ProcessItemFunctionSignature"/> that defines a function signature to process a key and value one at a time.</param>
        /// <param name="processInterval">A <see cref="double"/> which represents the process interval.</param>
        /// <param name="maximumThreads">An <see cref="int"/> that represents the max number of threads to use.</param>
        /// <param name="processTimeout">An <see cref="int"/> that represents the amount of time before a process times out.</param>
        /// <param name="requeueOnTimeout">A <see cref="bool"/> value that indicates whether the process should requeue the item after a timeout.</param>
        /// <param name="requeueOnException">A <see cref="bool"/> value that indicates whether the process should requeue the item after an exception.</param>
        public ProcessDictionary(ProcessItemFunctionSignature processItemFunction, double processInterval = DefaultProcessInterval, int maximumThreads = DefaultMaximumThreads, int processTimeout = DefaultProcessTimeout, bool requeueOnTimeout = DefaultRequeueOnTimeout, bool requeueOnException = DefaultRequeueOnException)
            : this(processItemFunction, null, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
        }

        /// <summary>
        /// Creates a <see cref="ProcessDictionary{TKey, TValue}"/> based on the generic <see cref="DictionaryList{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="processItemFunction">A delegate <see cref="ProcessItemFunctionSignature"/> that defines a function signature to process a key and value one at a time.</param>
        /// <param name="canProcessItemFunction">A delegate <see cref="CanProcessItemFunctionSignature"/> that determines if a key and value can currently be processed.</param>
        /// <param name="processInterval">A <see cref="double"/> which represents the process interval.</param>
        /// <param name="maximumThreads">An <see cref="int"/> that represents the max number of threads to use.</param>
        /// <param name="processTimeout">An <see cref="int"/> that represents the amount of time before a process times out.</param>
        /// <param name="requeueOnTimeout">A <see cref="bool"/> value that indicates whether the process should requeue the item after a timeout.</param>
        /// <param name="requeueOnException">A <see cref="bool"/> value that indicates whether the process should requeue the item after an exception.</param>
        public ProcessDictionary(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction = null, double processInterval = DefaultProcessInterval, int maximumThreads = DefaultMaximumThreads, int processTimeout = DefaultProcessTimeout, bool requeueOnTimeout = DefaultRequeueOnTimeout, bool requeueOnException = DefaultRequeueOnException)
            : base(null, null, null, new DictionaryList<TKey, TValue>(), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
            m_processItemFunction = processItemFunction; // Defining this function creates a ProcessingStyle = OneAtATime keyed process queue
            m_canProcessItemFunction = canProcessItemFunction;

            // Assigns translator functions for base class.
            base.ProcessItemFunction = ProcessKeyedItem;

            if ((object)m_canProcessItemFunction != null)
                base.CanProcessItemFunction = CanProcessKeyedItem;
        }

        /// <summary>
        /// Creates a bulk-item <see cref="ProcessDictionary{TKey, TValue}"/> based on the generic <see cref="DictionaryList{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="processItemsFunction">A delegate <see cref="ProcessItemFunctionSignature"/> that defines a function signature to process multiple items at once.</param>
        /// <param name="processInterval">A <see cref="double"/> which represents the process interval.</param>
        /// <param name="maximumThreads">An <see cref="int"/> that represents the max number of threads to use.</param>
        /// <param name="processTimeout">An <see cref="int"/> that represents the amount of time before a process times out.</param>
        /// <param name="requeueOnTimeout">A <see cref="bool"/> value that indicates whether the process should requeue the item after a timeout.</param>
        /// <param name="requeueOnException">A <see cref="bool"/> value that indicates whether the process should requeue the item after an exception.</param>
        public ProcessDictionary(ProcessItemsFunctionSignature processItemsFunction, double processInterval = DefaultProcessInterval, int maximumThreads = DefaultMaximumThreads, int processTimeout = DefaultProcessTimeout, bool requeueOnTimeout = DefaultRequeueOnTimeout, bool requeueOnException = DefaultRequeueOnException)
            : this(processItemsFunction, null, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
        }

        /// <summary>
        /// Creates a bulk-item <see cref="ProcessDictionary{TKey, TValue}"/> based on the generic <see cref="DictionaryList{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="processItemsFunction">A delegate <see cref="ProcessItemFunctionSignature"/> that defines a function signature to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">A delegate <see cref="CanProcessItemFunctionSignature"/> that determines if a key and value can currently be processed.</param>
        /// <param name="processInterval">A <see cref="double"/> which represents the process interval.</param>
        /// <param name="maximumThreads">An <see cref="int"/> that represents the max number of threads to use.</param>
        /// <param name="processTimeout">An <see cref="int"/> that represents the amount of time before a process times out.</param>
        /// <param name="requeueOnTimeout">A <see cref="bool"/> value that indicates whether the process should requeue the item after a timeout.</param>
        /// <param name="requeueOnException">A <see cref="bool"/> value that indicates whether the process should requeue the item after an exception.</param>
        public ProcessDictionary(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction = null, double processInterval = DefaultProcessInterval, int maximumThreads = DefaultMaximumThreads, int processTimeout = DefaultProcessTimeout, bool requeueOnTimeout = DefaultRequeueOnTimeout, bool requeueOnException = DefaultRequeueOnException)
            : base(null, processItemsFunction, null, new DictionaryList<TKey, TValue>(), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
            m_canProcessItemFunction = canProcessItemFunction;

            // Assigns translator functions for base class.
            if ((object)m_canProcessItemFunction != null)
                base.CanProcessItemFunction = CanProcessKeyedItem;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the user function used to process items in the list one at a time.
        /// </summary>
        /// <remarks>
        /// <para>This function and <see cref="ProcessDictionary{TKey,TValue}.ProcessItemFunction"/> cannot be defined at the same time.</para>
        /// <para>A <see cref="ProcessDictionary{TKey,TValue}"/> must be defined to process either a single item at a time or many items at once.</para>
        /// <para>Implementation of this function makes <see cref="QueueProcessingStyle"/> = OneAtATime.</para>
        /// </remarks>
        public new virtual ProcessItemFunctionSignature ProcessItemFunction
        {
            get
            {
                return m_processItemFunction;
            }
            set
            {
                if ((object)value != null)
                {
                    m_processItemFunction = value;

                    // Assigns translator function for base class.
                    base.ProcessItemFunction = ProcessKeyedItem;
                }
            }
        }

        /// <summary>
        /// Gets or sets the user function used to process multiple items in the list at once.
        /// </summary>
        /// <remarks>
        /// <para>This function and <see cref="ProcessDictionary{TKey,TValue}.ProcessItemFunction"/> cannot be defined at the same time.</para>
        /// <para>A <see cref="ProcessDictionary{TKey,TValue}"/> must be defined to process either a single item at a time or many items at once.</para>
        /// <para>Implementation of this function makes <see cref="QueueProcessingStyle"/> = ManyAtOnce.</para>
        /// </remarks>
        public override ProcessItemsFunctionSignature ProcessItemsFunction
        {
            get
            {
                return base.ProcessItemsFunction;
            }
            set
            {
                if ((object)value != null)
                {
                    m_processItemFunction = null;
                    base.ProcessItemsFunction = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the user function used to determine if an item is ready to be processed.
        /// </summary>
        public new virtual CanProcessItemFunctionSignature CanProcessItemFunction
        {
            get
            {
                return m_canProcessItemFunction;
            }
            set
            {
                m_canProcessItemFunction = value;

                // Assigns translator function for base class.
                if ((object)m_canProcessItemFunction == null)
                    base.CanProcessItemFunction = null;
                else
                    base.CanProcessItemFunction = CanProcessKeyedItem;
            }
        }

        /// <summary>
        /// Gets the class name.
        /// </summary>
        /// <returns>Class name.</returns>
        /// <remarks>
        /// <para>This name is used for class identification in strings (e.g., used in error message).</para>
        /// <para>Derived classes should override this method with a proper class name.</para>
        /// </remarks>
        public override string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation
        /// throws a KeyNotFoundException, and a set operation creates a new element with the specified key.</returns>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        /// <exception cref="KeyNotFoundException">The property is retrieved and key does not exist in the collection.</exception>
        public TValue this[TKey key]
        {
            get
            {
                lock (SyncRoot)
                {
                    return InternalDictionary[key];
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    InternalDictionary[key] = value;
                    DataAdded();
                }
            }
        }

        /// <summary>Gets an ICollection containing the keys of the <see cref="ProcessDictionary{TKey,TValue}"/>.</summary>
        /// <returns>An ICollection containing the keys of the <see cref="ProcessDictionary{TKey,TValue}"/>.</returns>
        public ICollection<TKey> Keys
        {
            get
            {
                return InternalDictionary.Keys;
            }
        }

        /// <summary>Gets an ICollection containing the values of the <see cref="ProcessDictionary{TKey,TValue}"/>.</summary>
        /// <returns>An ICollection containing the values of the <see cref="ProcessDictionary{TKey,TValue}"/>.</returns>
        public ICollection<TValue> Values
        {
            get
            {
                return InternalDictionary.Values;
            }
        }

        /// <summary>
        /// Gets the internal sorted dictionary for direct use by derived classes.
        /// </summary>
        protected DictionaryList<TKey, TValue> InternalDictionary
        {
            get
            {
                return (DictionaryList<TKey, TValue>)InternalEnumerable;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ProcessDictionary{TKey, TValue}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        m_processItemFunction = null;
                        m_canProcessItemFunction = null;
                    }
                }
                finally
                {
                    m_disposed = true; // Prevent duplicate dispose.
                    base.Dispose(disposing); // Call base class Dispose().
                }
            }
        }

        #region [ Item Processing Translation Functions ]

        // These functions act as intermediate "translators" between the delegate implementations of
        // ProcessQueue and ProcessDictionary. Users implementing a ProcessDictionary will likely be
        // thinking in terms of "keys" and "values", and not a KeyValuePair structure. Note that the
        // bulk item ProcessItems delegate is not translated since an array of KeyValuePair structures
        // would make more sense and be more efficient than two separate arrays of keys and values.
        private void ProcessKeyedItem(KeyValuePair<TKey, TValue> item)
        {
            m_processItemFunction(item.Key, item.Value);
        }

        private bool CanProcessKeyedItem(KeyValuePair<TKey, TValue> item)
        {
            return m_canProcessItemFunction(item.Key, item.Value);
        }

        //private void ProcessKeyedItems(KeyValuePair<TKey, TValue>[] items)
        //{
        //    // Copies an array of KeyValuePairs into an array of keys and values.
        //    TKey[] keys = new TKey[items.Length];
        //    TValue[] values = new TValue[items.Length];
        //    KeyValuePair<TKey, TValue> kvPair;

        //    for (int x = 0; x < items.Length; x++)
        //    {
        //        kvPair = items[x];
        //        keys[x] = kvPair.Key;
        //        values[x] = kvPair.Value;
        //    }

        //    m_processItemsFunction(keys, values);
        //}

        #endregion

        #region [ Generic IDictionary(Of TKey, TValue) Implementation ]

        /// <summary>Adds an element with the provided key and value to the <see cref="ProcessDictionary{TKey,TValue}"/>.</summary>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <exception cref="NotSupportedException">The <see cref="ProcessDictionary{TKey,TValue}"/> is read-only.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="ProcessDictionary{TKey,TValue}"/>.</exception>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public void Add(TKey key, TValue value)
        {
            lock (SyncRoot)
            {
                InternalDictionary.Add(key, value);
                DataAdded();
            }
        }

        /// <summary>Determines whether the <see cref="ProcessDictionary{TKey,TValue}"/> contains an element with the specified key.</summary>
        /// <returns>True, if the <see cref="ProcessDictionary{TKey,TValue}"/> contains an element with the key; otherwise, false.</returns>
        /// <param name="key">The key to locate in the <see cref="ProcessDictionary{TKey,TValue}"/>.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.ContainsKey(key);
            }
        }

        /// <summary>Determines whether the <see cref="ProcessDictionary{TKey,TValue}"/> contains an element with the specified value.</summary>
        /// <returns>True, if the <see cref="ProcessDictionary{TKey,TValue}"/> contains an element with the value; otherwise, false.</returns>
        /// <param name="value">The value to locate in the <see cref="ProcessDictionary{TKey,TValue}"/>.</param>
        public bool ContainsValue(TValue value)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.ContainsValue(value);
            }
        }

        /// <summary>
        /// Searches for the specified key and returns the zero-based index within the entire <see cref="ProcessDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="ProcessDictionary{TKey,TValue}"/>.</param>
        /// <returns>The zero-based index of key within the entire <see cref="ProcessDictionary{TKey,TValue}"/>, if found; otherwise, -1.</returns>
        public int IndexOfKey(TKey key)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.IndexOfKey(key);
            }
        }

        /// <summary>
        /// Searches for the specified value and returns the zero-based index of the first occurrence within the entire <see cref="ProcessDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="ProcessDictionary{TKey,TValue}"/>. The value can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire <see cref="ProcessDictionary{TKey,TValue}"/>, if found; otherwise, -1.</returns>
        public int IndexOfValue(TValue value)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.IndexOfValue(value);
            }
        }

        /// <summary>Removes the element with the specified key from the <see cref="ProcessDictionary{TKey,TValue}"/>.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        /// <returns>This method returns a <see cref="Boolean"/> value indicating whether the item was removed.</returns>
        public bool Remove(TKey key)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.Remove(key);
            }
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <returns>True, if the <see cref="ProcessDictionary{TKey,TValue}"/> contains an element with the specified key; otherwise, false.</returns>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the
        /// key is found; otherwise, the default value for the type of the value parameter. This parameter is passed
        /// uninitialized.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.TryGetValue(key, out value);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ProcessDictionary{TKey, TValue}"/> if the key does not already exist.
        /// </summary>
        /// <param name="key">The key to be added to the dictionary if it does not already exist.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <returns>The value of the key in the dictionary.</returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.GetOrAdd(key, valueFactory);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ProcessDictionary{TKey, TValue}"/> if the key does not already exist.
        /// </summary>
        /// <param name="key">The key to be added to the dictionary if it does not already exist.</param>
        /// <param name="value">The value to assign to the key if the key does not already exist.</param>
        /// <returns>The value of the key in the dictionary.</returns>
        public TValue GetOrAdd(TKey key, TValue value)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.GetOrAdd(key, value);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ProcessDictionary{TKey, TValue}"/> if the key does not already exist,
        /// or updates a key/value pair in the <see cref="ProcessDictionary{TKey, TValue}"/> if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.AddOrUpdate(key, addValueFactory, updateValueFactory);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ProcessDictionary{TKey, TValue}"/> if the key does not already exist,
        /// or updates a key/value pair in the <see cref="ProcessDictionary{TKey, TValue}"/> if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.AddOrUpdate(key, addValue, updateValueFactory);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ProcessDictionary{TKey, TValue}"/> if the key does not already exist,
        /// or updates a key/value pair in the <see cref="ProcessDictionary{TKey, TValue}"/> if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or updated.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <returns>The value of the key in the dictionary after updating.</returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> valueFactory)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.AddOrUpdate(key, valueFactory);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ProcessDictionary{TKey, TValue}"/> if the key does not already exist,
        /// or updates a key/value pair in the <see cref="ProcessDictionary{TKey, TValue}"/> if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or updated.</param>
        /// <param name="value">The value to be assigned to the key.</param>
        /// <returns>The value of the key in the dictionary after updating.</returns>
        public TValue AddOrUpdate(TKey key, TValue value)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.AddOrUpdate(key, value);
            }
        }

        #endregion

        #region [ Overriden List(T) Functions ]

        // Because consumers will be able to call these functions in their "dictionary" style queue, we'll make
        // sure they return something that makes sense in case they get called, but we will hide the functions from
        // the editor to help avoid confusion.

        ///	<summary>
        /// This function doesn't have the same meaning in the <see cref="ProcessDictionary{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="ProcessDictionary{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        ///	<returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int BinarySearch(KeyValuePair<TKey, TValue> item)
        {
            return IndexOfKey(item.Key);
        }

        ///	<summary>
        /// This function doesn't have the same meaning in the <see cref="ProcessDictionary{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="ProcessDictionary{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or-
        /// null to use the default comparer: Generic.Comparer(Of T).Default</param>
        ///	<returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int BinarySearch(KeyValuePair<TKey, TValue> item, IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            return IndexOfKey(item.Key);
        }

        ///	<summary>
        /// This function doesn't have the same meaning in the <see cref="ProcessDictionary{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="ProcessDictionary{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or- null to use
        /// the default comparer: Generic.Comparer(Of T).Default</param>
        ///	<returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int BinarySearch(int index, int count, KeyValuePair<TKey, TValue> item, IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// This function doesn't have the same meaning in the <see cref="ProcessDictionary{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="ProcessDictionary{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        ///	<returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int IndexOf(KeyValuePair<TKey, TValue> item)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// This function doesn't have the same meaning in the <see cref="ProcessDictionary{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="ProcessDictionary{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search.</param>
        ///	<returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int IndexOf(KeyValuePair<TKey, TValue> item, int index, int count)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// This function doesn't have the same meaning in the <see cref="ProcessDictionary{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="ProcessDictionary{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int LastIndexOf(KeyValuePair<TKey, TValue> item)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// This function doesn't have the same meaning in the <see cref="ProcessDictionary{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="ProcessDictionary{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int LastIndexOf(KeyValuePair<TKey, TValue> item, int index)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// This function doesn't have the same meaning in the <see cref="ProcessDictionary{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="ProcessDictionary{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int LastIndexOf(KeyValuePair<TKey, TValue> item, int index, int count)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// <see cref="ProcessDictionary{TKey,TValue}"/> is based on a <see cref="DictionaryList{TKey,TValue}"/> which is already
        /// sorted, so calling this function has no effect.  As a result this function is marked as hidden from the editor.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Sort()
        {
            // This list is already sorted.
        }

        /// <summary>
        /// <see cref="ProcessDictionary{TKey,TValue}"/> is based on a <see cref="DictionaryList{TKey,TValue}"/> which is already
        /// sorted, so calling this function has no effect.  As a result this function is marked as hidden from the editor.
        /// </summary>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or-
        /// null to use the default comparer: Generic.Comparer(Of T).Default</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Sort(IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            // This list is already sorted.
        }

        /// <summary>
        /// <see cref="ProcessDictionary{TKey,TValue}"/> is based on a <see cref="DictionaryList{TKey,TValue}"/> which is already
        /// sorted, so calling this function has no effect.  As a result this function is marked as hidden from the editor.
        /// </summary>
        /// <param name="comparison">The comparison to use when comparing elements.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Sort(Comparison<KeyValuePair<TKey, TValue>> comparison)
        {
            // This list is already sorted.
        }

        /// <summary>
        /// <see cref="ProcessDictionary{TKey,TValue}"/> is based on a <see cref="DictionaryList{TKey,TValue}"/> which is already
        /// sorted, so calling this function has no effect.  As a result this function is marked as hidden from the editor.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or-
        /// null to use the default comparer: Generic.Comparer(Of T).Default</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Sort(int index, int count, IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            // This list is already sorted.
        }

        #endregion

        #endregion
    }
}