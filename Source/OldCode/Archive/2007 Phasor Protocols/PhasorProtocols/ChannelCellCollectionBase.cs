using System.Diagnostics;
using System;
////using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
////using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;

//*******************************************************************************************************
//  ChannelCellCollectionBase.vb - Channel data cell collection base class
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

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent representation of a collection of any kind of data cell.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class ChannelCellCollectionBase<T> : ChannelCollectionBase<T>, IChannelCellCollection<T> where T : IChannelCell
    {



        private bool m_constantCellLength;

        protected ChannelCellCollectionBase()
        {
        }

        protected ChannelCellCollectionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize extra elements
            m_constantCellLength = info.GetBoolean("constantCellLength");

        }

        protected ChannelCellCollectionBase(int maximumCount, bool constantCellLength)
            : base(maximumCount)
        {


            m_constantCellLength = constantCellLength;

        }

        public override ushort BinaryLength
        {
            get
            {
                if (m_constantCellLength)
                {
                    // Cells will be constant length, so we can quickly calculate lengths
                    return base.BinaryLength;
                }
                else
                {
                    // Cells will be different lengths, so we must manually sum lengths
                    ushort length = 0;

                    for (int x = 0; x <= Count - 1; x++)
                    {
                        length += this[x].BinaryLength;
                    }

                    return length;
                }
            }
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize extra elements
            info.AddValue("constantCellLength", m_constantCellLength);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Constant Cell Length", m_constantCellLength.ToString());

                return baseAttributes;
            }
        }

    }
}
