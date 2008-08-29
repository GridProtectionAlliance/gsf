//*******************************************************************************************************
//  CommonFrameHeader.vb - BPA PDCstream Common frame header functions
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

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TVA;
using TVA.Interop;
using TVA.DateTime;
using TVA.Parsing;
using TVA.Measurements;

namespace PhasorProtocols
{
    namespace BpaPdcStream
    {
        // This class generates and parses a frame header specfic to BPA PDCstream
        [CLSCompliant(false), Serializable()]
        public sealed class CommonFrameHeader
        {
            #region " Internal Common Frame Header Instance Class "

            // This class is used to temporarily hold parsed frame header
            private class CommonFrameHeaderInstance : ICommonFrameHeader
            {
                private byte m_packetFlag;
                private short m_wordCount;
                private ushort m_idCode;
                private short m_sampleNumber;
                private long m_ticks;
                private Dictionary<string, string> m_attributes;
                private object m_tag;

                public CommonFrameHeaderInstance()
                {
                }

                protected CommonFrameHeaderInstance(SerializationInfo info, StreamingContext context)
                {
                    throw (new NotImplementedException());
                }

                public System.Type DerivedType
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
                        return ((FrameType)m_packetFlag == BpaPdcStream.FrameType.ConfigurationFrame ? BpaPdcStream.FrameType.ConfigurationFrame : BpaPdcStream.FrameType.DataFrame);
                    }
                }

                FundamentalFrameType IChannelFrame.FrameType
                {
                    get
                    {
                        return this.FundamentalFrameType;
                    }
                }

                public FundamentalFrameType FundamentalFrameType
                {
                    get
                    {
                        // Translate BPA PDCstream specific frame type to fundamental frame type
                        switch (FrameType)
                        {
                            case BpaPdcStream.FrameType.ConfigurationFrame:
                                return FundamentalFrameType.ConfigurationFrame;
                            case BpaPdcStream.FrameType.DataFrame:
                                return FundamentalFrameType.DataFrame;
                            default:
                                return FundamentalFrameType.Undetermined;
                        }
                    }
                }

                public ushort FrameLength
                {
                    get
                    {
                        return (ushort)(2 * m_wordCount);
                    }
                }

                public byte PacketNumber
                {
                    get
                    {
                        return this.PacketFlag;
                    }
                    set
                    {
                        this.PacketFlag = value;
                    }
                }

                public byte PacketFlag
                {
                    get
                    {
                        return m_packetFlag;
                    }
                    set
                    {
                        m_packetFlag = value;
                    }
                }

                public short WordCount
                {
                    get
                    {
                        return m_wordCount;
                    }
                    set
                    {
                        m_wordCount = value;
                    }
                }

                public short SampleNumber
                {
                    get
                    {
                        return m_sampleNumber;
                    }
                    set
                    {
                        m_sampleNumber = value;
                    }
                }

                public ushort IDCode
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

                public long Ticks
                {
                    get
                    {
                        return m_ticks;
                    }
                    set
                    {
                        m_ticks = value;
                    }
                }

                public IMeasurement LastSortedMeasurement
                {
                    get
                    {
                        return null;
                    }
                    set
                    {
                        throw (new NotImplementedException());
                    }
                }

                public byte[] BinaryImage
                {
                    get
                    {
                        throw (new NotImplementedException());
                    }
                }

                public ushort BinaryLength
                {
                    get
                    {
                        return 0;
                    }
                }

                int IBinaryDataProvider.BinaryLength
                {
                    get
                    {
                        return 0;
                    }
                }

                public void ParseBinaryImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
                {

                    throw (new NotImplementedException());

                }

                public object Cells
                {
                    get
                    {
                        return null;
                    }
                }

                public bool Published
                {
                    get
                    {
                        return false;
                    }
                    set
                    {
                        throw (new NotImplementedException());
                    }
                }

                // This frame is not complete - it only represents the parsed common "header" for frames
                public bool IsPartial
                {
                    get
                    {
                        return true;
                    }
                }

                public DateTime Timestamp
                {
                    get
                    {
                        return new DateTime(m_ticks);
                    }
                }

                public UnixTimeTag TimeTag
                {
                    get
                    {
                        return new UnixTimeTag(Timestamp);
                    }
                }

                public bool Equals(TVA.Measurements.IFrame other)
                {

                    return (CompareTo(other) == 0);

                }

                public int CompareTo(TVA.Measurements.IFrame other)
                {

                    return m_ticks.CompareTo(other.Ticks);

                }

                public int CompareTo(object obj)
                {
                    IFrame other = obj as IFrame;
                    if (other != null) return CompareTo(other);
                    throw new ArgumentException("Frame can only be compared with other IFrames...");
                }

                IFrame IFrame.Clone()
                {
                    return this;
                }

                IDictionary<MeasurementKey, IMeasurement> IFrame.Measurements
                {
                    get
                    {
                        return null;
                    }
                }

                int IFrame.PublishedMeasurements
                {
                    get
                    {
                        return 0;
                    }
                    set
                    {
                        throw (new NotImplementedException());
                    }
                }

                public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                {
                    throw (new NotImplementedException());
                }

                public Dictionary<string, string> Attributes
                {
                    get
                    {
                        // Create a new attributes dictionary or clear the contents of any existing one
                        if (m_attributes == null)
                        {
                            m_attributes = new Dictionary<string, string>();
                        }
                        else
                        {
                            m_attributes.Clear();
                        }

                        m_attributes.Add("Derived Type", DerivedType.Name);
                        m_attributes.Add("Binary Length", BinaryLength.ToString());
                        m_attributes.Add("Total Cells", "0");
                        m_attributes.Add("Fundamental Frame Type", (int)FundamentalFrameType + ": " + Enum.GetName(typeof(FundamentalFrameType), FundamentalFrameType));
                        m_attributes.Add("ID Code", IDCode.ToString());
                        m_attributes.Add("Is Partial Frame", IsPartial.ToString());
                        m_attributes.Add("Published", Published.ToString());
                        m_attributes.Add("Ticks", Ticks.ToString());
                        m_attributes.Add("Timestamp", Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        m_attributes.Add("Frame Type", (int)FrameType + ": " + Enum.GetName(typeof(FrameType), FrameType));
                        m_attributes.Add("Frame Length", FrameLength.ToString());
                        m_attributes.Add("Packet Flag", m_packetFlag.ToString());
                        m_attributes.Add("Word Count", m_wordCount.ToString());
                        m_attributes.Add("Sample Number", m_sampleNumber.ToString());

                        return m_attributes;
                    }
                }

                public object Tag
                {
                    get
                    {
                        return m_tag;
                    }
                    set
                    {
                        m_tag = value;
                    }
                }

            }

            #endregion

            public const ushort BinaryLength = 4;

            private CommonFrameHeader()
            {
                // This class contains only global functions and is not meant to be instantiated
            }

            // Note: in order to parse timestamp from data frame, this parse procedure needs six more bytes above and beyond common frame header binary length
            public static ICommonFrameHeader ParseBinaryImage(ConfigurationFrame configurationFrame, bool parseWordCountFromByte, byte[] binaryImage, int startIndex)
            {
                if (binaryImage[startIndex] != PhasorProtocols.Common.SyncByte)
                {
                    throw (new InvalidOperationException("Bad data stream, expected sync byte AA as first byte in BPA PDCstream frame, got " + binaryImage[startIndex].ToString("X").PadLeft(2, '0')));
                }

                CommonFrameHeaderInstance commonFrameHeader = new CommonFrameHeaderInstance();

                // Parse out packet flags and word count information...
                commonFrameHeader.PacketFlag = binaryImage[startIndex + 1];

                // Some older streams have a bad word count (e.g., the NYISO data stream has a 0x01 as the third byte
                // in the stream - this should be a 0x00 to make the word count come out correctly).  The following
                // compensates for this erratic behavior
                if (parseWordCountFromByte)
                {
                    commonFrameHeader.WordCount = binaryImage[startIndex + 3];
                }
                else
                {
                    commonFrameHeader.WordCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2);
                }

                if (commonFrameHeader.FrameType == FrameType.ConfigurationFrame)
                {
                    // We just assume current timestamp for configuration frames since they don't provide one
                    commonFrameHeader.Ticks = DateTime.UtcNow.Ticks;
                }
                else
                {
                    // Next six bytes in data frame is the timestamp - so we go ahead and get it
                    uint secondOfCentury = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 4);
                    commonFrameHeader.SampleNumber = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 8);

                    if (configurationFrame == null)
                    {
                        // Until configuration is available, we make a guess at time tag type - this will just be
                        // used for display purposes until a configuration frame arrives.  If second of century
                        // is greater than 3155673600 (SOC value for NTP timestamp 1/1/2007), then this is likely
                        // an NTP time stamp (else this is a Unix time tag for the year 2069 - not likely).
                        if (secondOfCentury > 3155673600)
                        {
                            commonFrameHeader.Ticks = (new NtpTimeTag(secondOfCentury)).ToDateTime().Ticks;
                        }
                        else
                        {
                            commonFrameHeader.Ticks = (new UnixTimeTag(secondOfCentury)).ToDateTime().Ticks;
                        }
                    }
                    else
                    {
                        if (configurationFrame.RevisionNumber == RevisionNumber.Revision0)
                        {
                            commonFrameHeader.Ticks = (new NtpTimeTag(secondOfCentury)).ToDateTime().Ticks + (commonFrameHeader.SampleNumber * (long)configurationFrame.TicksPerFrame);
                        }
                        else
                        {
                            commonFrameHeader.Ticks = (new UnixTimeTag(secondOfCentury)).ToDateTime().Ticks + (commonFrameHeader.SampleNumber * (long)configurationFrame.TicksPerFrame);
                        }
                    }
                }

                return (ICommonFrameHeader)commonFrameHeader;
            }

            public static byte[] BinaryImage(ICommonFrameHeader frameHeader)
            {
                byte[] buffer = new byte[BinaryLength];

                buffer[0] = PhasorProtocols.Common.SyncByte;
                buffer[1] = frameHeader.PacketNumber;
                EndianOrder.BigEndian.CopyBytes(frameHeader.WordCount, buffer, 2);

                return buffer;
            }

            public static void Clone(ICommonFrameHeader sourceFrameHeader, ICommonFrameHeader destinationFrameHeader)
            {
                destinationFrameHeader.PacketNumber = sourceFrameHeader.PacketNumber;
                destinationFrameHeader.WordCount = sourceFrameHeader.WordCount;
                destinationFrameHeader.Ticks = sourceFrameHeader.Ticks;
                destinationFrameHeader.SampleNumber = sourceFrameHeader.SampleNumber;
            }
        }
    }
}
