//*******************************************************************************************************
//  CommonFrameHeader.vb - IEEE C37.118 Common frame header functions
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
using PCS;
using PCS.Parsing;
using PCS.Measurements;

namespace PCS.PhasorProtocols
{
    namespace IeeeC37_118
    {
        // This class generates and parses a frame header specfic to C37.118
        [CLSCompliant(false), Serializable()]
        public sealed class CommonFrameHeader
        {
            #region " Internal Common Frame Header Instance Class "

            // This class is used to temporarily hold parsed frame header
            private class CommonFrameHeaderInstance : ICommonFrameHeader
            {
                private FrameType m_frameType;
                private byte m_version;
                private ushort m_frameLength;
                private ushort m_idCode;
                private long m_ticks;
                private int m_timeQualityFlags;
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
                        return m_frameType;
                    }
                    set
                    {
                        m_frameType = value;
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
                        // Translate IEEE C37.118 specific frame type to fundamental frame type
                        switch (m_frameType)
                        {
                            case IeeeC37_118.FrameType.DataFrame:
                                return FundamentalFrameType.DataFrame;
                            case IeeeC37_118.FrameType.ConfigurationFrame1:
                            case IeeeC37_118.FrameType.ConfigurationFrame2:
                                return FundamentalFrameType.ConfigurationFrame;
                            case IeeeC37_118.FrameType.HeaderFrame:
                                return FundamentalFrameType.HeaderFrame;
                            case IeeeC37_118.FrameType.CommandFrame:
                                return FundamentalFrameType.CommandFrame;
                            default:
                                return FundamentalFrameType.Undetermined;
                        }
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
                        m_version = value;
                    }
                }

                public ushort FrameLength
                {
                    get
                    {
                        return m_frameLength;
                    }
                    set
                    {
                        m_frameLength = value;
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

                public int TimeBase
                {
                    get
                    {
                        return int.MaxValue & ~Common.TimeQualityFlagsMask;
                    }
                }

                public byte[] BinaryImage
                {
                    get
                    {
                        throw (new NotImplementedException());
                    }
                }

                int IBinaryDataProducer.BinaryLength
                {
                    get
                    {
                        return 0;
                    }
                }

                public ushort BinaryLength
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

                public bool Equals(IFrame other)
                {

                    return (CompareTo(other) == 0);

                }

                public int CompareTo(IFrame other)
                {

                    return m_ticks.CompareTo(other.Ticks);

                }

                public int CompareTo(object obj)
                {
                    IFrame other = obj as IFrame;
                    if (other != null) return CompareTo(other);
                    throw new ArgumentException("Frame can only be compared with other IFrames...");
                }

                public IFrame Clone()
                {
                    return this;
                }

                public IDictionary<MeasurementKey, IMeasurement> Measurements
                {
                    get
                    {
                        throw (new NotImplementedException());
                    }
                }

                public int PublishedMeasurements
                {
                    get
                    {
                        return this.IFramePublishedMeasurements;
                    }
                    set
                    {
                        this.IFramePublishedMeasurements = value;
                    }
                }

                public int IFramePublishedMeasurements
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
                    throw new NotImplementedException();
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
                        m_attributes.Add("Fundamental Frame Type", (int)FundamentalFrameType + ": " + FundamentalFrameType);
                        m_attributes.Add("ID Code", IDCode.ToString());
                        m_attributes.Add("Is Partial Frame", IsPartial.ToString());
                        m_attributes.Add("Published", Published.ToString());
                        m_attributes.Add("Ticks", Ticks.ToString());
                        m_attributes.Add("Timestamp", Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        m_attributes.Add("Frame Type", (int)FrameType + ": " + FrameType);
                        m_attributes.Add("Frame Length", FrameLength.ToString());
                        m_attributes.Add("Version", Version.ToString());
                        m_attributes.Add("Second of Century", SecondOfCentury.ToString());
                        m_attributes.Add("Fraction of Second", FractionOfSecond.ToString());
                        m_attributes.Add("Time Quality Flags", (int)TimeQualityFlags + ": " + TimeQualityFlags);
                        m_attributes.Add("Time Quality Indicator Code", (int)TimeQualityIndicatorCode + ": " + TimeQualityIndicatorCode);
                        m_attributes.Add("Time Base", TimeBase.ToString());

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

            public const ushort BinaryLength = 14;

            private CommonFrameHeader()
            {
                // This class contains only global functions and is not meant to be instantiated
            }

            public static ICommonFrameHeader ParseBinaryImage(ConfigurationFrame configurationFrame, byte[] binaryImage, int startIndex)
            {
                if (binaryImage[startIndex] != PhasorProtocols.Common.SyncByte)
                {
                    throw (new InvalidOperationException("Bad data stream, expected sync byte AA as first byte in IEEE C37.118 frame, got " + binaryImage[startIndex].ToString("X").PadLeft(2, '0')));
                }

                CommonFrameHeaderInstance frameHeader = new CommonFrameHeaderInstance();

                // Strip out frame type and version information...
                frameHeader.FrameType = (FrameType)binaryImage[startIndex + 1] & ~FrameType.VersionNumberMask;
                frameHeader.Version = (byte)(binaryImage[startIndex + 1] & (byte)FrameType.VersionNumberMask);

                frameHeader.FrameLength = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 2);
                frameHeader.IDCode = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 4);

                uint secondOfCentury = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 6);
                int fractionOfSecond = EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 10);

                if (configurationFrame == null || frameHeader.FrameType == FrameType.ConfigurationFrame1 || frameHeader.FrameType == FrameType.ConfigurationFrame2)
                {
                    // Without timebase, the best timestamp you can get is down to the whole second
                    frameHeader.Ticks = (new UnixTimeTag(secondOfCentury)).ToDateTime().Ticks;
                }
                else
                {
                    frameHeader.Ticks = (new UnixTimeTag((double)secondOfCentury + (double)(fractionOfSecond & ~Common.TimeQualityFlagsMask) / (double)configurationFrame.TimeBase)).ToDateTime().Ticks;
                }

                frameHeader.InternalTimeQualityFlags = fractionOfSecond & Common.TimeQualityFlagsMask;

                return (ICommonFrameHeader)frameHeader;
            }

            public static byte[] BinaryImage(ICommonFrameHeader frameHeader)
            {
                byte[] buffer = new byte[BinaryLength];

                buffer[0] = PhasorProtocols.Common.SyncByte;
                buffer[1] = (byte)((byte)frameHeader.FrameType | frameHeader.Version);
                EndianOrder.BigEndian.CopyBytes(frameHeader.FrameLength, buffer, 2);
                EndianOrder.BigEndian.CopyBytes(frameHeader.IDCode, buffer, 4);
                EndianOrder.BigEndian.CopyBytes(frameHeader.SecondOfCentury, buffer, 6);
                EndianOrder.BigEndian.CopyBytes(frameHeader.FractionOfSecond | (int)frameHeader.TimeQualityFlags, buffer, 10);

                return buffer;
            }

            public static void Clone(ICommonFrameHeader sourceFrameHeader, ICommonFrameHeader destinationFrameHeader)
            {
                destinationFrameHeader.FrameType = sourceFrameHeader.FrameType;
                destinationFrameHeader.Version = sourceFrameHeader.Version;
                destinationFrameHeader.FrameLength = sourceFrameHeader.FrameLength;
                destinationFrameHeader.IDCode = sourceFrameHeader.IDCode;
                destinationFrameHeader.Ticks = sourceFrameHeader.Ticks;
                destinationFrameHeader.InternalTimeQualityFlags = sourceFrameHeader.InternalTimeQualityFlags;
            }

            public static byte Version(byte newVersion)
            {
                return (byte)(newVersion & (byte)FrameType.VersionNumberMask);
            }

            public static uint SecondOfCentury(ICommonFrameHeader frameHeader)
            {
                return (uint)System.Math.Floor(TimeTag(frameHeader).Value);
            }

            public static int FractionOfSecond(ICommonFrameHeader frameHeader)
            {
                return (int)(((double)TVA.DateTime.Common.get_TicksBeyondSecond(TimeTag(frameHeader).ToDateTime()) / 10000000.0D) * (double)frameHeader.TimeBase);
            }

            public static UnixTimeTag TimeTag(ICommonFrameHeader frameHeader)
            {
                return new UnixTimeTag(new DateTime(frameHeader.Ticks));
            }

            public static IeeeC37_118.TimeQualityFlags TimeQualityFlags(ICommonFrameHeader frameHeader)
            {
                return (IeeeC37_118.TimeQualityFlags)frameHeader.InternalTimeQualityFlags & ~IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask;
            }

            public static void SetTimeQualityFlags(ICommonFrameHeader frameHeader, TimeQualityFlags value)
            {
                frameHeader.InternalTimeQualityFlags = ((frameHeader.InternalTimeQualityFlags & (int)IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask) | (int)value);
            }

            public static IeeeC37_118.TimeQualityIndicatorCode TimeQualityIndicatorCode(ICommonFrameHeader frameHeader)
            {
                return (IeeeC37_118.TimeQualityIndicatorCode)(frameHeader.InternalTimeQualityFlags & (int)IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask);
            }

            public static void SetTimeQualityIndicatorCode(ICommonFrameHeader frameHeader, IeeeC37_118.TimeQualityIndicatorCode value)
            {
                frameHeader.InternalTimeQualityFlags = (frameHeader.InternalTimeQualityFlags & ~(int)IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask) | (int)value;
            }
        }
    }
}
