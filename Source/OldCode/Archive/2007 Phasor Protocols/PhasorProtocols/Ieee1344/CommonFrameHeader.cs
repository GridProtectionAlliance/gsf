//*******************************************************************************************************
//  CommonFrameHeader.vb - IEEE 1344 Common frame header functions
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
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TVA;
using TVA.Interop;
using TVA.DateTime;
using TVA.Parsing;
using TVA.Measurements;

namespace PhasorProtocols
{
    namespace Ieee1344
    {
        // This class generates and parses a frame header specfic to C37.118
        [CLSCompliant(false), Serializable()]
        public sealed class CommonFrameHeader
        {
            #region " Internal Common Frame Header Instance Class "

            // This class is used to temporarily hold parsed frame header
            internal class CommonFrameHeaderInstance : ICommonFrameHeader
            {
                private ulong m_idCode;
                private short m_sampleCount;
                private long m_ticks;
                private MemoryStream m_frameQueue;
                private short m_statusFlags;
                private Dictionary<string, string> m_attributes;
                private object m_tag;

                public CommonFrameHeaderInstance()
                {
                }

                protected CommonFrameHeaderInstance(SerializationInfo info, StreamingContext context)
                {
                    throw (new NotImplementedException());
                }

                ~CommonFrameHeaderInstance()
                {
                    Dispose();
                }

                public void Dispose()
                {
                    GC.SuppressFinalize(this);
                    if (m_frameQueue != null) m_frameQueue.Dispose();
                    m_frameQueue = null;
                }

                public System.Type DerivedType
                {
                    get
                    {
                        return this.GetType();
                    }
                }

                public ulong IDCode
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

                ushort IChannelFrame.IDCode
                {
                    get
                    {
                        if (m_idCode > ushort.MaxValue)
                        {
                            return ushort.MaxValue;
                        }
                        else
                        {
                            return (ushort)m_idCode;
                        }
                    }
                    set
                    {
                        m_idCode = value;
                    }
                }

                public void AppendFrameImage(byte[] binaryImage, int offset, int length)
                {
                    // Validate CRC of frame image being appended
                    if (!ChecksumIsValid(binaryImage, offset, length))
                    {
                        m_frameQueue = null;
                        throw (new InvalidOperationException("Invalid binary image detected - check sum of individual IEEE 1344 interleaved configuration or header frame did not match"));
                    }

                    // Create new frame queue to hold combined binary image, if it doesn't already exist
                    if (m_frameQueue == null)
                    {
                        m_frameQueue = new MemoryStream();

                        // Include initial header in new stream...
                        m_frameQueue.Write(binaryImage, offset, CommonFrameHeader.BinaryLength);
                    }

                    // Skip past header
                    offset += CommonFrameHeader.BinaryLength;

                    // Include frame image
                    m_frameQueue.Write(binaryImage, offset, length - CommonFrameHeader.BinaryLength);
                }

                private bool ChecksumIsValid(byte[] buffer, int startIndex, int length)
                {
                    int sumLength = length - 2;
                    return EndianOrder.BigEndian.ToUInt16(buffer, startIndex + sumLength) == TVA.IO.Compression.Common.CRC16(ushort.MaxValue, buffer, startIndex, sumLength);
                }

                public bool IsFirstFrame
                {
                    get
                    {
                        return CommonFrameHeader.IsFirstFrame(this);
                    }
                    set
                    {
                        CommonFrameHeader.SetIsFirstFrame(this, value);
                    }
                }

                public bool IsLastFrame
                {
                    get
                    {
                        return CommonFrameHeader.IsLastFrame(this);
                    }
                    set
                    {
                        CommonFrameHeader.SetIsLastFrame(this, value);
                    }
                }

                public short FrameCount
                {
                    get
                    {
                        return CommonFrameHeader.FrameCount(this);
                    }
                    set
                    {
                        CommonFrameHeader.SetFrameCount(this, value);
                    }
                }

                public FrameType FrameType
                {
                    get
                    {
                        return CommonFrameHeader.FrameType(this);
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
                        // Translate IEEE 1344 specific frame type to fundamental frame type
                        switch (FrameType)
                        {
                            case Ieee1344.FrameType.DataFrame:
                                return FundamentalFrameType.DataFrame;
                            case Ieee1344.FrameType.ConfigurationFrame:
                                return FundamentalFrameType.ConfigurationFrame;
                            case Ieee1344.FrameType.HeaderFrame:
                                return FundamentalFrameType.HeaderFrame;
                            default:
                                return FundamentalFrameType.Undetermined;
                        }
                    }
                }

                public ushort FrameLength
                {
                    get
                    {
                        if (m_frameQueue != null)
                        {
                            // If we are cumulating frames, we use this total length instead of length parsed from individual frame
                            return BinaryLength;
                        }
                        else
                        {
                            return CommonFrameHeader.FrameLength(this);
                        }
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

                public NtpTimeTag TimeTag
                {
                    get
                    {
                        return CommonFrameHeader.TimeTag(this);
                    }
                }

                UnixTimeTag IChannelFrame.TimeTag
                {
                    get
                    {
                        return new UnixTimeTag(Timestamp);
                    }
                }

                public byte[] BinaryImage
                {
                    get
                    {
                        if (m_frameQueue == null)
                        {
                            return null;
                        }
                        else
                        {
                            return m_frameQueue.ToArray();
                        }
                    }
                }

                int IBinaryDataProvider.BinaryLength
                {
                    get
                    {
                        return (int)BinaryLength;
                    }
                }

                public ushort BinaryLength
                {
                    get
                    {
                        if (m_frameQueue == null)
                        {
                            return 0;
                        }
                        else
                        {
                            return (ushort)m_frameQueue.Length;
                        }
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
                    if (other != null)
                    {
                        return CompareTo(other);
                    }
                    throw (new ArgumentException("Frame can only be compared with other IFrames..."));
                }

                IFrame IFrame.Clone()
                {
                    return this;
                }

                public IDictionary<MeasurementKey, IMeasurement> Measurements
                {
                    get
                    {
                        return null;
                    }
                }

                public int PublishedMeasurements
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
                        m_attributes.Add("64-Bit ID Code", IDCode.ToString());
                        m_attributes.Add("Sample Count", InternalSampleCount.ToString());
                        m_attributes.Add("Status Flags", InternalStatusFlags.ToString());
                        m_attributes.Add("Frame Count", FrameCount.ToString());
                        m_attributes.Add("Is First Frame", IsFirstFrame.ToString());
                        m_attributes.Add("Is Last Frame", IsLastFrame.ToString());

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

            public const ushort BinaryLength = 6;

            private CommonFrameHeader()
            {
                // This class contains only global functions and is not meant to be instantiated
            }

            public static ICommonFrameHeader ParseBinaryImage(ConfigurationFrame configurationFrame, byte[] binaryImage, int startIndex)
            {
                CommonFrameHeaderInstance headerFrame = new CommonFrameHeaderInstance();
                uint secondOfCentury;

                secondOfCentury = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex);
                headerFrame.InternalSampleCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4);

                // We go ahead and pre-grab cell's status flags so we can determine framelength - we
                // leave startindex at 6 so that cell will be able to parse flags as needed - note
                // this increases needed common frame header size by 2 (i.e., BinaryLength + 2)
                headerFrame.InternalStatusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 6);

                if (headerFrame.FrameType == Ieee1344.FrameType.DataFrame && (configurationFrame != null))
                {
                    // Data frames have subsecond time information
                    headerFrame.Ticks = (new NtpTimeTag((double)secondOfCentury + (double)SampleCount(headerFrame) / System.Math.Floor((double)Common.MaximumSampleCount / (double)configurationFrame.Period) / (double)configurationFrame.FrameRate)).ToDateTime().Ticks;
                }
                else
                {
                    // For other frames, the best timestamp you can get is down to the whole second
                    headerFrame.Ticks = (new NtpTimeTag(secondOfCentury)).ToDateTime().Ticks;
                }

                return headerFrame;
            }

            public static byte[] BinaryImage(ICommonFrameHeader frameHeader)
            {
                byte[] buffer = new byte[BinaryLength];

                // Make sure frame length gets included in status flags for generated binary image
                SetFrameLength(frameHeader, frameHeader.BinaryLength);

                EndianOrder.BigEndian.CopyBytes((uint)frameHeader.TimeTag.Value, buffer, 0);
                EndianOrder.BigEndian.CopyBytes(frameHeader.InternalSampleCount, buffer, 4);

                return buffer;
            }

            public static void Clone(ICommonFrameHeader sourceFrameHeader, ICommonFrameHeader destinationFrameHeader)
            {
                destinationFrameHeader.Ticks = sourceFrameHeader.Ticks;
                destinationFrameHeader.InternalSampleCount = sourceFrameHeader.InternalSampleCount;
            }

            public static NtpTimeTag TimeTag(ICommonFrameHeader frameHeader)
            {
                return new NtpTimeTag(new DateTime(frameHeader.Ticks));
            }

            public static FrameType FrameType(ICommonFrameHeader frameHeader)
            {
                return (FrameType)(frameHeader.InternalSampleCount & Common.FrameTypeMask);
            }

            public static void SetFrameType(ICommonFrameHeader frameHeader, FrameType value)
            {
                frameHeader.InternalSampleCount = (short)((frameHeader.InternalSampleCount & ~Common.FrameTypeMask) | (ushort)value);
            }

            public static short SampleCount(ICommonFrameHeader frameHeader)
            {
                return (short)(frameHeader.InternalSampleCount & ~Common.FrameTypeMask);
            }

            public static void SetSampleCount(ICommonFrameHeader frameHeader, short value)
            {
                if (value > Common.MaximumSampleCount)
                {
                    throw (new OverflowException("Sample count value cannot exceed " + Common.MaximumSampleCount));
                }
                else
                {
                    frameHeader.InternalSampleCount = (short)((frameHeader.InternalSampleCount & Common.FrameTypeMask) | (ushort)value);
                }
            }

            public static bool IsFirstFrame(ICommonFrameHeader frameHeader)
            {
                return (frameHeader.InternalSampleCount & Bit.Bit12) == 0;
            }

            public static void SetIsFirstFrame(ICommonFrameHeader frameHeader, bool value)
            {
                if (value)
                {
                    frameHeader.InternalSampleCount = (short)(frameHeader.InternalSampleCount & ~Bit.Bit12);
                }
                else
                {
                    frameHeader.InternalSampleCount = (short)(frameHeader.InternalSampleCount | Bit.Bit12);
                }
            }

            public static bool IsLastFrame(ICommonFrameHeader frameHeader)
            {
                return (frameHeader.InternalSampleCount & Bit.Bit11) == 0;
            }

            public static void SetIsLastFrame(ICommonFrameHeader frameHeader, bool value)
            {
                if (value)
                {
                    frameHeader.InternalSampleCount = (short)(frameHeader.InternalSampleCount & ~Bit.Bit11);
                }
                else
                {
                    frameHeader.InternalSampleCount = (short)(frameHeader.InternalSampleCount | Bit.Bit11);
                }
            }

            public static short FrameCount(ICommonFrameHeader frameHeader)
            {
                return (short)(frameHeader.InternalSampleCount & Common.FrameCountMask);
            }

            public static void SetFrameCount(ICommonFrameHeader frameHeader, short value)
            {
                if (value > Common.MaximumFrameCount)
                {
                    throw (new OverflowException("Frame count value cannot exceed " + Common.MaximumFrameCount));
                }
                else
                {
                    frameHeader.InternalSampleCount = (short)((frameHeader.InternalSampleCount & ~Common.FrameCountMask) | (ushort)value);
                }
            }

            public static ushort FrameLength(ICommonFrameHeader frameHeader)
            {
                return (ushort)(frameHeader.InternalStatusFlags & Common.FrameLengthMask);
            }

            public static void SetFrameLength(ICommonFrameHeader frameHeader, ushort value)
            {
                if (value > Common.MaximumFrameLength)
                {
                    throw (new OverflowException("Frame length value cannot exceed " + Common.MaximumFrameLength));
                }
                else
                {
                    frameHeader.InternalStatusFlags = (short)((frameHeader.InternalStatusFlags & ~Common.FrameLengthMask) | (ushort)value);
                }
            }

            public static ushort DataLength(ICommonFrameHeader frameHeader)
            {
                // Data length will be frame length minus common header length minus crc16
                return (ushort)(FrameLength(frameHeader) - BinaryLength - 2);
            }

            public static void SetDataLength(ICommonFrameHeader frameHeader, ushort value)
            {
                if (value > Common.MaximumDataLength)
                {
                    throw (new OverflowException("Data length value cannot exceed " + Common.MaximumDataLength));
                }
                else
                {
                    SetFrameLength(frameHeader, (ushort)(value + BinaryLength + 2));
                }
            }
        }
    }
}
