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
using TVA;
using TVA.IO.Checksums;
using TVA.Parsing;

namespace PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Represents the common header for all IEEE 1344 frames of data.
    /// </summary>
    [Serializable()]
    public class CommonFrameHeader : ICommonHeader<FrameType>, ISerializable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/>.
        /// </summary>
        public const ushort FixedLength = 6;

        // Fields
        private FrameImageCollector m_frameImages;
        private ushort m_sampleCount;
        private ushort m_statusFlags;
        private Ticks m_timestamp;
        private IChannelParsingState m_state;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from specified parameters.
        /// </summary>
        /// <param name="typeID">The IEEE 1344 specific frame type of this frame.</param>
        /// <param name="timestamp">The timestamp of this frame.</param>
        public CommonFrameHeader(FrameType typeID, Ticks timestamp)
        {
            m_timestamp = timestamp;
            TypeID = typeID;
            FrameCount = 1;
            IsFirstFrame = true;
            IsLastFrame = true;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="configurationFrame">IEEE 1344 <see cref="ConfigurationFrame"/> if already parsed.</param>
        /// <param name="binaryImage">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        public CommonFrameHeader(ConfigurationFrame configurationFrame, byte[] binaryImage, int startIndex)
        {
            uint secondOfCentury = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex);
            m_sampleCount = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 4);

            // We go ahead and pre-grab cell's status flags so we can determine framelength - we
            // leave startindex at 6 so that cell will be able to parse flags as needed - note
            // this increases needed common frame header size by 2 (i.e., BinaryLength + 2)
            m_statusFlags = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + FixedLength);

            // NTP timestamps based on NtpTimeTag class are designed to work for dates between
            // 1968-01-20 and 2104-02-26 based on recommended bit interpretation in RFC-2030.
            NtpTimeTag timetag = new NtpTimeTag(secondOfCentury, 0);

            // Data frames have subsecond time information, so we add this fraction of time to current seconds value
            if (TypeID == Ieee1344.FrameType.DataFrame && configurationFrame != null)
                timetag.Value += SampleCount / Math.Truncate((double)Common.MaximumSampleCount / (double)configurationFrame.Period) / (double)configurationFrame.FrameRate;

            // Cache timestamp value
            m_timestamp = timetag.ToDateTime().Ticks;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommonFrameHeader(SerializationInfo info, StreamingContext context)
        {
            // Deserialize common frame header
            m_sampleCount = info.GetUInt16("sampleCount");
            m_statusFlags = info.GetUInt16("statusFlags");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the timestamp of this frame in NTP format.
        /// </summary>
        public NtpTimeTag TimeTag
        {
            get
            {
                return new NtpTimeTag(m_timestamp);
            }
        }

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
        /// Gets or sets the IEEE 1344 specific frame type of this frame.
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
                return (FrameType)(m_sampleCount & Common.FrameTypeMask);
            }
            set
            {
                m_sampleCount = (ushort)((m_sampleCount & ~Common.FrameTypeMask) | (ushort)value);
            }
        }

        /// <summary>
        /// Gets or sets flag from that determines if this is the first frame image in a series of images representing an entire frame.
        /// </summary>
        public bool IsFirstFrame
        {
            get
            {
                return (m_sampleCount & (ushort)Bits.Bit12) == 0;
            }
            set
            {
                if (value)
                    m_sampleCount = (ushort)(m_sampleCount & ~(ushort)Bits.Bit12);
                else
                    m_sampleCount = (ushort)(m_sampleCount | (ushort)Bits.Bit12);
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this is the last frame image in a series of images representing an entire frame.
        /// </summary>
        public bool IsLastFrame
        {
            get
            {
                return (m_sampleCount & (ushort)Bits.Bit11) == 0;
            }
            set
            {
                if (value)
                    m_sampleCount = (ushort)(m_sampleCount & ~(ushort)Bits.Bit11);
                else
                    m_sampleCount = (ushort)(m_sampleCount | (ushort)Bits.Bit11);
            }
        }

        /// <summary>
        /// Gets or sets the total frame count.
        /// </summary>
        public ushort FrameCount
        {
            get
            {
                return (ushort)(m_sampleCount & Common.FrameCountMask);
            }
            set
            {
                if (value > Common.MaximumFrameCount)
                    throw new OverflowException("Frame count value cannot exceed " + Common.MaximumFrameCount);
                else
                    m_sampleCount = (ushort)((m_sampleCount & ~Common.FrameCountMask) | value);
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
                // Translate IEEE 1344 specific frame type to fundamental frame type
                switch (TypeID)
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

        /// <summary>
        /// Gets or sets reference to a <see cref="FrameImageCollector"/> associated the IEEE 1344 frame, if any.
        /// </summary>
        public FrameImageCollector FrameImages
        {
            get
            {
                return m_frameImages;
            }
            set
            {
                m_frameImages = value;
            }
        }

        /// <summary>
        /// Gets or sets the entire length of the IEEE 1344 frame.
        /// </summary>
        public ushort FrameLength
        {
            get
            {
                return (ushort)(m_statusFlags & Common.FrameLengthMask);
            }
            set
            {
                if (value > Common.MaximumFrameLength)
                    throw new OverflowException("Frame length value cannot exceed " + Common.MaximumFrameLength);
                else
                    m_statusFlags = (ushort)((m_statusFlags & ~Common.FrameLengthMask) | value);
            }
        }

        /// <summary>
        /// Gets or sets the length of the data in the IEEE 1344 frame (i.e., the <see cref="FrameLength"/> minus the header length and checksum: <see cref="FrameLength"/> - 8).
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
        /// Gets or sets the sample number (i.e., frame count) of this frame.
        /// </summary>
        public ushort SampleCount
        {
            get
            {
                return (ushort)(m_sampleCount & ~Common.FrameTypeMask);
            }
            set
            {
                if (value > Common.MaximumSampleCount)
                    throw new OverflowException("Sample count value cannot exceed " + Common.MaximumSampleCount);
                else
                    m_sampleCount = (ushort)((m_sampleCount & Common.FrameTypeMask) | value);
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

                // Make sure frame length gets included in status flags for generated binary image
                FrameLength = (ushort)FixedLength;

                EndianOrder.BigEndian.CopyBytes((uint)TimeTag.Value, buffer, 0);
                EndianOrder.BigEndian.CopyBytes(m_sampleCount, buffer, 4);

                return buffer;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Appends header specific attributes to <paramref name="attributes"/> dictionary.
        /// </summary>
        /// <param name="attributes">Dictionary to append header specific attributes to.</param>
        internal void AppendHeaderAttributes(Dictionary<string, string> attributes)
        {
            attributes.Add("Frame Type", (ushort)TypeID + ": " + TypeID);

            if (FrameImages != null)
            {
                attributes.Add("Frame Length", FrameImages.BinaryLength.ToString());
                attributes.Add("Frame Images", FrameImages.Count.ToString());
            }
            else
            {
                attributes.Add("Frame Length", FrameLength.ToString());
                attributes.Add("Frame Images", "0");
            }
            
            attributes.Add("Frame Count", FrameCount.ToString());
            attributes.Add("Sample Count", m_sampleCount.ToString());
            attributes.Add("Status Flags", m_statusFlags.ToString());
            attributes.Add("Is First Frame", IsFirstFrame.ToString());
            attributes.Add("Is Last Frame", IsLastFrame.ToString());
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
            info.AddValue("sampleCount", m_sampleCount);
            info.AddValue("statusFlags", m_statusFlags);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Validates the CRC-CCITT for the specified IEEE 1344 buffer.
        /// </summary>
        public static bool ChecksumIsValid(byte[] buffer, int startIndex, int length)
        {
            int sumLength = length - 2;
            return EndianOrder.BigEndian.ToUInt16(buffer, startIndex + sumLength) == buffer.CrcCCITTChecksum(startIndex, sumLength);
        }

        #endregion       
    }
}