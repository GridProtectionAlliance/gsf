//*******************************************************************************************************
//  CommonFrameHeader.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using PCS.Measurements;
using PCS.Parsing;

namespace PCS.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the common header for all IEEE C37.118 frames of data.
    /// </summary>
    [Serializable()]
    public class CommonFrameHeader : ICommonHeader<FrameType>, IChannelFrame
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/>.
        /// </summary>
        public const ushort FixedLength = 14;
        
        // Fields
        private FrameType m_frameType;
        private byte m_version;
        private ushort m_frameLength;
        private ushort m_idCode;
        private Ticks m_timestamp;
        private UInt24 m_timebase;
        private uint m_timeQualityFlags;
        private IChannelParsingState m_state;
        private Dictionary<string, string> m_attributes;
        private object m_tag;
		
        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from specified parameters.
        /// </summary>
        /// <param name="typeID">The IEEE C37.118 specific frame type of this frame.</param>
        /// <param name="timestamp">The timestamp of this frame.</param>
        public CommonFrameHeader(FrameType typeID, Ticks timestamp)
        {
            m_frameType = typeID;
            m_timestamp = timestamp;
            m_version = 1;
            m_timebase = (UInt24)100000;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="configurationFrame">IEEE C37.118 <see cref="ConfigurationFrame1"/> if already parsed.</param>
        /// <param name="binaryImage">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        public CommonFrameHeader(ConfigurationFrame1 configurationFrame, byte[] binaryImage, int startIndex)
        {
            if (binaryImage[startIndex] != PhasorProtocols.Common.SyncByte)
                throw new InvalidOperationException("Bad data stream, expected sync byte 0xAA as first byte in IEEE C37.118 frame, got " + binaryImage[startIndex].ToString("X").PadLeft(2, '0'));

            // Strip out frame type and version information...
            m_frameType = (FrameType)binaryImage[startIndex + 1] & ~IeeeC37_118.FrameType.VersionNumberMask;
            m_version = (byte)(binaryImage[startIndex + 1] & (byte)IeeeC37_118.FrameType.VersionNumberMask);

            m_frameLength = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 2);
            m_idCode = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 4);

            uint secondOfCentury = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 6);
            uint fractionOfSecond = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 10);

            // Without timebase, the best timestamp you can get is down to the whole second
            if (configurationFrame == null)
            {
                // The best timestamp you can get is down to the whole second without config frame
                m_timestamp = (new UnixTimeTag((double)secondOfCentury)).ToDateTime().Ticks;
            }
            else
            {
                // If config frame is available, frames have enough information for subsecond time resolution
                m_timebase = (UInt24)configurationFrame.Timebase;
                m_timestamp = (new UnixTimeTag((double)secondOfCentury + ((fractionOfSecond & ~Common.TimeQualityFlagsMask) / (double)m_timebase))).ToDateTime().Ticks;
            }

            m_timeQualityFlags = fractionOfSecond & Common.TimeQualityFlagsMask;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommonFrameHeader(SerializationInfo info, StreamingContext context)
        {
            // Deserialize common frame header
            m_frameType = (FrameType)info.GetValue("frameType", typeof(IeeeC37_118.FrameType));
            m_version = info.GetByte("version");
            m_frameLength = info.GetUInt16("frameLength");
            m_timebase = (UInt24)info.GetUInt32("timebase");
            m_timeQualityFlags = info.GetUInt32("timeQualityFlags");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets timestamp of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public Ticks Timestamp
        {
            get
            {
                return m_timestamp;
            }
            set
            {
                m_timestamp = value;
            }
        }


        /// <summary>
        /// Gets or sets the IEEE C37.118 specific frame type of this frame.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This returns the protocol specific frame classification which uniquely identifies the frame type.
        /// </para>
        /// <para>
        /// This is the <see cref="ICommonHeader{TTypeIdentifier}.TypeID"/> implementation.
        /// </para>
        /// </remarks>
        public FrameType TypeID
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

        /// <summary>
        /// Gets or sets the IEEE C37.118 version of this frame.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the IEEE C37.118 frame length of this frame.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the length of the data in the IEEE C37.118 frame (i.e., the <see cref="FrameLength"/> minus the header length and checksum: <see cref="FrameLength"/> - 8).
        /// </summary>
        public ushort DataLength
        {
            get
            {
                // Data length will be frame length minus common header length minus crc16
                return (ushort)(FrameLength - FixedLength - 2);
            }
            set
            {
                if (value > Common.MaximumDataLength)
                    throw new OverflowException("Data length value cannot exceed " + Common.MaximumDataLength);
                else
                    FrameLength = (ushort)(value + FixedLength + 2);
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 ID code of this frame.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the IEEE C37.118 resolution of fractional time stamps.
        /// </summary>
        public UInt24 Timebase
        {
            get
            {
                return m_timebase;
            }
            set
            {
                m_timebase = value;
            }
        }

        /// <summary>
        /// Gets the IEEE C37.118 second of century.
        /// </summary>
        public uint SecondOfCentury
        {
            get
            {
                return (uint)Math.Truncate(TimeTag.Value);
            }
        }

        /// <summary>
        /// Gets the IEEE C37.118 fraction of second.
        /// </summary>
        public UInt24 FractionOfSecond
        {
            get
            {
                return (UInt24)(m_timestamp.DistanceBeyondSecond().ToSeconds() * m_timebase);
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 <see cref="TimeQualityFlags"/>.
        /// </summary>
        public TimeQualityFlags TimeQualityFlags
        {
            get
            {
                return (TimeQualityFlags)(m_timeQualityFlags & ~(uint)IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask);
            }
            set
            {
                m_timeQualityFlags = (m_timeQualityFlags & (uint)IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask) | (uint)value;
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 <see cref="TimeQualityIndicatorCode"/>.
        /// </summary>
        public TimeQualityIndicatorCode TimeQualityIndicatorCode
        {
            get
            {
                return (TimeQualityIndicatorCode)(m_timeQualityFlags & (uint)IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask);
            }
            set
            {
                m_timeQualityFlags = (m_timeQualityFlags & ~(uint)IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask) | (uint)value;
            }
        }

        /// <summary>
        /// Gets time as a <see cref="UnixTimeTag"/> representing seconds of current <see cref="Timestamp"/>.
        /// </summary>
        public UnixTimeTag TimeTag
        {
            get
            {
                return new UnixTimeTag(m_timestamp);
            }
        }
        
        /// <summary>
        /// Gets or sets the parsing state for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public IChannelParsingState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        // Gets or sets any additional state information - satifies ICommonHeader<FrameType>.State interface property
        object ICommonHeader<FrameType>.State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value as IChannelParsingState;
            }
        }

        /// <summary>
        /// Gets the fundamental frame type of this frame.
        /// </summary>
        /// <remarks>
        /// Frames are generally classified as data, configuration or header frames. This returns the general frame classification.
        /// </remarks>
        public FundamentalFrameType FrameType
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

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return FixedLength;
            }
        }

        /// <summary>
        /// Gets the binary image of the common header portion of this frame.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] buffer = new byte[FixedLength];

                buffer[0] = PhasorProtocols.Common.SyncByte;
                buffer[1] = (byte)((byte)TypeID | Version);
                EndianOrder.BigEndian.CopyBytes(FrameLength, buffer, 2);
                EndianOrder.BigEndian.CopyBytes(IDCode, buffer, 4);
                EndianOrder.BigEndian.CopyBytes(SecondOfCentury, buffer, 6);
                EndianOrder.BigEndian.CopyBytes(FractionOfSecond | (int)TimeQualityFlags, buffer, 10);

                return buffer;
            }
        }

        /// <summary>
        /// Determines if <see cref="IChannelFrame"/> is only partially parsed.
        /// </summary>
        /// <remarks>
        /// This frame is not complete - it only represents the parsed common "header" for frames.
        /// </remarks>
        public bool IsPartial
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get
            {
                // Create a new attributes dictionary or clear the contents of any existing one
                if (m_attributes == null)
                    m_attributes = new Dictionary<string, string>();
                else
                    m_attributes.Clear();

                m_attributes.Add("Derived Type", this.GetType().Name);
                m_attributes.Add("Binary Length", FixedLength.ToString());
                m_attributes.Add("Total Cells", "0");
                m_attributes.Add("Fundamental Frame Type", (int)FrameType + ": " + FrameType);
                m_attributes.Add("ID Code", "undefined");
                m_attributes.Add("Is Partial Frame", IsPartial.ToString());
                m_attributes.Add("Published", "n/a");
                m_attributes.Add("Ticks", "undefined");
                m_attributes.Add("Timestamp", "n/a");
                AppendHeaderAttributes(m_attributes);

                return m_attributes;
            }
        }

        /// <summary>
        /// User definable object used to hold a reference associated with the <see cref="IChannel"/> object.
        /// </summary>
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

        #region [ IChannelFrame Implementation ]

        ushort IChannelFrame.IDCode
        {
            get
            {
                return 0;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        object IChannelFrame.Cells
        {
            get
            {
                return null;
            }
        }

        int ISupportBinaryImage.Initialize(byte[] binaryImage, int startIndex, int length)
        {
            // The common frame header is parsed during construction
            throw new NotImplementedException();
        }

        int IFrame.PublishedMeasurements
        {
            get
            {
                return 0;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        IDictionary<MeasurementKey, IMeasurement> IFrame.Measurements
        {
            get
            {
                return null;
            }
        }

        bool IFrame.Published
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        IMeasurement IFrame.LastSortedMeasurement
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="IFrame"/>.
        /// </summary>
        /// <param name="other">An <see cref="IFrame"/> value to compare to this instance.</param>
        /// <returns>
        /// True if <paramref name="other"/> has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(IFrame other)
        {
            return (CompareTo(other) == 0);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="IFrame"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="other">An <see cref="IFrame"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(IFrame other)
        {
            return (this as IFrame).Timestamp.CompareTo(other.Timestamp);
        }

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        /// <exception cref="ArgumentException">value is not an <see cref="IFrame"/>.</exception>
        public int CompareTo(object obj)
        {
            IFrame other = obj as IFrame;

            if (other != null)
                return CompareTo(other);

            throw new ArgumentException("Frame can only be compared with other IFrames...");
        }

        /// <summary>
        /// Appends header specific attributes to <paramref name="attributes"/> dictionary.
        /// </summary>
        /// <param name="attributes">Dictionary to append header specific attributes to.</param>
        internal void AppendHeaderAttributes(Dictionary<string, string> attributes)
        {
            attributes.Add("Frame Type", (int)TypeID + ": " + TypeID);
            attributes.Add("Frame Length", FrameLength.ToString());
            attributes.Add("Version", Version.ToString());
            attributes.Add("Second of Century", SecondOfCentury.ToString());
            attributes.Add("Fraction of Second", FractionOfSecond.ToString());
            attributes.Add("Time Quality Flags", (int)TimeQualityFlags + ": " + TimeQualityFlags);
            attributes.Add("Time Quality Indicator Code", (int)TimeQualityIndicatorCode + ": " + TimeQualityIndicatorCode);
            attributes.Add("Time Base", Timebase.ToString());
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize unique common frame header values
            info.AddValue("frameType", m_frameType, typeof(IeeeC37_118.FrameType));
            info.AddValue("version", m_version);
            info.AddValue("frameLength", m_frameLength);
            info.AddValue("timebase", (uint)m_timebase);
            info.AddValue("timeQualityFlags", m_timeQualityFlags);
        }

        #endregion
    }
}