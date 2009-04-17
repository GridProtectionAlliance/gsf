//*******************************************************************************************************
//  CommandFrame.cs
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
using PCS.IO.Checksums;

namespace PCS.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="ICommandFrame"/> that can be sent or received.
    /// </summary>
    /// <remarks>
    /// IEEE C37.118 command frames are designed only to be sent to a device, not received from a device. As a result
    /// this frame does not implement <see cref="ISupportFrameImage{T}"/> for automated frame parsing. This class
    /// exposes a constructor that accepts a binary image in order to manually parse a command frame.
    /// </remarks>
    [Serializable()]
    public class CommandFrame : CommandFrameBase
    {
        #region [ Members ]

        // Fields
        private CommonFrameHeader m_frameHeader;
        private byte m_version;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from the given <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to parse a received IEEE C37.118 command frame. Typically
        /// command frames are sent to a device. This constructor would used if this code was being used
        /// inside of a phasor measurement device.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is not large enough to parse frame.</exception>
        public CommandFrame(byte[] binaryImage, int startIndex, int length)
            : base(new CommandCellCollection(0), DeviceCommand.ReservedBits)
        {
            if (length < CommonFrameHeader.FixedLength)
                throw new ArgumentOutOfRangeException("length");

            m_frameHeader = new CommonFrameHeader(null, binaryImage, startIndex);

            if (m_frameHeader.TypeID != IeeeC37_118.FrameType.CommandFrame)
                throw new InvalidOperationException("Binary image does not represent an IEEE C37.118 command frame");

            if (length < m_frameHeader.FrameLength)
                throw new ArgumentOutOfRangeException("length");

            Initialize(binaryImage, startIndex, length);
        }

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="CommandFrame"/>.</param>
        /// <param name="command">The <see cref="DeviceCommand"/> for this <see cref="CommandFrame"/>.</param>
        /// <param name="version">IEEE C37.118 revision number.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEEE 1344 command frame.
        /// </remarks>
        public CommandFrame(ushort idCode, DeviceCommand command, byte version)
            : base(new CommandCellCollection(Common.MaximumExtendedDataLength), command)
        {
            base.IDCode = idCode;
            base.Timestamp = DateTime.UtcNow.Ticks;
            m_version = version;
        }

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommandFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize command frame
            m_version = info.GetByte("version");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            get
            {
                // Make sure frame header exists
                if (m_frameHeader == null)
                    m_frameHeader = new CommonFrameHeader(IeeeC37_118.FrameType.CommandFrame, base.Timestamp, m_version);

                return m_frameHeader;
            }
            set
            {
                m_frameHeader = value;

                if (m_frameHeader != null)
                    State = m_frameHeader.State as ICommandFrameParsingState;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="DataFrame"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public override Ticks Timestamp
        {
            get
            {
                return CommonHeader.Timestamp;
            }
            set
            {
                // Keep timestamp updates synchrnonized...
                CommonHeader.Timestamp = value;
                base.Timestamp = value;
            }
        }

        /// <summary>
        /// Gets the IEEE C37.118 protocol version of this <see cref="CommandFrame"/>.
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
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                return CommonFrameHeader.FixedLength;
            }
        }

        /// <summary>
        /// Gets the binary header image of the <see cref="DataFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                // Make sure to provide proper frame length for use in the common header image
                unchecked
                {
                    CommonHeader.FrameLength = (ushort)BinaryLength;
                }

                return CommonHeader.BinaryImage;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                CommonHeader.AppendHeaderAttributes(baseAttributes);

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] binaryImage, int startIndex, int length)
        {
            // We already parsed the frame header, so we just skip past it...
            return CommonFrameHeader.FixedLength;
        }

        /// <summary>
        /// Calculates checksum of given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer image over which to calculate checksum.</param>
        /// <param name="offset">Start index into <paramref name="buffer"/> to calculate checksum.</param>
        /// <param name="length">Length of data within <paramref name="buffer"/> to calculate checksum.</param>
        /// <returns>Checksum over specified portion of <paramref name="buffer"/>.</returns>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            // IEEE C37.118 uses CRC-CCITT to calculate checksum for frames
            return buffer.CrcCCITTChecksum(offset, length);
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize command frame
            info.AddValue("version", m_version);
        }

        #endregion
    }
}