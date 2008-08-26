using System.Diagnostics;
using System;
//using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
//using TVA.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;

//*******************************************************************************************************
//  ChannelCellBase.vb - Channel frame cell base class
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


namespace PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent representation of any kind of data cell.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class ChannelCellBase : ChannelBase, IChannelCell
    {



        private IChannelFrame m_parent;
        private ushort m_idCode;
        private bool m_alignOnDWordBoundary;

        protected ChannelCellBase()
        {
        }

        protected ChannelCellBase(SerializationInfo info, StreamingContext context)
        {

            // Deserialize basic channel cell values
            m_parent = (IChannelFrame)info.GetValue("parent", typeof(IChannelFrame));
            m_idCode = info.GetUInt16("id");
            m_alignOnDWordBoundary = info.GetBoolean("alignOnDWordBoundary");

        }

        protected ChannelCellBase(IChannelFrame parent, bool alignOnDWordBoundary)
        {

            m_parent = parent;
            m_alignOnDWordBoundary = alignOnDWordBoundary;

        }

        protected ChannelCellBase(IChannelFrame parent, bool alignOnDWordBoundary, ushort idCode)
            : this(parent, alignOnDWordBoundary)
        {

            m_idCode = idCode;

        }

        // Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As int, ByVal binaryImage As Byte(), ByVal startIndex As int)

        // Derived classes are expected to expose a Protected Sub New(ByVal channelCell As IChannelCell)
        protected ChannelCellBase(IChannelCell channelCell)
            : this(channelCell.Parent, channelCell.AlignOnDWordBoundary, channelCell.IDCode)
        {


        }

        public virtual IChannelFrame Parent
        {
            get
            {
                return m_parent;
            }
        }

        public virtual ushort IDCode
        {
            get
            {
                return m_idCode;
            }
            set
            {
                m_idCode = value;
            }
        }

        public virtual bool AlignOnDWordBoundary
        {
            get
            {
                return m_alignOnDWordBoundary;
            }
        }

        public override ushort BinaryLength
        {
            get
            {
                ushort length = base.BinaryLength;

                if (m_alignOnDWordBoundary)
                {
                    // If requested, we align frame cells on 32-bit word boundries
                    while (!(length % 4 == 0))
                    {
                        length++;
                    }
                }

                return length;
            }
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            // Serialize basic channel cell values
            info.AddValue("parent", m_parent, typeof(IChannelFrame));
            info.AddValue("id", m_idCode);
            info.AddValue("alignOnDWordBoundary", m_alignOnDWordBoundary);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("ID Code", IDCode.ToString());
                baseAttributes.Add("Align on DWord Boundary", AlignOnDWordBoundary.ToString());

                return baseAttributes;
            }
        }

    }
}
