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
using System.Text;
using TVA.DateTime;

//*******************************************************************************************************
//  HeaderFrameBase.vb - Header frame base class
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
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    /// <summary>This class represents the protocol independent common implementation of a header frame that can be sent or received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class HeaderFrameBase : ChannelFrameBase<IHeaderCell>, IHeaderFrame
    {



        protected HeaderFrameBase()
        {
        }

        protected HeaderFrameBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


        }

        protected HeaderFrameBase(HeaderCellCollection cells)
            : base(cells)
        {


        }

        // Derived classes are expected to expose a Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As int)
        // and automatically pass in parsing state
        protected HeaderFrameBase(IHeaderFrameParsingState state, byte[] binaryImage, int startIndex)
            : base(state, binaryImage, startIndex)
        {


        }

        // Derived classes are expected to expose a Public Sub New(ByVal headerFrame As IHeaderFrame)
        protected HeaderFrameBase(IHeaderFrame headerFrame)
            : this(headerFrame.Cells)
        {


        }

        protected override FundamentalFrameType FundamentalFrameType
        {
            get
            {
                return PhasorProtocols.FundamentalFrameType.HeaderFrame;
            }
        }

        public virtual new HeaderCellCollection Cells
        {
            get
            {
                return (HeaderCellCollection)base.Cells;
            }
        }

        public virtual string HeaderData
        {
            get
            {
                return Encoding.ASCII.GetString(Cells.BinaryImage);
            }
            set
            {
                Cells.Clear();
                ParseBodyImage(new HeaderFrameParsingState(Cells, 0, (short)value.Length), Encoding.ASCII.GetBytes(value), 0);
            }
        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Header Data", HeaderData);

                return baseAttributes;
            }
        }

    }
}
