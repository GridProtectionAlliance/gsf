//*******************************************************************************************************
//  ChannelCollectionBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  3/7/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using PCS.Parsing;

namespace PCS.PhasorProtocols
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
    [Serializable()]
    public abstract class ChannelCollectionBase<T> : Collection<T>, IChannelCollection<T> where T : IChannel
    {
        #region [ Members ]

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
            m_lastValidIndex = Int32.MaxValue;
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

            for (int x = 0; x <= info.GetInt32("count") - 1; x++)
            {
                Add((T)info.GetValue("item" + x, typeof(T)));
            }
        }

        /// <summary>
        /// Creates a new <see cref="ChannelCollectionBase{T}"/> from specified parameters.
        /// </summary>
        /// <param name="lastValidIndex">Last valid index for the collection (i.e., maximum count - 1).</param>
        /// <remarks>
        /// <paramref name="lastValidIndex"/> is used instead of maximum count so that maximum type values may
        /// be specified as needed. For example, if the protocol specifies a collection with a signed 16-bit
        /// maximum length you can specify <see cref="Int16.MaxValue"/> (i.e., 32,767) as the last valid index
        /// for the collection since total number of items supported would be 32,768.
        /// </remarks>
        protected ChannelCollectionBase(int lastValidIndex)
        {
            m_lastValidIndex = lastValidIndex;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the maximum allowed number of items for this <see cref="ChannelCollectionBase{T}"/>.
        /// </summary>
        public virtual int MaximumCount
        {
            get
            {
                return m_lastValidIndex + 1;
            }
            set
            {
                m_lastValidIndex = (value - 1);
            }
        }

        /// <summary>
        /// Gets or sets the parsing state for this <see cref="IChannel"/> object.
        /// </summary>
        public virtual IChannelParsingState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        /// <remarks>
        /// The length of the <see cref="ChannelCollectionBase{T}"/> binary image is the combined length of all the items in the collection.<br/>
        /// The default implementation assumes all <see cref="IChannel"/> items in the collection have the same length, hence the value returned
        /// is the <see cref="IChannel.BinaryLength"/> of the first item in the collection times the <see cref="List{T}.Count"/>.
        /// </remarks>
        public virtual int BinaryLength
        {
            get
            {
                if (Count > 0)
                    return this[0].BinaryLength * Count;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets the binary image of this <see cref="ChannelCollectionBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// The binary image of the <see cref="ChannelCollectionBase{T}"/> is the combined images of all the items in the collection.
        /// </remarks>
        public virtual byte[] BinaryImage
        {
            get
            {
                // Create a buffer large enough to hold all images
                byte[] buffer = new byte[BinaryLength];
                int index = 0;

                // Copy in each element's binary inage
                for (int x = 0; x <= Count - 1; x++)
                {
                    this[x].CopyImage(buffer, ref index);
                }

                return buffer;
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
                if (m_attributes == null)
                    m_attributes = new Dictionary<string, string>();
                else
                    m_attributes.Clear();

                m_attributes.Add("Derived Type", this.GetType().Name);
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
            get
            {
                return m_tag;
            }
            set
            {
                m_tag = value;
            }
        }

        #endregion

        #region [ Methods ]

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
                info.AddValue("item" + x, this[x], typeof(T));
            }
        }

        /// <summary>
        /// Inserts an element into the <see cref="ChannelCollectionBase{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            // Interception of inserted items occurs with this override (via Collection<T>) allowing maximum length to be validated
            if (Count > m_lastValidIndex)
                throw new OverflowException("Maximum " + this.GetType().Name + " item limit reached");

            base.InsertItem(index, item);
        }

        // Collections are not designed to parse binary images
        int ISupportBinaryImage.Initialize(byte[] binaryImage, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}