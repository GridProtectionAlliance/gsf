//*******************************************************************************************************
//  HeaderFrame.vb - IEEE1344 Header Frame
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
using System.Runtime.Serialization;
using System.Collections.Generic;
using TVA.DateTime;

namespace PhasorProtocols
{
    namespace Ieee1344
    {
        /// <summary>IEEE1344 Header Frame</summary>
        [CLSCompliant(false), Serializable()]
        public class HeaderFrame : HeaderFrameBase, ICommonFrameHeader
        {
            private ulong m_idCode;
            private short m_sampleCount;
            private short m_statusFlags;

            public HeaderFrame()
                : base(new HeaderCellCollection(PhasorProtocols.Ieee1344.Common.MaximumHeaderDataLength))
            {

                CommonFrameHeader.SetFrameType(this, Ieee1344.FrameType.HeaderFrame);

            }

            protected HeaderFrame(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize header frame
                m_idCode = info.GetUInt64("idCode64Bit");
                m_sampleCount = info.GetInt16("sampleCount");
                m_statusFlags = info.GetInt16("statusFlags");

            }

            public HeaderFrame(ICommonFrameHeader parsedFrameHeader, byte[] binaryImage, int startIndex)
                : base(new HeaderFrameParsingState(new HeaderCellCollection(Common.MaximumHeaderDataLength), parsedFrameHeader.FrameLength, (short)(parsedFrameHeader.FrameLength - CommonFrameHeader.BinaryLength - 2)), binaryImage, startIndex)
            {
                CommonFrameHeader.SetFrameType(this, Ieee1344.FrameType.HeaderFrame);
                CommonFrameHeader.Clone(parsedFrameHeader, this);
                parsedFrameHeader.Dispose();
            }

            public HeaderFrame(IHeaderFrame headerFrame)
                : base(headerFrame)
            {

                CommonFrameHeader.SetFrameType(this, Ieee1344.FrameType.HeaderFrame);

            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
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
                    return CommonFrameHeader.TimeTag(this);
                }
            }

            public FrameType FrameType
            {
                get
                {
                    return CommonFrameHeader.FrameType(this);
                }
            }

            FundamentalFrameType ICommonFrameHeader.FundamentalFrameType
            {
                get
                {
                    return base.FundamentalFrameType;
                }
            }

            public ushort FrameLength
            {
                get
                {
                    return CommonFrameHeader.FrameLength(this);
                }
            }

            public ushort DataLength
            {
                get
                {
                    return CommonFrameHeader.DataLength(this);
                }
            }

            public short InternalSampleCount
            {
                get
                {
                    return m_sampleCount;
                }
                set
                {
                    m_sampleCount = value;
                }
            }

            public short InternalStatusFlags
            {
                get
                {
                    return m_statusFlags;
                }
                set
                {
                    m_statusFlags = value;
                }
            }

            protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
            {

                // IEEE 1344 uses CRC16 to calculate checksum for frames
                return TVA.IO.Compression.Common.CRC16(ushort.MaxValue, buffer, offset, length);

            }

            protected override ushort HeaderLength
            {
                get
                {
                    return CommonFrameHeader.BinaryLength;
                }
            }

            protected override byte[] HeaderImage
            {
                get
                {
                    return CommonFrameHeader.BinaryImage(this);
                }
            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize header frame
                info.AddValue("idCode64Bit", m_idCode);
                info.AddValue("sampleCount", m_sampleCount);
                info.AddValue("statusFlags", m_statusFlags);

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Frame Type", (int)FrameType + ": " + Enum.GetName(typeof(FrameType), FrameType));
                    baseAttributes.Add("Frame Length", FrameLength.ToString());
                    baseAttributes.Add("64-Bit ID Code", IDCode.ToString());
                    baseAttributes.Add("Sample Count", m_sampleCount.ToString());

                    return baseAttributes;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
            }
        }
    }
}
