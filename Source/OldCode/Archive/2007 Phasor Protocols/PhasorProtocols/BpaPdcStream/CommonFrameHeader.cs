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
using TVA.Parsing;

namespace TVA.PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the common header for all BPA PDCstream frames of data.
    /// </summary>
    [Serializable()]
    public class CommonFrameHeader : ICommonHeader<FrameType>, ISerializable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/>.
        /// </summary>
        public const ushort FixedLength = 4;

        // Fields
        private byte m_packetNumber;
        private ushort m_wordCount;
        private Ticks m_roughTimestamp;
        private IChannelParsingState m_state;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from specified parameters.
        /// </summary>
        /// <param name="packetNumber">The BPA PDCstream packet number, configuration frame is packet zero.</param>
        public CommonFrameHeader(byte packetNumber)
        {
            m_packetNumber = packetNumber;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="parseWordCountFromByte">Defines flag that interprets word count in packet header from a byte instead of a word.</param>
        /// <param name="binaryImage">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        /// <param name="length">Maximum length of valid data from start index.</param>
        public CommonFrameHeader(bool parseWordCountFromByte, byte[] binaryImage, int startIndex, int length)
        {
            if (binaryImage[startIndex] != PhasorProtocols.Common.SyncByte)
                throw new InvalidOperationException("Bad data stream, expected sync byte 0xAA as first byte in BPA PDCstream frame, got 0x" + binaryImage[startIndex].ToString("X").PadLeft(2, '0'));

            // Get packet number
            m_packetNumber = binaryImage[startIndex + 1];

            // Some older streams have a bad word count (e.g., some data streams have a 0x01 as the third byte
            // in the stream - this should be a 0x00 to make the word count come out correctly).  The following
            // compensates for this erratic behavior
            if (parseWordCountFromByte)
                m_wordCount = binaryImage[startIndex + 3];
            else
                m_wordCount = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 2);

            // If this is a data frame get a rough timestamp down to the second (full parse will get accurate timestamp), this way
            // data frames that don't get fully parsed because configuration hasn't been received will still show a timestamp
            if (m_packetNumber > 0 && length > 8)
            {
                uint secondOfCentury = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 4);

                // Until configuration is available, we make a guess at time tag type - this will just be
                // used for display purposes until a configuration frame arrives.  If second of century
                // is greater than 3155673600 (SOC value for NTP timestamp 1/1/2007), then this is likely
                // an NTP time stamp (else this is a Unix time tag for the year 2069 - not likely).
                if (secondOfCentury > 3155673600)
                    m_roughTimestamp = (new NtpTimeTag(secondOfCentury, 0)).ToDateTime().Ticks;
                else
                    m_roughTimestamp = (new UnixTimeTag(secondOfCentury)).ToDateTime().Ticks;
            }
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommonFrameHeader(SerializationInfo info, StreamingContext context)
        {
            // Deserialize common frame header
            m_packetNumber = info.GetByte("packetNumber");
            m_wordCount = info.GetUInt16("wordCount");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the BPA PDCstream specific frame type of this frame.
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
                return (m_packetNumber == 0 ? BpaPdcStream.FrameType.ConfigurationFrame : BpaPdcStream.FrameType.DataFrame);
            }
        }

        /// <summary>
        /// Gets or sets the BPA PDCstream packet number of this frame - set to 00 for configuration frame
        /// </summary>
        public byte PacketNumber
        {
            get
            {
                return m_packetNumber;
            }
            set
            {
                m_packetNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the BPA PDCstream frame length of this frame.
        /// </summary>
        public ushort FrameLength
        {
            get
            {
                return (ushort)(2 * m_wordCount);
            }
            set
            {
                m_wordCount = (ushort)(value / 2);
            }
        }

        /// <summary>
        /// Gets or sets the BPA PDcstream word count.
        /// </summary>
        public ushort WordCount
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

        /// <summary>
        /// Gets or sets the length of the data in the BPA PDCstream frame (i.e., the <see cref="FrameLength"/> minus the header length and checksum: <see cref="FrameLength"/> - 8).
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
        /// Gets rough timestamp, accuarate to the second, that can be used until configuration frame arrives.
        /// </summary>
        public Ticks RoughTimestamp
        {
            get
            {
                return m_roughTimestamp;
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
                // Translate BPA PDCstream specific frame type to fundamental frame type
                switch (TypeID)
                {
                    case BpaPdcStream.FrameType.DataFrame:
                        return FundamentalFrameType.DataFrame;
                    case BpaPdcStream.FrameType.ConfigurationFrame:
                        return FundamentalFrameType.ConfigurationFrame;
                    default:
                        return FundamentalFrameType.Undetermined;
                }
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
                buffer[1] = m_packetNumber;
                EndianOrder.BigEndian.CopyBytes(m_wordCount, buffer, 2);

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
            attributes.Add("Frame Length", FrameLength.ToString());
            attributes.Add("Packet Number", PacketNumber.ToString());
            attributes.Add("Word Count", WordCount.ToString());
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
            info.AddValue("packetNumber", m_packetNumber);
            info.AddValue("wordCount", m_wordCount);
        }

        #endregion
    }
}