
//*******************************************************************************************************
//  CommandFrame.vb - IEEE1344 Command Frame
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

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel;
using PCS.IO.Checksums;

namespace PCS.PhasorProtocols
{
    namespace Ieee1344
    {

        [CLSCompliant(false), Serializable()]
        public class CommandFrame : CommandFrameBase
        {



            public const ushort FrameLength = 16;

            private ulong m_idCode;

            protected CommandFrame()
            {
            }

            protected CommandFrame(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize command frame
                m_idCode = info.GetUInt64("idCode64Bit");

            }

            public CommandFrame(ulong idCode, DeviceCommand command)
                : base(new CommandCellCollection(0), command)
            {

                m_idCode = idCode;

            }

            public CommandFrame(byte[] binaryImage, int startIndex)
                : base(new CommandFrameParsingState(new CommandCellCollection(0), FrameLength, 0), binaryImage, startIndex)
            {


            }

            public CommandFrame(ICommandFrame commandFrame)
                : base(commandFrame)
            {


            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            // IEEE 1344 command frame doesn't support extended data - so we hide cell collection and extended data property...
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override CommandCellCollection Cells
            {
                get
                {
                    return base.Cells;
                }
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public override byte[] ExtendedData
            {
                get
                {
                    return base.ExtendedData;
                }
                set
                {
                    base.ExtendedData = value;
                }
            }

            public new ulong IDCode
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

            public new NtpTimeTag TimeTag
            {
                get
                {
                    return new NtpTimeTag(new DateTime(Ticks));
                }
            }

            protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
            {
                // IEEE 1344 uses CRC16 to calculate checksum for frames
                return buffer.Crc16Checksum(offset, length);
            }

            protected override ushort HeaderLength
            {
                get
                {
                    return 12;
                }
            }

            protected override byte[] HeaderImage
            {
                get
                {
                    byte[] buffer = new byte[HeaderLength];

                    EndianOrder.BigEndian.CopyBytes((uint)TimeTag.Value, buffer, 0);
                    EndianOrder.BigEndian.CopyBytes(m_idCode, buffer, 4);

                    return buffer;
                }
            }

            protected override void ParseHeaderImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                Ticks = (new NtpTimeTag(EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex))).ToDateTime().Ticks;
                m_idCode = EndianOrder.BigEndian.ToUInt64(binaryImage, startIndex + 4);

            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize command frame
                info.AddValue("idCode64Bit", m_idCode);

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("64-Bit ID Code", IDCode.ToString());

                    return baseAttributes;
                }
            }

        }

    }
}
