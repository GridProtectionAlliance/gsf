using System.Diagnostics;
using System;
////using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
////using TVA.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;
//using PhasorProtocols.IeeeC37_118.Common;

//*******************************************************************************************************
//  CommandFrame.vb - IEEE C37.118 command frame
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


namespace PhasorProtocols
{
    namespace IeeeC37_118
    {

        [CLSCompliant(false), Serializable()]
        public class CommandFrame : CommandFrameBase, ICommonFrameHeader
        {



            private byte m_version;

            protected CommandFrame()
            {
            }

            protected CommandFrame(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize command frame
                m_version = info.GetByte("version");

            }

            public CommandFrame(ushort idCode, DeviceCommand command, byte version)
                : base(new CommandCellCollection(Common.MaximumExtendedDataLength), command)
            {

                base.IDCode = idCode;
                m_version = version;

            }

            public CommandFrame(ICommonFrameHeader parsedFrameHeader, byte[] binaryImage, int startIndex)
                : base(new CommandFrameParsingState(new CommandCellCollection(Common.MaximumExtendedDataLength), parsedFrameHeader.FrameLength, (short)(parsedFrameHeader.FrameLength - CommonFrameHeader.BinaryLength - 4)), binaryImage, startIndex)
            {


                CommonFrameHeader.Clone(parsedFrameHeader, this);

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

            public FrameType FrameType
            {
                get
                {
                    return IeeeC37_118.FrameType.CommandFrame;
                }
                set
                {
                    // Frame type is readonly for command frames - we don't throw an exception here if someone attempts to change
                    // the frame type on a command frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                    // but we don't do anything with the value either.
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
                    // Command frame doesn't need subsecond time resolution - so this factor is just defaulted to max...
                    return int.MaxValue & ~Common.TimeQualityFlagsMask;
                }
            }

            public int InternalTimeQualityFlags
            {
                get
                {
                    return 0;
                }
                set
                {
                    // Time quality flags are readonly for command frames - we don't throw an exception here if someone attempts to change
                    // the time quality on a command frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                    // but we don't do anything with the value either.
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
                    // Nothing to do - time quality flags is readonly for command frames
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
                    // Nothing to do - time quality flags is readonly for command frames
                }
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

                // Serialize command frame
                info.AddValue("version", m_version);

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Frame Type", (int)FrameType + ": " + Enum.GetName(typeof(FrameType), FrameType));
                    baseAttributes.Add("Frame Length", FrameLength.ToString());
                    baseAttributes.Add("Version", Version.ToString());
                    baseAttributes.Add("Second of Century", SecondOfCentury.ToString());
                    baseAttributes.Add("Fraction of Second", FractionOfSecond.ToString());
                    baseAttributes.Add("Time Quality Flags", (int)TimeQualityFlags + ": " + Enum.GetName(typeof(TimeQualityFlags), TimeQualityFlags));
                    baseAttributes.Add("Time Quality Indicator Code", (int)TimeQualityIndicatorCode + ": " + Enum.GetName(typeof(TimeQualityIndicatorCode), TimeQualityIndicatorCode));
                    baseAttributes.Add("Time Base", TimeBase.ToString());

                    return baseAttributes;
                }
            }

        }

    }
}
