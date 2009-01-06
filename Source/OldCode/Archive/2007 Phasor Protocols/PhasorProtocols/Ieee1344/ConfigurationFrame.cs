//*******************************************************************************************************
//  ConfigurationFrame.vb - IEEE1344 Configuration Frame
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

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using PCS;
using PCS.Parsing;
using PCS.IO.Checksums;

namespace PCS.PhasorProtocols.Ieee1344
{
    [CLSCompliant(false), Serializable()]
    public class ConfigurationFrame : ConfigurationFrameBase, ISupportFrameImage<FrameType>
    {
        private CommonFrameHeader m_frameHeader;
        private ulong m_idCode;
        private short m_sampleCount;

        public ConfigurationFrame()
            : base(new ConfigurationCellCollection())
        {
        }

        protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


            // Deserialize configuration frame
            m_idCode = info.GetUInt64("idCode64Bit");
            m_sampleCount = info.GetInt16("sampleCount");

        }

        public ConfigurationFrame(ulong idCode, long ticks, short frameRate)
            : base((ushort)(idCode % ushort.MaxValue), new ConfigurationCellCollection(), ticks, frameRate)
        {

            //CommonFrameHeader.SetFrameType(this, Ieee1344.FrameType.ConfigurationFrame);
            m_idCode = idCode;

        }

        //public ConfigurationFrame(ICommonFrameHeader parsedFrameHeader, byte[] binaryImage, int startIndex)
        //    : base(new ConfigurationFrameParsingState(new ConfigurationCellCollection(), parsedFrameHeader.FrameLength, Ieee1344.ConfigurationCell.CreateNewConfigurationCell), binaryImage, startIndex)
        //{
        //    //CommonFrameHeader.SetFrameType(this, Ieee1344.FrameType.ConfigurationFrame);
        //    //CommonFrameHeader.Clone(parsedFrameHeader, this);
        //    //parsedFrameHeader.Dispose();
        //}

        public ConfigurationFrame(IConfigurationFrame configurationFrame)
            : base(configurationFrame)
        {
            //CommonFrameHeader.SetFrameType(this, Ieee1344.FrameType.ConfigurationFrame);
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

        public new ulong IDCode
        {
            get
            {
                return m_idCode;
            }
            set
            {
                m_idCode = value;

                // Base classes constrain maximum value to 65535
                if (m_idCode > ushort.MaxValue)
                    base.IDCode = ushort.MaxValue;
                else
                    base.IDCode = (ushort)value;
            }
        }

        public ushort FrameLength
        {
            get
            {
                return m_frameHeader.FrameLength;
            }
        }

        public ushort DataLength
        {
            get
            {
                return m_frameHeader.DataLength;
            }
        }

        //public short InternalSampleCount
        //{
        //    get
        //    {
        //        return m_sampleCount;
        //    }
        //    set
        //    {
        //        m_sampleCount = value;
        //    }
        //}

        //// Since IEEE 1344 only supports a single PMU there will only be one cell, so we just share status flags with our only child
        //// and expose the value at the parent level for convience in determing frame length at the frame level
        //public short InternalStatusFlags
        //{
        //    get
        //    {
        //        return Cells[0].StatusFlags;
        //    }
        //    set
        //    {
        //        Cells[0].StatusFlags = value;
        //    }
        //}

        public new NtpTimeTag TimeTag
        {
            get
            {
                return m_frameHeader.TimeTag;
            }
        }

        // Since IEEE 1344 only supports a single PMU there will only be one cell, so we just share nominal frequency with our only child
        // and expose the value at the parent level for convience
        public LineFrequency NominalFrequency
        {
            get
            {
                return Cells[0].NominalFrequency;
            }
            set
            {
                Cells[0].NominalFrequency = value;
            }
        }

        public short Period
        {
            get
            {
                return (short)((double)NominalFrequency / (double)FrameRate * 100.0D);
            }
            set
            {
                FrameRate = (short)((double)NominalFrequency * 100.0D / (double)value);
            }
        }

        public FrameType TypeID
        {
            get
            {
                return Ieee1344.FrameType.ConfigurationFrame;
            }
        }

        public CommonFrameHeader CommonHeader
        {
            get
            {
                return m_frameHeader;
            }
            set
            {
                m_frameHeader = value;
            }
        }

        ICommonHeader<FrameType> ISupportFrameImage<FrameType>.CommonHeader
        {
            get
            {
                return (ICommonHeader<FrameType>)m_frameHeader;
            }
            set
            {
                m_frameHeader = (CommonFrameHeader)value;
            }
        }

        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            // IEEE 1344 uses CRC16 to calculate checksum for frames
            return buffer.Xor16CheckSum(offset, length);
        }

        protected override int HeaderLength
        {
            get
            {
                return m_frameHeader.BinaryLength;
            }
        }

        protected override byte[] HeaderImage
        {
            get
            {
                return m_frameHeader.BinaryImage;
            }
        }

        protected override void ParseHeaderImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            // Header was preparsed by common frame header...

            // IEEE 1344 only supports a single PMU...
            ((IConfigurationFrameParsingState)state).CellCount = 1;

        }

        protected override int FooterLength
        {
            get
            {
                return 2;
            }
        }

        protected override byte[] FooterImage
        {
            get
            {
                return EndianOrder.BigEndian.GetBytes(Period);
            }
        }

        protected override void ParseFooterImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {

            Period = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);

        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {

            base.GetObjectData(info, context);

            // Serialize configuration frame
            info.AddValue("idCode64Bit", m_idCode);
            info.AddValue("sampleCount", m_sampleCount);

        }

        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Frame Type", (int)TypeID + ": " + TypeID);
                baseAttributes.Add("Frame Length", FrameLength.ToString());
                baseAttributes.Add("64-Bit ID Code", IDCode.ToString());
                baseAttributes.Add("Sample Count", m_sampleCount.ToString());
                baseAttributes.Add("Period", Period.ToString());

                return baseAttributes;
            }
        }
    }
}