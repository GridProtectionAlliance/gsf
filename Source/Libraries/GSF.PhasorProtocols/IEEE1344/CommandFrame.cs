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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       MOdified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using GSF.IO.Checksums;
using GSF.Parsing;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.IEEE1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="ICommandFrame"/> that can be sent or received.
    /// </summary>
    /// <remarks>
    /// IEEE 1344 command frames are designed only to be sent to a device, not received from a device. As a result
    /// this frame does not implement <see cref="ISupportFrameImage{TTypeIdentifier}"/> for automated frame parsing. This class
    /// exposes a constructor that accepts a binary image in order to manually parse a command frame.
    /// </remarks>
    [Serializable]
    public class CommandFrame : CommandFrameBase
    {
        #region [ Members ]

        /// <summary>
        /// Total frame length of a IEEE 1344 <see cref="CommandFrame"/>.
        /// </summary>
        public const ushort FrameLength = 16;

        // Fields
        private ulong m_idCode;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to parse a received IEEE 1344 command frame. Typically
        /// command frames are sent to a device. This constructor would used if this code was being used
        /// inside of a phasor measurement device.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> must be at least 16.</exception>
        public CommandFrame(byte[] buffer, int startIndex, int length)
            : base(new CommandCellCollection(0), DeviceCommand.ReservedBits)
        {
            if (length < FrameLength)
                throw new ArgumentOutOfRangeException(nameof(length));

            // Validate check-sum
            const int sumLength = FrameLength - 2;

            if (BigEndian.ToUInt16(buffer, startIndex + sumLength) != CalculateChecksum(buffer, startIndex, sumLength))
                throw new InvalidOperationException($"Invalid binary image detected - check sum of {GetType().Name} did not match");

            ParseBinaryImage(buffer, startIndex, length);
        }

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="CommandFrame"/>.</param>
        /// <param name="command">The <see cref="DeviceCommand"/> for this <see cref="CommandFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEEE 1344 command frame.
        /// </remarks>
        public CommandFrame(ulong idCode, DeviceCommand command)
            : base(new CommandCellCollection(0), command)
        {
            IDCode = idCode;
            Timestamp = DateTime.UtcNow.Ticks;
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
            IDCode = info.GetUInt64("idCode64Bit");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="CommandCellCollection"/> for this <see cref="CommandFrame"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // IEEE 1344 command frame doesn't support extended data - so we hide cell collection property...
        public override CommandCellCollection Cells => base.Cells;

        /// <summary>
        /// Gets or sets extended binary image data for this <see cref="CommandFrame"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // IEEE 1344 command frame doesn't support extended data - so we hide extended data property...
        public override byte[] ExtendedData
        {
            get => base.ExtendedData;
            set => base.ExtendedData = value;
        }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="CommandFrame"/>.
        /// </summary>
        public new ulong IDCode
        {
            get => m_idCode;
            set
            {
                m_idCode = value;

                // Base classes constrain maximum value to 65535
                base.IDCode = m_idCode > ushort.MaxValue ? ushort.MaxValue : (ushort)value;
            }
        }

        /// <summary>
        /// Gets NTP based time representation of the ticks of this <see cref="CommandFrame"/>.
        /// </summary>
        public new NtpTimeTag TimeTag => new(Timestamp);

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => 12;

        /// <summary>
        /// Gets the binary header image of the <see cref="CommandFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];

                BigEndian.CopyBytes((uint)TimeTag.Value, buffer, 0);
                BigEndian.CopyBytes(m_idCode, buffer, 4);

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="CommandFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("64-Bit ID Code", IDCode.ToString());

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
            Timestamp = new NtpTimeTag(BigEndian.ToUInt32(buffer, startIndex), 0).ToDateTime().Ticks;
            m_idCode = BigEndian.ToUInt64(buffer, startIndex + 4);
            return 12;
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
            // IEEE 1344 uses CRC-CCITT to calculate checksum for frames
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
            info.AddValue("idCode64Bit", m_idCode);
        }

        #endregion
    }
}