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
//using PhasorProtocols.Common;

//*******************************************************************************************************
//  CommandFrameBase.vb - Command frame base class
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
    /// <summary>This class represents the protocol independent common implementation of a command frame that can be sent or received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class CommandFrameBase : ChannelFrameBase<ICommandCell>, ICommandFrame
    {



        private DeviceCommand m_command;

        protected CommandFrameBase()
        {
        }

        protected CommandFrameBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize command frame
            m_command = (DeviceCommand)info.GetValue("command", typeof(DeviceCommand));

        }

        protected CommandFrameBase(CommandCellCollection cells, DeviceCommand command)
            : base(cells)
        {

            m_command = command;

        }

        // Derived classes are expected to expose a Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As int)
        // and automatically pass in parsing state
        protected CommandFrameBase(ICommandFrameParsingState state, byte[] binaryImage, int startIndex)
            : base(state, binaryImage, startIndex)
        {


        }

        // Derived classes are expected to expose a Public Sub New(ByVal commandFrame As ICommandFrame)
        protected CommandFrameBase(ICommandFrame commandFrame)
            : this(commandFrame.Cells, commandFrame.Command)
        {


        }

        protected override FundamentalFrameType FundamentalFrameType
        {
            get
            {
                return PhasorProtocols.FundamentalFrameType.CommandFrame;
            }
        }

        public virtual new CommandCellCollection Cells
        {
            get
            {
                return (CommandCellCollection)base.Cells;
            }
        }

        public virtual DeviceCommand Command
        {
            get
            {
                return m_command;
            }
            set
            {
                m_command = value;
            }
        }

        public virtual byte[] ExtendedData
        {
            get
            {
                return Cells.BinaryImage;
            }
            set
            {
                Cells.Clear();
                base.ParseBodyImage(new CommandFrameParsingState(Cells, 0, (short)value.Length), value, 0);
            }
        }

        protected override ushort BodyLength
        {
            get
            {
                return (ushort)(base.BodyLength + 2);
            }
        }

        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];
                int index = 2;

                EndianOrder.BigEndian.CopyBytes((short)m_command, buffer, 0);
                PhasorProtocols.Common.CopyImage(base.BodyImage, buffer, ref index, base.BodyLength);

                return buffer;
            }
        }

        protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            m_command = (DeviceCommand)EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
            base.ParseBodyImage(state, binaryImage, startIndex + 2);

        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize command frame
            info.AddValue("command", m_command, typeof(DeviceCommand));

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Device Command", (int)Command + ": " + Enum.GetName(typeof(DeviceCommand), Command));

                if (Cells.Count > 0)
                {
                    baseAttributes.Add("Extended Data", ((ByteEncoding)ByteEncoding.Hexadecimal).GetString(Cells.BinaryImage, 0, Cells.Count));
                }
                else
                {
                    baseAttributes.Add("Extended Data", "<null>");
                }

                return baseAttributes;
            }
        }

    }
}
