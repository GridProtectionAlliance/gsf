using System.Diagnostics;
using System;
//using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
//using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;
//using System.Buffer;
using System.Text;
//using PhasorProtocols.Common;

//*******************************************************************************************************
//  HeaderCell.vb - Header cell class
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the protocol independent common implementation of an element of header frame data that can be received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public class HeaderCell : ChannelCellBase, IHeaderCell
    {



        private byte m_character;

        protected HeaderCell()
        {
        }

        protected HeaderCell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize header cell value
            m_character = info.GetByte("character");

        }

        public HeaderCell(IHeaderFrame parent)
            : base(parent, false)
        {


        }

        public HeaderCell(IHeaderFrame parent, byte[] binaryImage, int startIndex)
            : this(parent)
        {

            ParseBinaryImage(null, binaryImage, startIndex);

        }

        public HeaderCell(IHeaderFrame parent, byte character)
            : base(parent, false)
        {

            m_character = character;

        }

        public HeaderCell(IHeaderCell headerCell)
            : this(headerCell.Parent, headerCell.Character)
        {


        }

        internal static IHeaderCell CreateNewHeaderCell(IChannelFrame parent, IChannelFrameParsingState<IHeaderCell> state, int index, byte[] binaryImage, int startIndex)
        {

            return new HeaderCell((IHeaderFrame)parent, binaryImage, startIndex);

        }

        public override System.Type DerivedType
        {
            get
            {
                return this.GetType();
            }
        }

        public virtual new IHeaderFrame Parent
        {
            get
            {
                return (IHeaderFrame)base.Parent;
            }
        }

        public virtual byte Character
        {
            get
            {
                return m_character;
            }
            set
            {
                m_character = value;
            }
        }

        protected override int BodyLength
        {
            get
            {
                return 1;
            }
        }

        protected override byte[] BodyImage
        {
            get
            {
                return new byte[] { m_character };
            }
        }

        protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            m_character = binaryImage[startIndex];

        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize header cell value
            info.AddValue("character", m_character);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Character", Encoding.ASCII.GetString(new byte[] { Character }));

                return baseAttributes;
            }
        }

    }
}
