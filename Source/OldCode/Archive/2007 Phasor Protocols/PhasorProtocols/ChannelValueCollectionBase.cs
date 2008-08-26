//*******************************************************************************************************
//  ChannelValueCollectionBase.vb - Channel data value collection base class
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
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;
using TVA;

namespace PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent representation of a collection of any kind of data value.</summary>
    [CLSCompliant(false), Serializable(), SuppressMessage("Microsoft.Usage", "CA2240")]
    public abstract class ChannelValueCollectionBase<TDefinition, TValue> : ChannelCollectionBase<TValue>
        where TDefinition : IChannelDefinition
        where TValue : IChannelValue<TDefinition>
    {

        private int m_fixedCount;
        private int m_floatCount;

        protected ChannelValueCollectionBase()
        {
        }

        protected ChannelValueCollectionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


        }

        protected ChannelValueCollectionBase(int maximumCount)
            : base(maximumCount)
        {


        }

        public virtual new void Add(TValue value)
        {

            // In typical usage, all channel values will be of the same data type - but we can't anticipate all
            // possible uses of collection, so we track totals of each data type so we can quickly ascertain if
            // all the items in the collection are of the same data type
            if (value.DataFormat == PhasorProtocols.DataFormat.FixedInteger)
            {
                m_fixedCount++;
            }
            else
            {
                m_floatCount++;
            }

            base.Add(value);

        }

        public override ushort BinaryLength
        {
            get
            {
                if (Count > 0)
                {
                    if (m_fixedCount == 0 || m_floatCount == 0)
                    {
                        // Data types in list are consistent, a simple calculation will derive total binary length
                        return (ushort)(this[0].BinaryLength * Count);
                    }
                    else
                    {
                        // List has items of different data types, will have to traverse list to calculate total binary length
                        ushort length = 0;

                        for (int x = 0; x <= Count - 1; x++)
                        {
                            length += this[x].BinaryLength;
                        }

                        return length;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        public virtual bool AllValuesAssigned
        {
            get
            {
                bool allAssigned = true;

                for (int x = 0; x <= Count - 1; x++)
                {
                    if (this[x].IsEmpty)
                    {
                        allAssigned = false;
                        break;
                    }
                }

                return allAssigned;
            }
        }

        public virtual new void Clear()
        {

            base.Clear();
            m_fixedCount = 0;
            m_floatCount = 0;

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Fixed Count", m_fixedCount.ToString());
                baseAttributes.Add("Float Count", m_floatCount.ToString());
                baseAttributes.Add("All Values Assigned", AllValuesAssigned.ToString());

                return baseAttributes;
            }
        }
    }
}
