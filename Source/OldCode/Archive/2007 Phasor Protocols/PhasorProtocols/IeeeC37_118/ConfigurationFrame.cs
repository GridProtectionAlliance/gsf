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
using System.Text;
//using PhasorProtocols.Common;
//using PhasorProtocols.IeeeC37_118.Common;

//*******************************************************************************************************
//  ConfigurationFrame.vb - IEEE C37.118 Configuration Frame
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
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PCS.PhasorProtocols
{
    namespace IeeeC37_118
    {

        [CLSCompliant(false), Serializable()]
        public class ConfigurationFrame : ConfigurationFrameBase, ICommonFrameHeader
        {



            private FrameType m_frameType;
            private byte m_version;
            private int m_timeBase;
            private int m_timeQualityFlags;

            protected ConfigurationFrame()
            {
            }

            protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize configuration frame
                m_frameType = (FrameType)info.GetValue("frameType", typeof(FrameType));
                m_version = info.GetByte("version");
                m_timeBase = info.GetInt32("timeBase");
                m_timeQualityFlags = info.GetInt32("timeQualityFlags");

            }

            public ConfigurationFrame(FrameType frameType, int timeBase, ushort idCode, long ticks, short frameRate, byte version)
                : base(idCode, new ConfigurationCellCollection(), ticks, frameRate)
            {

                this.FrameType = frameType;
                m_timeBase = timeBase;
                m_version = version;

            }

            public ConfigurationFrame(ICommonFrameHeader parsedFrameHeader, byte[] binaryImage, int startIndex)
                : base(new ConfigurationFrameParsingState(new ConfigurationCellCollection(), parsedFrameHeader.FrameLength, IeeeC37_118.ConfigurationCell.CreateNewConfigurationCell), binaryImage, startIndex)
            {


                CommonFrameHeader.Clone(parsedFrameHeader, this);

            }

            public ConfigurationFrame(IConfigurationFrame configurationFrame)
                : base(configurationFrame)
            {


                // Assign default values for a 37.118 configuration frame
                m_frameType = IeeeC37_118.FrameType.ConfigurationFrame2;
                m_version = 1;
                m_timeBase = 100000;

            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new ConfigurationCellCollection Cells
            {
                get
                {
                    return (ConfigurationCellCollection)base.Cells;
                }
            }

            public virtual DraftRevision DraftRevision
            {
                get
                {
                    return IeeeC37_118.DraftRevision.Draft7;
                }
            }

            public FrameType FrameType
            {
                get
                {
                    return m_frameType;
                }
                set
                {
                    if (value == IeeeC37_118.FrameType.ConfigurationFrame2 || value == IeeeC37_118.FrameType.ConfigurationFrame1)
                    {
                        m_frameType = value;
                    }
                    else
                    {
                        throw (new InvalidCastException("Invalid frame type specified for configuration frame.  Can only be ConfigurationFrame1 or ConfigurationFrame2"));
                    }
                }
            }

            FundamentalFrameType ICommonFrameHeader.FundamentalFrameType
            {
                get
                {
                    return base.FundamentalFrameType;
                }
            }

            public byte Version
            {
                get
                {
                    return m_version;
                }
                set
                {
                    m_version = CommonFrameHeader.Version(value);
                }
            }

            public ushort FrameLength
            {
                get
                {
                    return base.BinaryLength;
                }
                set
                {
                    base.ParsedBinaryLength = value;
                }
            }

            public int TimeBase
            {
                get
                {
                    return m_timeBase;
                }
                set
                {
                    if (value == 0)
                    {
                        m_timeBase = 1000000;
                    }
                    else
                    {
                        m_timeBase = value;
                    }
                }
            }

            int ICommonFrameHeader.TimeBase
            {
                get
                {
                    return m_timeBase;
                }
            }

            public int InternalTimeQualityFlags
            {
                get
                {
                    return m_timeQualityFlags;
                }
                set
                {
                    m_timeQualityFlags = value;
                }
            }

            public uint SecondOfCentury
            {
                get
                {
                    return CommonFrameHeader.SecondOfCentury(this);
                }
            }

            public int FractionOfSecond
            {
                get
                {
                    return CommonFrameHeader.FractionOfSecond(this);
                }
            }

            public TimeQualityFlags TimeQualityFlags
            {
                get
                {
                    return CommonFrameHeader.TimeQualityFlags(this);
                }
                set
                {
                    CommonFrameHeader.SetTimeQualityFlags(this, value);
                }
            }

            public TimeQualityIndicatorCode TimeQualityIndicatorCode
            {
                get
                {
                    return CommonFrameHeader.TimeQualityIndicatorCode(this);
                }
                set
                {
                    CommonFrameHeader.SetTimeQualityIndicatorCode(this, value);
                }
            }

            protected override ushort HeaderLength
            {
                get
                {
                    return CommonFrameHeader.BinaryLength + 6;
                }
            }

            protected override byte[] HeaderImage
            {
                get
                {
                    byte[] buffer = new byte[HeaderLength];
                    int index = 0;

                    PhasorProtocols.Common.CopyImage(CommonFrameHeader.BinaryImage(this), buffer, ref index, CommonFrameHeader.BinaryLength);
                    EndianOrder.BigEndian.CopyBytes(m_timeBase, buffer, index);
                    EndianOrder.BigEndian.CopyBytes((short)Cells.Count, buffer, index + 4);

                    return buffer;
                }
            }

            protected override void ParseHeaderImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                // We parse the C37.18 stream specific header image here...
                IConfigurationFrameParsingState parsingState = (IConfigurationFrameParsingState)state;

                m_timeBase = EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 14);
                parsingState.CellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 18);

            }

            protected override ushort FooterLength
            {
                get
                {
                    if (DraftRevision == DraftRevision.Draft6)
                    {
                        return 2;
                    }
                    else
                    {
                        return 4;
                    }
                }
            }

            protected override byte[] FooterImage
            {
                get
                {
                    byte[] buffer = new byte[FooterLength];

                    EndianOrder.BigEndian.CopyBytes(FrameRate, buffer, 0);

                    return buffer;
                }
            }

            protected override void ParseFooterImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                FrameRate = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);

            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize configuration frame
                info.AddValue("frameType", m_frameType, typeof(FrameType));
                info.AddValue("version", m_version);
                info.AddValue("timeBase", m_timeBase);
                info.AddValue("timeQualityFlags", m_timeQualityFlags);

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Frame Type", (int)FrameType + ": " + FrameType);
                    baseAttributes.Add("Frame Length", FrameLength.ToString());
                    baseAttributes.Add("Version", Version.ToString());
                    baseAttributes.Add("Second of Century", SecondOfCentury.ToString());
                    baseAttributes.Add("Fraction of Second", FractionOfSecond.ToString());
                    baseAttributes.Add("Time Quality Flags", (int)TimeQualityFlags + ": " + TimeQualityFlags);
                    baseAttributes.Add("Time Quality Indicator Code", (int)TimeQualityIndicatorCode + ": " + TimeQualityIndicatorCode);
                    baseAttributes.Add("Time Base", TimeBase.ToString());
                    baseAttributes.Add("Draft Revision", (int)DraftRevision + ": " + DraftRevision);

                    return baseAttributes;
                }
            }

        }

    }
}
