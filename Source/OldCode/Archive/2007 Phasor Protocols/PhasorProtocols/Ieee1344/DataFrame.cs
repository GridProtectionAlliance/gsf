//*******************************************************************************************************
//  DataFrame.vb - IEEE1344 Data Frame
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
using TVA;
using TVA.Interop;
using TVA.DateTime;

namespace PhasorProtocols
{
    namespace Ieee1344
    {
        /// <summary>IEEE1344 Data Frame</summary>
        /// <remarks>This is essentially a "row" of PMU data at a given timestamp</remarks>
        [CLSCompliant(false), Serializable()]
        public class DataFrame : DataFrameBase, ICommonFrameHeader
        {
            private short m_sampleCount;

            protected DataFrame()
            {
            }

            protected DataFrame(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize data frame
                m_sampleCount = info.GetInt16("sampleCount");

            }

            public DataFrame(long ticks, ConfigurationFrame configurationFrame)
                : base(new DataCellCollection(), ticks, configurationFrame)
            {

                CommonFrameHeader.SetFrameType(this, Ieee1344.FrameType.DataFrame);

            }

            public DataFrame(ICommonFrameHeader parsedFrameHeader, ConfigurationFrame configurationFrame, byte[] binaryImage, int startIndex)
                : base(new DataFrameParsingState(new DataCellCollection(), parsedFrameHeader.FrameLength, configurationFrame, Ieee1344.DataCell.CreateNewDataCell), binaryImage, startIndex)
            {
                CommonFrameHeader.SetFrameType(this, Ieee1344.FrameType.DataFrame);
                CommonFrameHeader.Clone(parsedFrameHeader, this);
                parsedFrameHeader.Dispose();
            }

            public DataFrame(IDataFrame dataFrame)
                : base(dataFrame)
            {

                CommonFrameHeader.SetFrameType(this, Ieee1344.FrameType.DataFrame);

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

            public new ulong IDCode
            {
                get
                {
                    return ConfigurationFrame.IDCode;
                }
                set
                {
                    // ID code is readonly for data frames - we don't throw an exception here if someone attempts to change
                    // the ID code on a data frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                    // but we don't do anything with the value either.
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

            public short SampleCount
            {
                get
                {
                    return CommonFrameHeader.SampleCount(this);
                }
                set
                {
                    CommonFrameHeader.SetSampleCount(this, value);
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

            // Since IEEE 1344 only supports a single PMU there will only be one cell, so we just share status flags with our only child
            // and expose the value at the parent level for convience in determing frame length at the frame level
            public short InternalStatusFlags
            {
                get
                {
                    return Cells[0].StatusFlags;
                }
                set
                {
                    Cells[0].StatusFlags = value;
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
                    return Ieee1344.FrameType.DataFrame;
                }
            }

            FundamentalFrameType ICommonFrameHeader.FundamentalFrameType
            {
                get
                {
                    return base.FundamentalFrameType;
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

                // Serialize data frame
                info.AddValue("sampleCount", m_sampleCount);

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Frame Type", (int)FrameType + ": " + Enum.GetName(typeof(FrameType), FrameType));
                    baseAttributes.Add("Frame Length", FrameLength.ToString());
                    baseAttributes.Add("64-Bit ID Code", IDCode.ToString());
                    baseAttributes.Add("Sample Count", SampleCount.ToString());

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
