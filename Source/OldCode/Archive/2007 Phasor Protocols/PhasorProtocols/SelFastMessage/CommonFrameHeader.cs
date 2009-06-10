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
//  04/26/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using PCS.Parsing;

namespace PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents the common header for all SEL Fast Message frames of data.
    /// </summary>
    [Serializable()]
    public class CommonFrameHeader : CommonHeaderBase<int>, ISerializable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/>.
        /// </summary>
        public const ushort FixedLength = 24;
        
        /// <summary>
        /// Total header length of <see cref="CommonFrameHeader"/>.
        /// </summary>
        public const ushort HeaderLength = 18;

        // Fields
        private FrameSize m_frameSize;
        private uint m_idCode;
        private Ticks m_timestamp;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from specified parameters.
        /// </summary>
        /// <param name="frameSize">The <see cref="SelFastMessage.FrameSize"/> of this frame.</param>
        /// <param name="timestamp">The timestamp of this frame.</param>
        public CommonFrameHeader(FrameSize frameSize, Ticks timestamp)
        {
            m_frameSize = frameSize;
            m_timestamp = timestamp;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        public CommonFrameHeader(byte[] binaryImage, int startIndex)
        {
            // Validate SEL Fast Message data image
            if (binaryImage[startIndex] != Common.HeaderByte1 || binaryImage[startIndex + 1] != Common.HeaderByte2)
                throw new InvalidOperationException("Bad data stream, expected header bytes 0xA546 as first bytes in SEL Fast Message frame, got 0x" + binaryImage[startIndex].ToString("X").PadLeft(2, '0') + binaryImage[startIndex + 1].ToString("X").PadLeft(2, '0'));

            ushort sampleCount;
            uint secondOfCentury;
            NtpTimeTag timetag;

            // Parse relevant common header values
            m_frameSize = (FrameSize)binaryImage[startIndex + 2];
            m_idCode = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 12);
            sampleCount = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 18);
            secondOfCentury = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 20);
            
            // We use an NTP time tag since SEL Fast Message SOC also starts at 1/1/1900
            timetag = new NtpTimeTag(secondOfCentury, 0);

            // Data frames have subsecond time information, so we add this fraction of time to current seconds value
            timetag.Value += sampleCount * 50.0D / 1000.0D;

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
            m_frameSize = (FrameSize)info.GetValue("frameSize", typeof(FrameSize));
            m_idCode = info.GetUInt32("idCode32Bit");
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
        /// Gets or sets the SEL Fast Message destination address (PMADDR setting) of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public uint IDCode
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
        /// Gets the SEL Fast Message second of century of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public uint SecondOfCentury
        {
            get
            {
                return (uint)Math.Truncate(TimeTag.Value);
            }
        }

        /// <summary>
        /// Gets the SEL Fast Message sample number of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public ushort SampleNumber
        {
            get
            {
                return (ushort)(m_timestamp.DistanceBeyondSecond().ToMilliseconds() / 50);
            }
        }

        /// <summary>
        /// Gets or sets the SEL Fast Message frame size of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public FrameSize FrameSize
        {
            get
            {
                return m_frameSize;
            }
            set
            {
                m_frameSize = value;
            }
        }

        /// <summary>
        /// Gets the SEL Fast Message register count of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public ushort RegisterCount
        {
            get
            {
                return (ushort)(DataLength / 2);
            }
        }

        /// <summary>
        /// Gets the length of the data in the SEL Fast Message frame (i.e., the <see cref="FrameSize"/> minus the <see cref="HeaderLength"/> and checksum) of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public ushort DataLength
        {
            get
            {
                // Data length will be frame length minus common header length minus crc16
                return (ushort)((ushort)m_frameSize - HeaderLength - 2);
            }
        }


        /// <summary>
        /// Gets the binary image of the common header portion of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] buffer = new byte[FixedLength];

                buffer[0] = Common.HeaderByte1;
                buffer[1] = Common.HeaderByte2;
                buffer[2] = (byte)m_frameSize;
                buffer[9] = 0x20;   // Function code
                buffer[10] = 0xC0;  // Sequence byte

                // Include destination address (PMADDR setting)
                EndianOrder.BigEndian.CopyBytes(m_idCode, buffer, 12);

                // Include register count
                EndianOrder.BigEndian.CopyBytes(RegisterCount, buffer, 16);

                // Include sample number
                EndianOrder.BigEndian.CopyBytes(SampleNumber, buffer, 18);

                // Include second of century
                EndianOrder.BigEndian.CopyBytes(SecondOfCentury, buffer, 20);

                return buffer;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize unique common frame header values
            info.AddValue("frameSize", m_frameSize, typeof(FrameSize));
            info.AddValue("idCode32Bit", m_idCode);
        }

        #endregion
    }
}