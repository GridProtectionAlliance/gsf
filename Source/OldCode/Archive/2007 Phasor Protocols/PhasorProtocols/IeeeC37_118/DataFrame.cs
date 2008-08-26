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
//  DataFrame.vb - IEEE C37.118 data frame
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

        // This is essentially a "row" of PMU data at a given timestamp
        [CLSCompliant(false), Serializable()]
        public class DataFrame : DataFrameBase, ICommonFrameHeader
        {



            private int m_timeQualityFlags;

            protected DataFrame()
            {
            }

            protected DataFrame(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize data frame
                m_timeQualityFlags = info.GetInt32("timeQualityFlags");

            }

            public DataFrame(long ticks, ConfigurationFrame configurationFrame)
                : base(new DataCellCollection(), ticks, configurationFrame)
            {


            }

            public DataFrame(ICommonFrameHeader parsedFrameHeader, ConfigurationFrame configurationFrame, byte[] binaryImage, int startIndex)
                : base(new DataFrameParsingState(new DataCellCollection(), parsedFrameHeader.FrameLength, configurationFrame, DataCell.CreateNewDataCell), binaryImage, startIndex)
            {


                CommonFrameHeader.Clone(parsedFrameHeader, this);

            }

            public DataFrame(IDataFrame dataFrame)
                : base(dataFrame)
            {


            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new DataCellCollection Cells
            {
                get
                {
                    return (DataCellCollection)base.Cells;
                }
            }

            public new ConfigurationFrame ConfigurationFrame
            {
                get
                {
                    return (ConfigurationFrame)base.ConfigurationFrame;
                }
                set
                {
                    base.ConfigurationFrame = value;
                }
            }

            public FrameType FrameType
            {
                get
                {
                    return IeeeC37_118.FrameType.DataFrame;
                }
                set
                {
                    // Frame type is readonly for data frames - we don't throw an exception here if someone attempts to change
                    // the frame type on a data frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
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
                    return ConfigurationFrame.Version;
                }
                set
                {
                    // Version number is readonly for data frames - we don't throw an exception here if someone attempts to change
                    // the version number on a data frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                    // but we don't do anything with the value either.
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

            public override ushort IDCode
            {
                get
                {
                    return base.IDCode;
                }
                set
                {
                    // ID code is readonly for data frames - we don't throw an exception here if someone attempts to change
                    // the ID code on a data frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                    // but we don't do anything with the value either.
                }
            }

            public int TimeBase
            {
                get
                {
                    return ConfigurationFrame.TimeBase;
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

                // Serialize data frame
                info.AddValue("timeQualityFlags", m_timeQualityFlags);

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
