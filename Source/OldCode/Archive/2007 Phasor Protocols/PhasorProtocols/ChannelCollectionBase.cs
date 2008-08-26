//*******************************************************************************************************
//  ChannelCollectionBase.vb - Channel data collection base class
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  3/7/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using TVA;
using TVA.Parsing;

namespace PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent representation of a collection of any kind of data.</summary>
    /// <remarks>By having our collections implement IChannel (inherited via IChannelCollection), we have the benefit of providing a binary image of the entire collection</remarks>
    [CLSCompliant(false), Serializable()]
    public abstract class ChannelCollectionBase<T> : List<T>, IChannelCollection<T> where T : IChannel
    {
        private int m_maximumCount;
        private Dictionary<string, string> m_attributes;
        private object m_tag;

        protected ChannelCollectionBase()
        {
        }

        protected ChannelCollectionBase(SerializationInfo info, StreamingContext context)
        {

            // Deserialize collection
            m_maximumCount = info.GetInt32("maximumCount");

            for (int x = 0; x <= info.GetInt32("count") - 1; x++)
            {
                Add((T)info.GetValue("item" + x, typeof(T)));
            }

        }

        protected ChannelCollectionBase(int maximumCount)
        {

            m_maximumCount = maximumCount;

        }

        public virtual new void Add(T item)
        {

            // Note: Maximum count is much easier to specify by using <value>.MaxValue which runs from 0 to MaxValue (i.e., MaxValue + 1)
            // so we allow one extra item in the following check to keep from having to add 1 to all maximum count specifications
            if (Count > m_maximumCount)
            {
                throw (new OverflowException("Maximum " + DerivedType.Name + " item limit reached"));
            }
            base.Add(item);

        }

        public abstract Type DerivedType
        {
            get;
        }

        public virtual int MaximumCount
        {
            get
            {
                return m_maximumCount;
            }
            set
            {
                m_maximumCount = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ParseBinaryImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            throw (new NotImplementedException("Binary images are not intended to be parsed at a collection level"));

        }

        public virtual ushort BinaryLength
        {
            get
            {
                if (Count > 0)
                {
                    return (ushort)(this[0].BinaryLength * Count);
                }
                else
                {
                    return 0;
                }
            }
        }

        int IBinaryDataProvider.BinaryLength
        {
            get
            {
                return (int)BinaryLength;
            }
        }

        public virtual byte[] BinaryImage
        {
            get
            {
                byte[] buffer = new byte[BinaryLength];
                int index = 0;

                for (int x = 0; x <= Count - 1; x++)
                {
                    PhasorProtocols.Common.CopyImage(this[x], buffer, ref index);
                }

                return buffer;
            }
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            // Serialize collection
            info.AddValue("maximumCount", m_maximumCount);
            info.AddValue("count", Count);

            for (int x = 0; x < Count; x++)
            {
                info.AddValue("item" + x, this[x], typeof(T));
            }

        }

        public virtual Dictionary<string, string> Attributes
        {
            get
            {
                // Create a new attributes dictionary or clear the contents of any existing one
                if (m_attributes == null)
                {
                    m_attributes = new Dictionary<string, string>();
                }
                else
                {
                    m_attributes.Clear();
                }

                m_attributes.Add("Derived Type", DerivedType.Name);
                m_attributes.Add("Binary Length", BinaryLength.ToString());
                m_attributes.Add("Maximum Count", MaximumCount.ToString());
                m_attributes.Add("Current Count", Count.ToString());

                return m_attributes;
            }
        }

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
    }
}
