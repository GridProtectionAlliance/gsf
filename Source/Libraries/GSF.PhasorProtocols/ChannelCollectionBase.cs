//******************************************************************************************************
//  ChannelCollectionBase.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  3/7/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using GSF.Collections;
using GSF.Parsing;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent collection of <see cref="IChannel"/> objects.<br/>
    /// This is the base class of all collection classes in the phasor protocols library;
    /// it is the root of the collection class hierarchy.
    /// </summary>
    /// <remarks>
    /// This channel collection implements <see cref="IChannel"/> (inherited via <see cref="IChannelCollection{T}"/>) for the benefit
    /// of providing a cumulative binary image of the entire collection.
    /// </remarks>
    /// <typeparam name="T">Specific <see cref="IChannel"/> type that the <see cref="ChannelCollectionBase{T}"/> contains.</typeparam>
    [Serializable]
    public abstract class ChannelCollectionBase<T> : ListCollection<T>, IChannelCollection<T>, INotifyCollectionChanged where T : IChannel
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Notifies listeners of dynamic changes, such as when items get added and removed or the whole list is refreshed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        // Fields
        private int m_lastValidIndex;                       // Last valid index in the collection (i.e., maximum value count - 1)
        private IChannelParsingState m_state;               // Current parsing state
        private Dictionary<string, string> m_attributes;    // Attributes dictionary
        private object m_tag;                               // User defined tag

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChannelCollectionBase{T}"/>.
        /// </summary>
        protected ChannelCollectionBase()
        {
            m_lastValidIndex = int.MaxValue;
        }

        /// <summary>
        /// Creates a new <see cref="ChannelCollectionBase{T}"/> using specified <paramref name="lastValidIndex"/>.
        /// </summary>
        /// <param name="lastValidIndex">Last valid index for the collection (i.e., maximum count - 1).</param>
        /// <remarks>
        /// <paramref name="lastValidIndex"/> is used instead of maximum count so that maximum type values may
        /// be specified as needed. For example, if the protocol specifies a collection with a signed 16-bit
        /// maximum length you can specify <see cref="short.MaxValue"/> (i.e., 32,767) as the last valid index
        /// for the collection since total number of items supported would be 32,768.
        /// </remarks>
        protected ChannelCollectionBase(int lastValidIndex)
        {
            m_lastValidIndex = lastValidIndex;
        }

        /// <summary>
        /// Creates a new <see cref="ChannelCollectionBase{T}"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ChannelCollectionBase(SerializationInfo info, StreamingContext context)
        {
            // Deserialize collection
            m_lastValidIndex = info.GetInt32("maximumCount") - 1;

            for (int x = 0; x < info.GetInt32("count"); x++)
            {
                Add((T)info.GetValue($"item{x}", typeof(T)));
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the maximum allowed number of items for this <see cref="ChannelCollectionBase{T}"/>.
        /// </summary>
        public virtual int MaximumCount
        {
            get => m_lastValidIndex + 1;
            set => m_lastValidIndex = value - 1;
        }

        /// <summary>
        /// Gets or sets the parsing state for this <see cref="IChannel"/> object.
        /// </summary>
        public virtual IChannelParsingState State
        {
            get => m_state;
            set => m_state = value;
        }

        /// <summary>
        /// Gets the length of the <see cref="ChannelCollectionBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// The length of the <see cref="ChannelCollectionBase{T}"/> binary image is the combined length of all the items in the collection.<br/>
        /// The default implementation assumes all <see cref="IChannel"/> items in the collection have the same length, hence the value returned
        /// is the <see cref="ISupportBinaryImage.BinaryLength"/> of the first item in the collection times the <see cref="List{T}.Count"/>.
        /// </remarks>
        public virtual int BinaryLength
        {
            get
            {
                int count = Count;

                if (count > 0)
                    return this[0].BinaryLength * count;

                return 0;
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="IChannel"/> object.
        /// </summary>
        public virtual Dictionary<string, string> Attributes
        {
            get
            {
                // Create a new attributes dictionary or clear the contents of any existing one
                if (m_attributes is null)
                    m_attributes = new Dictionary<string, string>();
                else
                    m_attributes.Clear();

                m_attributes.Add("Derived Type", GetType().Name);
                m_attributes.Add("Binary Length", BinaryLength.ToString());
                m_attributes.Add("Maximum Count", MaximumCount.ToString());
                m_attributes.Add("Current Count", Count.ToString());

                return m_attributes;
            }
        }

        /// <summary>
        /// Gets or sets a user definable reference to an object associated with this <see cref="IChannel"/> object.
        /// </summary>
        public virtual object Tag
        {
            get => m_tag;
            set => m_tag = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Generates binary image of the object and copies it into the given buffer, for <see cref="ISupportBinaryImage.BinaryLength"/> bytes.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <see cref="BinaryLength"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <see cref="BinaryLength"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public virtual int GenerateBinaryImage(byte[] buffer, int startIndex)
        {
            buffer.ValidateParameters(startIndex, BinaryLength);

            int index = startIndex;

            // Copy in each element's binary image
            for (int x = 0; x < Count; x++)
                this[x].CopyImage(buffer, ref index);

            return index - startIndex;
        }

        // Collections are not designed to parse binary images
        int ISupportBinaryImage.ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ChannelCollectionBase{T}"/>.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements should be added to the end of the <see cref="ChannelCollectionBase{T}"/>.
        /// The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public virtual void AddRange(IEnumerable<T> collection)
        {
            if (collection is null)
                throw new ArgumentNullException(nameof(collection), "collection is null");

            foreach (T item in collection)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Manually sends a <see cref="NotifyCollectionChangedAction.Reset"/> to the collection changed event.
        /// </summary>
        public virtual void RefreshBinding()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Inserts an element into the <see cref="ChannelCollectionBase{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="OverflowException">Maximum collection item limit reached.</exception>
        protected override void InsertItem(int index, T item)
        {
            // Interception of inserted items occurs with this override (via Collection<T>) allowing maximum length to be validated
            if (Count > m_lastValidIndex)
                throw new OverflowException($"Maximum {GetType().Name} item limit reached");

            base.InsertItem(index, item);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="ChannelCollectionBase{T}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            if (CollectionChanged is null)
            {
                base.RemoveItem(index);
            }
            else
            {
                T oldItem = Items[index];
                base.RemoveItem(index);

                // ReSharper disable once PossibleNullReferenceException
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
            }
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
        protected override void SetItem(int index, T item)
        {
            if (CollectionChanged == null)
            {
                base.SetItem(index, item);
            }
            else
            {
                T oldItem = Items[index];
                base.SetItem(index, item);

                // ReSharper disable once PossibleNullReferenceException
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
            }
        }

        /// <summary>
        /// Removes all elements from the <see cref="ChannelCollectionBase{T}"/>.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Raises the <see cref="NotifyCollectionChangedEventHandler"/> event.
        /// </summary>
        /// <param name="e">Changed event arguments.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize collection
            info.AddValue("maximumCount", m_lastValidIndex + 1);
            info.AddValue("count", Count);

            for (int x = 0; x < Count; x++)
            {
                info.AddValue($"item{x}", this[x], typeof(T));
            }
        }

        #endregion
    }
}