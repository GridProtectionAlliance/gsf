//******************************************************************************************************
//  CommandFrame.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/19/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSF.IO.Checksums;
using GSF.Parsing;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.IEC61850_90_5
{
    /// <summary>
    /// Represents the IEC 61850-90-5 implementation of a <see cref="ICommandFrame"/> that can be sent or received.
    /// </summary>
    /// <remarks>
    /// IEC 61850-90-5 command frames are designed only to be sent to a device, not received from a device. As a result
    /// this frame does not implement <see cref="ISupportFrameImage{TTypeIdentifier}"/> for automated frame parsing. This class
    /// exposes a constructor that accepts a binary image in order to manually parse a command frame.
    /// </remarks>
    [Serializable]
    public class CommandFrame : CommandFrameBase
    {
        #region [ Members ]

        // Fields
        private CommonFrameHeader m_frameHeader;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to parse a received IEC 61850-90-5 command frame. Typically
        /// command frames are sent to a device. This constructor would used if this code was being used
        /// inside of a phasor measurement device.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is not large enough to parse frame.</exception>
        public CommandFrame(byte[] buffer, int startIndex, int length)
            : base(new CommandCellCollection(Common.MaximumExtendedDataLength), DeviceCommand.ReservedBits)
        {
            if (length < CommonFrameHeader.FixedLength)
                throw new ArgumentOutOfRangeException(nameof(length));

            m_frameHeader = new CommonFrameHeader(null, false, false, false, true, true, AngleFormat.Degrees, buffer, startIndex, 0);

            if (m_frameHeader.TypeID != IEC61850_90_5.FrameType.CommandFrame)
                throw new InvalidOperationException("Binary image does not represent an IEC 61850-90-5 command frame");

            if (length < m_frameHeader.FrameLength)
                throw new ArgumentOutOfRangeException(nameof(length), $"Buffer size, {length}, is not large enough to parse IEC 61850-90-5 command frame with a length of {m_frameHeader.FrameLength}");

            // Validate check-sum
            int sumLength = m_frameHeader.FrameLength - 2;

            if (BigEndian.ToUInt16(buffer, startIndex + sumLength) != CalculateChecksum(buffer, startIndex, sumLength))
                throw new InvalidOperationException($"Invalid binary image detected - check sum of {GetType().Name} did not match");

            m_frameHeader.State = new CommandFrameParsingState(m_frameHeader.FrameLength, m_frameHeader.DataLength, true, true);
            CommonHeader = m_frameHeader;
            ParseBinaryImage(buffer, startIndex, length);
        }

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="CommandFrame"/>.</param>
        /// <param name="command">The <see cref="DeviceCommand"/> for this <see cref="CommandFrame"/>.</param>
        /// <param name="version">IEC 61850-90-5 revision number.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEC 61850-90-5 command frame.
        /// </remarks>
        public CommandFrame(ushort idCode, DeviceCommand command, byte version)
            : base(new CommandCellCollection(Common.MaximumExtendedDataLength), command)
        {
            base.IDCode = idCode;
            base.Timestamp = DateTime.UtcNow.Ticks;
            Version = version;
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
            m_frameHeader = (CommonFrameHeader)info.GetValue("frameHeader", typeof(CommonFrameHeader));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            // Make sure frame header exists
            get => m_frameHeader ??= new CommonFrameHeader(null, IEC61850_90_5.FrameType.CommandFrame, base.IDCode, base.Timestamp);
            set
            {
                m_frameHeader = value;

                if (m_frameHeader is null)
                    return;

                State = m_frameHeader.State as ICommandFrameParsingState;
                base.IDCode = m_frameHeader.IDCode;
                base.Timestamp = m_frameHeader.Timestamp;
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
            get => CommonHeader.Timestamp;
            set
            {
                // Keep timestamp updates synchronized...
                CommonHeader.Timestamp = value;
                base.Timestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets the ID code.
        /// </summary>
        public override ushort IDCode
        {
            get => CommonHeader.IDCode;
            set
            {
                // Keep ID code updates synchronized...
                CommonHeader.IDCode = value;
                base.IDCode = value;
            }
        }

        /// <summary>
        /// Gets the IEC 61850-90-5 protocol version of this <see cref="CommandFrame"/>.
        /// </summary>
        public byte Version
        {
            get => CommonHeader.Version;
            set => CommonHeader.Version = value;
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => CommonFrameHeader.FixedLength;

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
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] buffer, int startIndex, int length)
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
            // IEC 61850-90-5 uses CRC-CCITT to calculate checksum for frames
            return buffer.CrcCCITTChecksum(offset, length);
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize command frame
            info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
        }

        #endregion
    }
}