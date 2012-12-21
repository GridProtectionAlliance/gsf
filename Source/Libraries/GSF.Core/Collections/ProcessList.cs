//******************************************************************************************************
//  ProcessList.cs - Gbtc
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

namespace GSF.Collections
{
    #region [ Enumerations ]

    /// <summary>
    /// Enumeration of possible requeue modes.
    /// </summary>
    public enum RequeueMode
    {
        /// <summary>
        /// Requeues item at the beginning of the <see cref="ProcessList{T}"/>.
        /// </summary>
        Prefix,
        /// <summary>
        /// Requeues item at the end of the <see cref="ProcessList{T}"/>.
        /// </summary>
        Suffix
    }

    #endregion

    /// <summary>
    /// Represents a thread-safe (via locking) list of items, based on <see cref="List{T}"/>, that get processed on independent threads with a consumer provided function.
    /// </summary>
    /// <typeparam name="T">Type of object to process</typeparam>
    /// <remarks>
    /// <para>This class acts as a strongly-typed collection of objects to be processed.</para>
    /// <para>Consumers are expected to create new instances of this class through the static construction functions (e.g., CreateAsynchronousQueue, CreateSynchronousQueue, etc.)</para>
    /// <para>Note that the <see cref="ProcessList{T}"/> will not start processing until the Start method is called.</para>
    /// </remarks>
    public class ProcessList<T> : ProcessQueue<T>, IList<T>
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default setting for requeuing mode on processing timeout.
        /// </summary>
        public const RequeueMode DefaultRequeueModeOnTimeout = RequeueMode.Prefix;

        /// <summary>
        /// Default setting for requeuing mode on processing exceptions.
        /// </summary>
        public const RequeueMode DefaultRequeueModeOnException = RequeueMode.Prefix;

        // Fields
        private RequeueMode m_requeueModeOnTimeout;
        private RequeueMode m_requeueModeOnException;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a <see cref="ProcessList{T}"/> based on the generic <see cref="List{T}"/> class.
        /// </summary>
        /// <param name="processItemFunction">A delegate <see cref="ProcessQueue{T}.ProcessItemFunctionSignature"/> that defines a function signature to process a key and value one at a time.</param>
        /// <param name="canProcessItemFunction">A delegate <see cref="ProcessQueue{T}.CanProcessItemFunctionSignature"/> that determines of a key and value can currently be processed.</param>
        /// <param name="processInterval">A <see cref="double"/> which represents the process interval.</param>
        /// <param name="maximumThreads">An <see cref="int"/> that represents the max number of threads to use.</param>
        /// <param name="processTimeout">An <see cref="int"/> that represents the amount of time before a process times out.</param>
        /// <param name="requeueOnTimeout">A <see cref="bool"/> value that indicates whether the process should requeue the item after a timeout.</param>
        /// <param name="requeueOnException">A <see cref="bool"/> value that indicates whether the process should requeue the item after an exception.</param>
        protected ProcessList(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
            : base(processItemFunction, null, canProcessItemFunction, new List<T>(), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
            m_requeueModeOnTimeout = DefaultRequeueModeOnTimeout;
            m_requeueModeOnException = DefaultRequeueModeOnException;
        }

        /// <summary>
        /// Creates a bulk-item <see cref="ProcessList{T}"/> based on the generic <see cref="List{T}"/> class.
        /// </summary>
        /// <param name="processItemsFunction">A delegate ProcessItemsFunctionSignature that defines a function signature to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">A delegate <see cref="ProcessQueue{T}.CanProcessItemFunctionSignature"/> that determines of a key and value can currently be processed.</param>
        /// <param name="processInterval">A <see cref="double"/> which represents the process interval.</param>
        /// <param name="maximumThreads">An <see cref="int"/> that represents the max number of threads to use.</param>
        /// <param name="processTimeout">An <see cref="int"/> that represents the amount of time before a process times out.</param>
        /// <param name="requeueOnTimeout">A <see cref="bool"/> value that indicates whether the process should requeue the item after a timeout.</param>
        /// <param name="requeueOnException">A <see cref="bool"/> value that indicates whether the process should requeue the item after an exception.</param>
        protected ProcessList(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
            : base(null, processItemsFunction, canProcessItemFunction, new List<T>(), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
            m_requeueModeOnTimeout = DefaultRequeueModeOnTimeout;
            m_requeueModeOnException = DefaultRequeueModeOnException;
        }

        /// <summary>
        /// Allows derived classes to define their own instance, if desired.
        /// </summary>
        /// <param name="processItemFunction">Delegate that defines a method to process one item at a time.</param>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate that determines if an item can currently be processed.</param>
        /// <param name="processQueue">A storage list for items to be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        protected ProcessList(ProcessItemFunctionSignature processItemFunction, ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, IEnumerable<T> processQueue, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
            : base(processItemFunction, processItemsFunction, canProcessItemFunction, processQueue, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
            m_requeueModeOnTimeout = DefaultRequeueModeOnTimeout;
            m_requeueModeOnException = DefaultRequeueModeOnException;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the mode of insertion used (prefix or suffix) when at item is placed back into the <see cref="ProcessList{T}"/>
        /// after processing times out.
        /// </summary>
        /// <remarks>Only relevant when RequeueOnTimeout = True.</remarks>
        public virtual RequeueMode RequeueModeOnTimeout
        {
            get
            {
                return m_requeueModeOnTimeout;
            }
            set
            {
                m_requeueModeOnTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the mode of insertion used (prefix or suffix) when at item is placed back into the
        /// <see cref="ProcessList{T}"/> after an exception occurs while processing.
        /// </summary>
        /// <remarks>Only relevant when RequeueOnException = True.</remarks>
        public virtual RequeueMode RequeueModeOnException
        {
            get
            {
                return m_requeueModeOnException;
            }
            set
            {
                m_requeueModeOnException = value;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>The element at the specified index.</returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than <see cref="ProcessList{T}"/> length.</exception>
        public virtual T this[int index]
        {
            get
            {
                lock (SyncRoot)
                {
                    return InternalList[index];
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    InternalList[index] = value;
                    DataAdded();
                }
            }
        }

        /// <summary>
        /// Gets the number of elements actually contained in the <see cref="ProcessList{T}"/>.
        /// </summary>
        /// <returns>The number of elements actually contained in the <see cref="ProcessList{T}"/>.</returns>
        public override int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return InternalList.Count;
                }
            }
        }

        /// <summary>Gets a value indicating whether the <see cref="ProcessList{T}"/> is read-only.</summary>
        /// <returns>True, if the <see cref="ProcessList{T}"/> is read-only; otherwise, false. In the default implementation, this property
        /// always returns false.</returns>
        public virtual bool IsReadOnly
        {
            get
            {
                return InternalList.IsReadOnly;
            }
        }

        /// <summary>
        /// Gets the internal list for direct use by <see cref="ProcessList{T}"/>.
        /// </summary>
        protected IList<T> InternalList
        {
            get
            {
                return InternalEnumerable as IList<T>;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether an element is in the <see cref="ProcessList{T}"/>.
        /// </summary>
        /// <returns>True, if item is found in the <see cref="ProcessList{T}"/>; otherwise, false.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessList{T}"/>. The value can be null for reference types.</param>
        public override bool Contains(T item)
        {
            lock (SyncRoot)
            {
                return InternalList.Contains(item);
            }
        }

        /// <summary>
        /// Copies the elements contained in the <see cref="ProcessList{T}"/> to a new array. 
        /// </summary>
        /// <returns>A new array containing the elements copied from the <see cref="ProcessList{T}"/>.</returns>
        public override T[] ToArray()
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                // We call base implementation this feature if process queue is not a List(Of T).
                if ((object)processList == null)
                    return base.ToArray();

                // Otherwise, we will call native implementation.
                return processList.ToArray();
            }
        }

        /// <summary>
        /// Attempts to remove and return an object from the <see cref="ProcessList{T}"/>.
        /// </summary>
        /// <param name="item">When this method returns, if the object was removed and returned successfully, item contains the removed object. If no object was available to be removed, the value is unspecified.</param>
        /// <returns><c>true</c> if an object was removed and returned successfully; otherwise, <c>false</c>.</returns>
        public override bool TryTake(out T item)
        {
            lock (SyncRoot)
            {
                IList<T> list = InternalList;

                if (list.Count > 0)
                {
                    item = list[0];
                    list.RemoveAt(0);
                    return true;
                }
            }

            item = default(T);
            return false;
        }

        /// <summary>
        /// Attempts to remove and return all objects from the <see cref="ProcessList{T}"/>.
        /// </summary>
        /// <param name="items">When this method returns, if any objects were removed and returned successfully, item array contains the removed objects. If no object was available to be removed, the value is null.</param>
        /// <returns><c>true</c> if any objects were removed and returned successfully; otherwise, <c>false</c>.</returns>
        public override bool TryTake(out T[] items)
        {
            lock (SyncRoot)
            {
                if (Count > 0)
                {
                    items = ToArray();
                    Clear();
                    return true;
                }
            }

            items = null;
            return false;
        }

        /// <summary>
        /// Requeues item into <see cref="ProcessList{T}"/> according to specified requeue reason.
        /// </summary>
        /// <param name="item">A generic item of type T to be requeued.</param>
        /// <param name="reason">The reason the object is being requeued.</param>
        protected override void RequeueItem(T item, RequeueReason reason)
        {
            RequeueMode mode;

            switch (reason)
            {
                case RequeueReason.Exception:
                    mode = RequeueModeOnException;
                    break;
                case RequeueReason.Timeout:
                    mode = RequeueModeOnTimeout;
                    break;
                default:
                    mode = RequeueMode.Prefix;
                    break;
            }

            if (mode == RequeueMode.Prefix)
            {
                Insert(0, item);
            }
            else
            {
                Add(item);
            }
        }

        /// <summary>
        /// Requeues items into <see cref="ProcessList{T}"/> according to specified requeue reason.
        /// </summary>
        /// <param name="items">Array of type T to be requeued.</param>
        /// <param name="reason">The reason the object is being requeued.</param>
        protected override void RequeueItems(T[] items, RequeueReason reason)
        {
            RequeueMode mode;

            switch (reason)
            {
                case RequeueReason.Exception:
                    mode = RequeueModeOnException;
                    break;
                case RequeueReason.Timeout:
                    mode = RequeueModeOnTimeout;
                    break;
                default:
                    mode = RequeueMode.Prefix;
                    break;
            }

            if (mode == RequeueMode.Prefix)
            {
                InsertRange(0, items);
            }
            else
            {
                AddRange(items);
            }
        }

        #region [ Handy List(Of T) Functions Implementation ]

        // The internal list is declared as an IList(Of T). Derived classes (e.g., ProcessDictionary) can use their own
        // list implementation for process functionality. However, the regular List(Of T) provides many handy functions
        // that are not required to be exposed by the IList(Of T) interface. So, if the implemented list is a List(Of T),
        // we'll expose this native functionality; otherwise, we implement it for you. Yeah, you'll thank me one day.

        // Note: All List(Of T) implementations should be synchronized, as necessary.

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ProcessList{T}"/>.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements should be added to the end of the <see cref="ProcessList{T}"/>.
        /// The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.
        /// </param>
        /// <exception cref="ArgumentNullException">collection is null.</exception>
        public override void AddRange(IEnumerable<T> collection)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature if process queue is not a List(Of T).
                    if ((object)collection == null)
                        throw new ArgumentNullException("collection", "collection is null");

                    foreach (T item in collection)
                    {
                        InternalList.Add(item);
                    }
                }
                else
                {
                    // Otherwise, we'll call native implementation.
                    processList.AddRange(collection);
                }

                DataAdded();
            }
        }

        ///	<summary>
        /// Searches the entire sorted <see cref="ProcessList{T}"/>, using a binary search algorithm, for an element using the
        /// default comparer and returns the zero-based index of the element.
        /// </summary>
        /// <remarks>
        /// <see cref="ProcessList{T}"/> must be sorted in order for this function to return an accurate result.
        /// </remarks>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        /// <returns>
        /// The zero-based index of item in the sorted <see cref="ProcessList{T}"/>, if item is found; otherwise, a negative number that is the
        /// bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        /// the bitwise complement of count.
        /// </returns>
        ///	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an
        /// implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        public virtual int BinarySearch(T item)
        {
            return BinarySearch(0, InternalList.Count, item, null);
        }

        ///	<summary>
        /// Searches the entire sorted <see cref="ProcessList{T}"/>, using a binary search algorithm, for an element using the
        /// specified comparer and returns the zero-based index of the element.
        /// </summary>
        /// <remarks>
        /// <see cref="ProcessList{T}"/> must be sorted in order for this function to return an accurate result.
        /// </remarks>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or-
        /// null to use the default comparer: Generic.Comparer(Of T).Default</param>
        /// <returns>
        /// The zero-based index of item in the sorted <see cref="ProcessList{T}"/>, if item is found; otherwise, a negative number that is the
        /// bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        /// the bitwise complement of count.
        /// </returns>
        ///	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an
        /// implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        public virtual int BinarySearch(T item, IComparer<T> comparer)
        {
            return BinarySearch(0, InternalList.Count, item, comparer);
        }

        ///	<summary>
        /// Searches a range of elements in the sorted <see cref="ProcessList{T}"/>, using a binary search algorithm, for an
        /// element using the specified comparer and returns the zero-based index of the element.
        /// </summary>
        /// <remarks>
        /// <see cref="ProcessList{T}"/> must be sorted in order for this function to return an accurate result.
        /// </remarks>
        /// <param name="index">The zero-based starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or- null to use
        /// the default comparer: Generic.Comparer(Of T).Default</param>
        /// <returns>
        /// The zero-based index of item in the sorted <see cref="ProcessList{T}"/>, if item is found; otherwise, a negative number that is the
        /// bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        /// the bitwise complement of count.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="ProcessList{T}"/>
        /// -or- count is less than 0 -or- startIndex and count do not specify a valid section in the <see cref="ProcessList{T}"/></exception>
        ///	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an
        /// implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        public virtual int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    int foundIndex = -1;
                    int startIndex = index;
                    int stopIndex = index + count - 1;
                    int currentIndex;
                    int result;

                    // Validates start and stop index.
                    if (startIndex < 0 || count < 0 || stopIndex > InternalList.Count - 1)
                        throw new ArgumentOutOfRangeException("index", "index and/or count is outside the range of valid indexes for the queue");

                    if ((object)comparer == null)
                        comparer = Comparer<T>.Default;

                    if (count > 0)
                    {
                        while (true)
                        {
                            // Finds next mid point.
                            currentIndex = startIndex + (stopIndex - startIndex) / 2;

                            // Compares item at mid-point
                            result = comparer.Compare(item, InternalList[currentIndex]);

                            if (result == 0)
                            {
                                // For a found item, returns located index.
                                foundIndex = currentIndex;
                                break;
                            }
                            else if (startIndex == stopIndex)
                            {
                                // Met in the middle and didn't find match, so we are finished,
                                foundIndex = startIndex ^ -1;
                                break;
                            }
                            else if (result > 0)
                            {
                                if (currentIndex < count - 1)
                                {
                                    // Item is beyond current item, so we start search at next item.
                                    startIndex = currentIndex + 1;
                                }
                                else
                                {
                                    // Looked to the end and did not find match, so we are finished.
                                    foundIndex = (count - 1) ^ -1;
                                    break;
                                }
                            }
                            else
                            {
                                if (currentIndex > 0)
                                {
                                    // Item is before current item, so we will stop search at current item.
                                    // Note that because of the way the math works, you do not stop at the
                                    // prior item, as you might guess. It can cause you to skip an item.
                                    stopIndex = currentIndex;
                                }
                                else
                                {
                                    // Looked to the top and did not find match, so we are finished.
                                    foundIndex = 0 ^ -1;
                                    break;
                                }
                            }
                        }
                    }

                    return foundIndex;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processList.BinarySearch(index, count, item, comparer);
                }
            }
        }

        /// <summary>Converts the elements in the current <see cref="ProcessList{T}"/> to another type, and returns a <see cref="ProcessList{T}"/> containing the
        /// converted elements.</summary>
        /// <returns>A generic list of the target type containing the converted elements from the current <see cref="ProcessList{T}"/>.</returns>
        /// <param name="converter">A Converter delegate that converts each element from one type to another type.</param>
        /// <exception cref="ArgumentNullException">converter is null.</exception>
        /// <typeparam name="TOutput">The generic type used.</typeparam>
        public virtual List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if ((object)converter == null)
                        throw new ArgumentNullException("converter", "converter is null");

                    List<TOutput> result = new List<TOutput>();

                    foreach (T item in InternalList)
                    {
                        result.Add(converter(item));
                    }

                    return result;
                }
                else
                {
                    // Otherwise, we will call native implementation
                    return processList.ConvertAll(converter);
                }
            }
        }

        /// <summary>Determines whether the <see cref="ProcessList{T}"/> contains elements that match the conditions defined by the specified
        /// predicate.</summary>
        /// <returns>True, if the <see cref="ProcessList{T}"/> contains one or more elements that match the conditions defined by the specified
        /// predicate; otherwise, false.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the elements to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual bool Exists(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if ((object)match == null)
                        throw new ArgumentNullException("match", "match is null");

                    bool found = false;

                    for (int x = 0; x < InternalList.Count; x++)
                    {
                        if (match(InternalList[x]))
                        {
                            found = true;
                            break;
                        }
                    }

                    return found;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processList.Exists(match);
                }
            }
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the first occurrence within the entire <see cref="ProcessList{T}"/>.</summary>
        /// <returns>The first element that matches the conditions defined by the specified predicate, if found;
        /// otherwise, the default value for type T.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual T Find(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if ((object)match == null)
                        throw new ArgumentNullException("match", "match is null");

                    T foundItem = default(T);
                    int foundIndex = FindIndex(match);

                    if (foundIndex >= 0)
                        foundItem = InternalList[foundIndex];

                    return foundItem;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processList.Find(match);
                }
            }
        }

        /// <summary>Retrieves all elements that match the conditions defined by the specified predicate.</summary>
        /// <returns>A generic list containing all elements that match the conditions defined by the specified predicate,
        /// if found; otherwise, an empty list.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the elements to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual List<T> FindAll(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if ((object)match == null)
                        throw new ArgumentNullException("match", "match is null");

                    List<T> foundItems = new List<T>();

                    foreach (T item in InternalList)
                    {
                        if (match(item))
                            foundItems.Add(item);
                    }

                    return foundItems;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processList.FindAll(match);
                }
            }
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the first occurrence within the range of elements in the <see cref="ProcessList{T}"/> that extends from the
        /// specified index to the last element.</summary>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindIndex(Predicate<T> match)
        {
            return FindIndex(0, InternalList.Count, match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the first occurrence within the range of elements in the <see cref="ProcessList{T}"/> that extends from the
        /// specified index to the last element.</summary>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="ProcessList{T}"/>.</exception>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindIndex(int startIndex, Predicate<T> match)
        {
            return FindIndex(startIndex, InternalList.Count, match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the first occurrence within the range of elements in the <see cref="ProcessList{T}"/> that extends from the
        /// specified index to the last element.</summary>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="ProcessList{T}"/>
        /// -or- count is less than 0 -or- startIndex and count do not specify a valid section in the <see cref="ProcessList{T}"/>.</exception>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (startIndex < 0 || count < 0 || startIndex + count > InternalList.Count)
                        throw new ArgumentOutOfRangeException("startIndex", "startIndex and/or count is outside the range of valid indexes for the queue");

                    if ((object)match == null)
                        throw new ArgumentNullException("match", "match is null");

                    int foundindex = -1;

                    for (int x = startIndex; x < startIndex + count; x++)
                    {
                        if (match(InternalList[x]))
                        {
                            foundindex = x;
                            break;
                        }
                    }

                    return foundindex;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processList.FindIndex(startIndex, count, match);
                }
            }
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="ProcessList{T}"/>.</summary>
        /// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual T FindLast(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature if process queue is not a List(Of T)
                    if ((object)match == null)
                        throw (new ArgumentNullException("match", "match is null"));

                    T foundItem = default(T);
                    int foundIndex = FindLastIndex(match);

                    if (foundIndex >= 0)
                        foundItem = InternalList[foundIndex];

                    return foundItem;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processList.FindLast(match);
                }
            }
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the last occurrence within the entire <see cref="ProcessList{T}"/>.</summary>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindLastIndex(Predicate<T> match)
        {
            return FindLastIndex(0, InternalList.Count, match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the last occurrence within the range of elements in the <see cref="ProcessList{T}"/> that extends from the
        /// first element to the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="ProcessList{T}"/>.</exception>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return FindLastIndex(startIndex, InternalList.Count, match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the last occurrence within the range of elements in the <see cref="ProcessList{T}"/> that contains the
        /// specified number of elements and ends at the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="ProcessList{T}"/>
        /// -or- count is less than 0 -or- startIndex and count do not specify a valid section in the <see cref="ProcessList{T}"/>.</exception>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (startIndex < 0 || count < 0 || startIndex + count > InternalList.Count)
                        throw new ArgumentOutOfRangeException("startIndex", "startIndex and/or count is outside the range of valid indexes for the queue");

                    if ((object)match == null)
                        throw new ArgumentNullException("match", "match is null");

                    int foundindex = -1;

                    for (int x = startIndex + count - 1; x >= startIndex; x--)
                    {
                        if (match(InternalList[x]))
                        {
                            foundindex = x;
                            break;
                        }
                    }

                    return foundindex;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processList.FindLastIndex(startIndex, count, match);
                }
            }
        }

        /// <summary>Performs the specified action on each element of the <see cref="ProcessList{T}"/>.</summary>
        /// <param name="action">The Action delegate to perform on each element of the <see cref="ProcessList{T}"/>.</param>
        /// <exception cref="ArgumentNullException">action is null.</exception>
        public virtual void ForEach(Action<T> action)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if ((object)action == null)
                        throw new ArgumentNullException("action", "action is null");

                    foreach (T item in InternalList)
                    {
                        action(item);
                    }
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    processList.ForEach(action);
                }
            }
        }

        /// <summary>Creates a shallow copy of a range of elements in the source <see cref="ProcessList{T}"/>.</summary>
        /// <returns>A shallow copy of a range of elements in the source <see cref="ProcessList{T}"/>.</returns>
        /// <param name="count">The number of elements in the range.</param>
        /// <param name="index">The zero-based <see cref="ProcessList{T}"/> index at which the range starts.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        /// <exception cref="ArgumentException">index and count do not denote a valid range of elements in the <see cref="ProcessList{T}"/>.</exception>
        public virtual List<T> GetRange(int index, int count)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index + count > InternalList.Count)
                        throw new ArgumentException("Index and count do not denote a valid range of elements in the queue");

                    if (index < 0 || count < 0)
                        throw new ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue");

                    List<T> items = new List<T>();

                    for (int x = index; x < index + count; x++)
                    {
                        items.Add(InternalList[x]);
                    }

                    return items;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processList.GetRange(index, count);
                }
            }
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within
        /// the range of elements in the <see cref="ProcessList{T}"/> that extends from the specified index to the last element.</summary>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="ProcessList{T}"/> that
        /// extends from index to the last element, if found; otherwise, –1.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessList{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="ProcessList{T}"/>.</exception>
        public virtual int IndexOf(T item, int index)
        {
            return IndexOf(item, index, InternalList.Count);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within
        /// the range of elements in the <see cref="ProcessList{T}"/> that starts at the specified index and contains the specified number of
        /// elements.</summary>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="ProcessList{T}"/> that
        /// starts at index and contains count number of elements, if found; otherwise, –1.</returns>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="item">The object to locate in the <see cref="ProcessList{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="ProcessList{T}"/>
        /// -or- count is less than 0 -or- index and count do not specify a valid section in the <see cref="ProcessList{T}"/>.</exception>
        public virtual int IndexOf(T item, int index, int count)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index < 0 || count < 0 || index + count > InternalList.Count)
                        throw new ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue");

                    int foundindex = -1;
                    Comparer<T> comparer = Comparer<T>.Default;

                    for (int x = index; x < index + count; x++)
                    {
                        if (comparer.Compare(item, InternalList[x]) == 0)
                        {
                            foundindex = x;
                            break;
                        }
                    }

                    return foundindex;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processList.IndexOf(item, index, count);
                }
            }
        }

        /// <summary>Inserts the elements of a collection into the <see cref="ProcessList{T}"/> at the specified index.</summary>
        /// <param name="collection">The collection whose elements should be inserted into the <see cref="ProcessList{T}"/>. The collection
        /// itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is greater than <see cref="ProcessList{T}"/> length.</exception>
        /// <exception cref="ArgumentNullException">collection is null.</exception>
        public virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index < 0 || index > InternalList.Count - 1)
                        throw new ArgumentOutOfRangeException("index", "index is outside the range of valid indexes for the queue");

                    if ((object)collection == null)
                        throw new ArgumentNullException("collection", "collection is null");

                    foreach (T item in collection)
                    {
                        InternalList.Insert(index, item);
                        index++;
                    }
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    processList.InsertRange(index, collection);
                }

                DataAdded();
            }
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
        /// entire <see cref="ProcessList{T}"/>.</summary>
        /// <returns>The zero-based index of the last occurrence of item within the entire the <see cref="ProcessList{T}"/>, if found;
        /// otherwise, –1.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessList{T}"/>. The value can be null for reference types.</param>
        public virtual int LastIndexOf(T item)
        {
            return LastIndexOf(item, 0, InternalList.Count);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
        /// range of elements in the <see cref="ProcessList{T}"/> that extends from the first element to the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of item within the range of elements in the <see cref="ProcessList{T}"/> that
        /// extends from the first element to index, if found; otherwise, –1.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessList{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="ProcessList{T}"/>. </exception>
        public virtual int LastIndexOf(T item, int index)
        {
            return LastIndexOf(item, index, InternalList.Count);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
        /// range of elements in the <see cref="ProcessList{T}"/> that contains the specified number of elements and ends at the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of item within the range of elements in the <see cref="ProcessList{T}"/> that
        /// contains count number of elements and ends at index, if found; otherwise, –1.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessList{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="ProcessList{T}"/> -or-
        /// count is less than 0 -or- index and count do not specify a valid section in the <see cref="ProcessList{T}"/>.</exception>
        public virtual int LastIndexOf(T item, int index, int count)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index < 0 || count < 0 || index + count > InternalList.Count)
                        throw new ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue");

                    int foundindex = -1;
                    Comparer<T> comparer = Comparer<T>.Default;

                    for (int x = index + count - 1; x >= index; x--)
                    {
                        if (comparer.Compare(item, InternalList[x]) == 0)
                        {
                            foundindex = x;
                            break;
                        }
                    }

                    return foundindex;
                }
                else
                {
                    // Otherwise, we'll call native implementation.
                    return processList.LastIndexOf(item, index, count);
                }
            }
        }

        /// <summary>Removes the all the elements that match the conditions defined by the specified predicate.</summary>
        /// <returns>The number of elements removed from the <see cref="ProcessList{T}"/>.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the elements to remove.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int RemoveAll(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if ((object)match == null)
                        throw new ArgumentNullException("match", "match is null");

                    int removedItems = 0;

                    // Process removal from the bottom up to maintain proper index access
                    for (int x = InternalList.Count - 1; x >= 0; x--)
                    {
                        if (match(InternalList[x]))
                        {
                            InternalList.RemoveAt(x);
                            removedItems++;
                        }
                    }

                    return removedItems;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processList.RemoveAll(match);
                }
            }
        }

        /// <summary>Removes a range of elements from the <see cref="ProcessList{T}"/>.</summary>
        /// <param name="count">The number of elements to remove.</param>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        /// <exception cref="ArgumentException">index and count do not denote a valid range of elements in the <see cref="ProcessList{T}"/>.</exception>
        public virtual void RemoveRange(int index, int count)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index < 0 || count < 0 || index + count > InternalList.Count)
                        throw new ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue");

                    for (int x = index + count - 1; x >= index; x--)
                    {
                        InternalList.RemoveAt(x);
                    }
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    processList.RemoveRange(index, count);
                }
            }
        }

        /// <summary>Reverses the order of the elements in the entire <see cref="ProcessList{T}"/>.</summary>
        public virtual void Reverse()
        {
            Reverse(0, InternalList.Count);
        }

        /// <summary>Reverses the order of the elements in the specified range.</summary>
        /// <param name="count">The number of elements in the range to reverse.</param>
        /// <param name="index">The zero-based starting index of the range to reverse.</param>
        /// <exception cref="ArgumentException">index and count do not denote a valid range of elements in the <see cref="ProcessList{T}"/>. </exception>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        public virtual void Reverse(int index, int count)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index + count > InternalList.Count)
                        throw new ArgumentException("Index and count do not denote a valid range of elements in the queue");

                    if (index < 0 || count < 0)
                        throw new ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue");

                    T item;
                    int stopIndex = index + count - 1;

                    for (int x = index; x < (index + count) / 2; x++)
                    {
                        if (x < stopIndex)
                        {
                            // Swaps items top to bottom to reverse order.
                            item = InternalList[x];
                            InternalList[x] = InternalList[stopIndex];
                            InternalList[stopIndex] = item;
                            stopIndex--;
                        }
                    }
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    processList.Reverse(index, count);
                }
            }
        }

        /// <summary>Sorts the elements in the entire <see cref="ProcessList{T}"/>, using the default comparer.</summary>
        ///	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an
        /// implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        public virtual void Sort()
        {
            Sort(0, InternalList.Count, null);
        }

        /// <summary>Sorts the elements in the entire <see cref="ProcessList{T}"/>, using the specified comparer.</summary>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements, or null to use
        /// the default comparer: Generic.Comparer.Default.</param>
        /// <exception cref="ArgumentException">The implementation of comparer caused an error during the sort. For
        /// example, comparer might not return 0 when comparing an item with itself.</exception>
        ///	<exception cref="InvalidOperationException">the comparer is null and the default comparer,
        /// Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the
        /// IComparable interface for type T.</exception>
        public virtual void Sort(IComparer<T> comparer)
        {
            Sort(0, InternalList.Count, comparer);
        }

        /// <summary>Sorts the elements in a range of elements in the <see cref="ProcessList{T}"/>, using the specified comparer.</summary>
        /// <param name="count">The length of the range to sort.</param>
        /// <param name="index">The zero-based starting index of the range to sort.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements, or null to use
        /// the default comparer: Generic.Comparer.Default.</param>
        /// <exception cref="ArgumentException">The implementation of comparer caused an error during the sort. For
        /// example, comparer might not return 0 when comparing an item with itself.</exception>
        ///	<exception cref="InvalidOperationException">the comparer is null and the default comparer,
        /// Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the
        /// IComparable interface for type T.</exception>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        public virtual void Sort(int index, int count, IComparer<T> comparer)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if ((object)comparer == null)
                        comparer = Comparer<T>.Default;

                    // This sort implementation is a little harsh, but the normal process queue uses List(Of T) and the
                    // keyed process queue is based on a sorted list anyway (i.e., no sorting needed); so, this alternate
                    // sort implementation exists for any future derived process queue possibly based on a non List(Of T)
                    // queue and will at least ensure that the function will perform as expected.
                    T[] items = ToArray();
                    Array.Sort<T>(items, index, count, comparer);
                    InternalList.Clear();
                    AddRange(items);
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    processList.Sort(index, count, comparer);
                }
            }
        }

        /// <summary>Sorts the elements in the entire <see cref="ProcessList{T}"/>, using the specified comparison.</summary>
        /// <param name="comparison">The comparison to use when comparing elements.</param>
        /// <exception cref="ArgumentException">The implementation of comparison caused an error during the sort. For
        /// example, comparison might not return 0 when comparing an item with itself.</exception>
        /// <exception cref="ArgumentNullException">comparison is null.</exception>
        public virtual void Sort(Comparison<T> comparison)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if ((object)comparison == null)
                        throw new ArgumentNullException("comparison", "comparison is null");

                    // This sort implementation is a little harsh, but the normal process queue uses List(Of T) and the
                    // keyed process queue is based on a sorted list anyway (i.e., no sorting needed); so, this alternate
                    // sort implementation exists for any future derived process queue possibly based on a non-List(Of T)
                    // queue and will at least ensure that the function will perform as expected. Maybe some clever
                    // programmer will come behind me and add some "Linq-y" expression that will magically do this...
                    T[] items = ToArray();
                    Array.Sort<T>(items, comparison);
                    InternalList.Clear();
                    AddRange(items);
                }
                else
                {
                    // Otherwise we'll call native implementation
                    processList.Sort(comparison);
                }
            }
        }

        /// <summary>Determines whether every element in the <see cref="ProcessList{T}"/> matches the conditions defined by the specified
        /// predicate.</summary>
        /// <returns>True, if every element in the <see cref="ProcessList{T}"/> matches the conditions defined by the specified predicate;
        /// otherwise, false. If the <see cref="ProcessList{T}"/> has no elements, the return value is true.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions to check against the elements.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual bool TrueForAll(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                List<T> processList = InternalList as List<T>;

                if ((object)processList == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if ((object)match == null)
                        throw (new ArgumentNullException("match", "match is null"));

                    bool allTrue = true;

                    foreach (T item in InternalList)
                    {
                        if (!match(item))
                        {
                            allTrue = false;
                            break;
                        }
                    }

                    return allTrue;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processList.TrueForAll(match);
                }
            }
        }

        #endregion

        #region [ Generic IList(Of T) Implementation ]

        // Note: All IList(Of T) implementations should be synchronized, as necessary.

        /// <summary>Adds an item to the <see cref="ProcessList{T}"/>.</summary>
        /// <param name="item">The item to add to the <see cref="ProcessList{T}"/>.</param>
        public override void Add(T item)
        {
            lock (SyncRoot)
            {
                InternalList.Add(item);
                DataAdded();
            }
        }

        /// <summary>Inserts an element into the <see cref="ProcessList{T}"/> at the specified index.</summary>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is greater than <see cref="ProcessList{T}"/> length.</exception>
        public virtual void Insert(int index, T item)
        {
            lock (SyncRoot)
            {
                InternalList.Insert(index, item);
                DataAdded();
            }
        }

        /// <summary>Copies the entire <see cref="ProcessList{T}"/> to a compatible one-dimensional array, starting at the beginning of the
        /// target array.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from <see cref="ProcessList{T}"/>. The
        /// array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentException">arrayIndex is equal to or greater than the length of array -or- the
        /// number of elements in the source <see cref="ProcessList{T}"/> is greater than the available space from arrayIndex to the end of the
        /// destination array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        public override void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                InternalList.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within
        /// the entire <see cref="ProcessList{T}"/>.</summary>
        /// <returns>The zero-based index of the first occurrence of item within the entire <see cref="ProcessList{T}"/>, if found; otherwise, –1.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessList{T}"/>. The value can be null for reference types.</param>
        public virtual int IndexOf(T item)
        {
            lock (SyncRoot)
            {
                return InternalList.IndexOf(item);
            }
        }

        /// <summary>
        /// Removes all elements from the <see cref="ProcessList{T}"/>.
        /// </summary>
        public override void Clear()
        {
            lock (SyncRoot)
            {
                InternalList.Clear();
            }
        }

        /// <summary>Removes the first occurrence of a specific object from the <see cref="ProcessList{T}"/>.</summary>
        /// <returns>True, if item is successfully removed; otherwise, false. This method also returns false if item was
        /// not found in the <see cref="ProcessList{T}"/>.</returns>
        /// <param name="item">The object to remove from the <see cref="ProcessList{T}"/>. The value can be null for reference types.</param>
        public virtual bool Remove(T item)
        {
            lock (SyncRoot)
            {
                return InternalList.Remove(item);
            }
        }

        /// <summary>Removes the element at the specified index of the <see cref="ProcessList{T}"/>.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than
        /// <see cref="ProcessList{T}"/> length.</exception>
        public virtual void RemoveAt(int index)
        {
            lock (SyncRoot)
            {
                InternalList.RemoveAt(index);
            }
        }

        #endregion

        #endregion

        #region [ Static ]

        #region [ Single-Item Processing Constructors ]

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessList{T}"/> with the default settings: ProcessInterval = 100, MaximumThreads = 5,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction)
        {
            return CreateAsynchronousQueue(processItemFunction, null, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessList{T}"/> with the default settings: ProcessInterval = 100, MaximumThreads = 5,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessList{T}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemFunction, null, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessList{T}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessList{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateAsynchronousQueue(processItemFunction, null, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessList{T}"/> using  specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessList<T>(processItemFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous <see cref="ProcessList{T}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction)
        {
            return CreateSynchronousQueue(processItemFunction, null, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous <see cref="ProcessList{T}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateSynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous <see cref="ProcessList{T}"/> (i.e., single process thread) using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateSynchronousQueue(processItemFunction, null, processInterval, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous <see cref="ProcessList{T}"/> (i.e., single process thread) using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessList<T>(processItemFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new real-time <see cref="ProcessList{T}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction)
        {
            return CreateRealTimeQueue(processItemFunction, null, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new real-time <see cref="ProcessList{T}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateRealTimeQueue(processItemFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new real-time <see cref="ProcessList{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateRealTimeQueue(processItemFunction, null, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new real-time <see cref="ProcessList{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessList<T>(processItemFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }

        #endregion

        #region [ Multi-Item Processing Constructors ]

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessList{T}"/> with the default settings: ProcessInterval = 100,
        /// MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction)
        {
            return CreateAsynchronousQueue(processItemsFunction, null, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessList{T}"/> with the default settings: ProcessInterval = 100,
        /// MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessList{T}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemsFunction, null, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessList{T}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessList{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateAsynchronousQueue(processItemsFunction, null, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessList{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessList<T>(processItemsFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous, bulk item <see cref="ProcessList{T}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction)
        {
            return CreateSynchronousQueue(processItemsFunction, null, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous, bulk item <see cref="ProcessList{T}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateSynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous, bulk item <see cref="ProcessList{T}"/> (i.e., single process thread) using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateSynchronousQueue(processItemsFunction, null, processInterval, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous, bulk item <see cref="ProcessList{T}"/> (i.e., single process thread) using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessList<T>(processItemsFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new real-time, bulk item <see cref="ProcessList{T}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction)
        {
            return CreateRealTimeQueue(processItemsFunction, null, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new real-time, bulk item <see cref="ProcessList{T}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateRealTimeQueue(processItemsFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new real-time, bulk item <see cref="ProcessList{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateRealTimeQueue(processItemsFunction, null, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new real-time, bulk item <see cref="ProcessList{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessList object based on type T.</returns>
        public static new ProcessList<T> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessList<T>(processItemsFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }

        #endregion

        #endregion
    }
}
