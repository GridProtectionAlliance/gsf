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
//using System.Buffer;
//using PhasorProtocols.Common;

//*******************************************************************************************************
//  CommandCell.vb - Command cell class
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
    /// <summary>This class represents the protocol independent common implementation of an element of extended frame data of a command frame that can be received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public class CommandCell : ChannelCellBase, ICommandCell
    {



        private byte m_extendedDataByte;

        protected CommandCell()
        {
        }

        protected CommandCell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize command cell value
            m_extendedDataByte = info.GetByte("extendedDataByte");

        }

        public CommandCell(ICommandFrame parent)
            : base(parent, false)
        {


        }

        public CommandCell(ICommandFrame parent, byte[] binaryImage, int startIndex)
            : this(parent)
        {

            ParseBinaryImage(null, binaryImage, startIndex);

        }

        public CommandCell(ICommandFrame parent, byte extendedDataByte)
            : base(parent, false)
        {

            m_extendedDataByte = extendedDataByte;

        }

        public CommandCell(IHeaderCell headerCell)
            : this((ICommandFrame)headerCell.Parent, headerCell.Character)
        {


        }

        internal static ICommandCell CreateNewCommandCell(IChannelFrame parent, IChannelFrameParsingState<ICommandCell> state, int index, byte[] binaryImage, int startIndex)
        {

            return new CommandCell((ICommandFrame)parent, binaryImage, startIndex);

        }

        public override System.Type DerivedType
        {
            get
            {
                return this.GetType();
            }
        }

        public virtual new ICommandFrame Parent
        {
            get
            {
                return (ICommandFrame)base.Parent;
            }
        }

        public virtual byte ExtendedDataByte
        {
            get
            {
                return m_extendedDataByte;
            }
            set
            {
                m_extendedDataByte = value;
            }
        }

        protected override ushort BodyLength
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
                return new byte[] { m_extendedDataByte };
            }
        }

        protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            m_extendedDataByte = binaryImage[startIndex];

        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize command cell value
            info.AddValue("extendedDataByte", m_extendedDataByte);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Extended Data Byte", ExtendedDataByte.ToString("x"));

                return baseAttributes;
            }
        }

    }
}
