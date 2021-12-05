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
//  04/27/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSF.IO.Checksums;
using GSF.Parsing;

namespace GSF.PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents the SEL Fast Message implementation of a <see cref="ICommandFrame"/> that can be sent or received.
    /// </summary>
    /// <remarks>
    /// SEL Fast Message command frames are designed only to be sent to a device, not received from a device. As a result
    /// this frame does not implement <see cref="ISupportFrameImage{TTypeIdentifier}"/> for automated frame parsing. This class
    /// exposes a constructor that accepts a binary image in order to manually parse a command frame.
    /// </remarks>
    [Serializable]
    public class CommandFrame : CommandFrameBase
    {
        #region [ Members ]

        // Fields
        private MessagePeriod m_messagePeriod;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to parse a received SEL Fast Message command frame. Typically
        /// command frames are sent to a device. This constructor would used if this code was being used
        /// inside of a phasor measurement device.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is not large enough to parse frame.</exception>
        public CommandFrame(byte[] buffer, int startIndex, int length)
            : base(new CommandCellCollection(0), PhasorProtocols.DeviceCommand.ReservedBits)
        {
            if (length < 16)
                throw new ArgumentOutOfRangeException(nameof(length));

            // Validate check-sum
            if (!ChecksumIsValid(buffer, startIndex))
                throw new InvalidOperationException($"Invalid binary image detected - check sum of {GetType().Name} did not match");

            // Validate SEL Fast Message data image
            if (buffer[startIndex] != Common.HeaderByte1 || buffer[startIndex + 1] != Common.HeaderByte2)
                throw new InvalidOperationException($"Bad data stream, expected header bytes 0xA546 as first bytes in SEL Fast Message command frame, got 0x{buffer[startIndex].ToString("X").PadLeft(2, '0')}{buffer[startIndex + 1].ToString("X").PadLeft(2, '0')}");

            Command = (DeviceCommand)buffer[startIndex + 9];

            if (Command == DeviceCommand.EnableUnsolicitedMessages)
                m_messagePeriod = (MessagePeriod)BigEndian.ToUInt16(buffer, startIndex + 14);

            // Validate check-sum
            int sumLength = FrameSize - 2;

            if (BigEndian.ToUInt16(buffer, startIndex + sumLength) != CalculateChecksum(buffer, startIndex, sumLength))
                throw new InvalidOperationException($"Invalid binary image detected - check sum of {GetType().Name} did not match");
        }

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from specified parameters.
        /// </summary>
        /// <param name="command">The <see cref="GSF.PhasorProtocols.DeviceCommand"/> for this <see cref="CommandFrame"/>.</param>
        /// <param name="messagePeriod">The desired <see cref="SelFastMessage.MessagePeriod"/> for SEL device connection.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an SEL Fast Message command frame.
        /// </remarks>
        public CommandFrame(PhasorProtocols.DeviceCommand command, MessagePeriod messagePeriod)
            : base(new CommandCellCollection(0), command)
        {
            if (command != PhasorProtocols.DeviceCommand.EnableRealTimeData && command != PhasorProtocols.DeviceCommand.DisableRealTimeData)
                throw new ArgumentException($"SEL Fast Message does not support {command} device command.", nameof(command));

            m_messagePeriod = messagePeriod;
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
            m_messagePeriod = (MessagePeriod)info.GetValue("messagePeriod", typeof(MessagePeriod));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="DeviceCommand"/> for this <see cref="CommandFrame"/>.
        /// </summary>
        public new DeviceCommand Command
        {
            // Interpret SEL specific device command
            get
            {
                return base.Command switch
                {
                    PhasorProtocols.DeviceCommand.EnableRealTimeData => DeviceCommand.EnableUnsolicitedMessages,
                    PhasorProtocols.DeviceCommand.DisableRealTimeData => DeviceCommand.DisableUnsolicitedMessages,
                    _ => DeviceCommand.Undefined,
                };
            }
            set
            {
                base.Command = value switch
                {
                    DeviceCommand.EnableUnsolicitedMessages => PhasorProtocols.DeviceCommand.EnableRealTimeData,
                    DeviceCommand.DisableUnsolicitedMessages => PhasorProtocols.DeviceCommand.DisableRealTimeData,
                    _ => PhasorProtocols.DeviceCommand.ReservedBits,
                };
            }
        }

        /// <summary>
        /// Gets or sets the desired message period for the SEL device.
        /// </summary>
        public MessagePeriod MessagePeriod
        {
            get => m_messagePeriod;
            set => m_messagePeriod = value;
        }

        /// <summary>
        /// Gets frame size based on <see cref="Command"/>.
        /// </summary>
        public byte FrameSize => (byte)(Command == DeviceCommand.EnableUnsolicitedMessages ? 18 : 16);

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => 9;

        /// <summary>
        /// Gets the binary header image of the <see cref="DataFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[9];

                buffer[0] = Common.HeaderByte1;
                buffer[1] = Common.HeaderByte2;
                buffer[2] = FrameSize;

                return buffer;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength =>            
            FrameSize - HeaderLength - 2; // Total frame size - header length - CRC value length

        /// <summary>
        /// Gets the binary body image of this <see cref="CommandFrame"/>.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];

                buffer[0] = (byte)Command;
                buffer[1] = 0xC0;
                buffer[3] = 0x20;

                // Only add desired message rate for enable command
                if (Command == DeviceCommand.EnableUnsolicitedMessages)
                    BigEndian.CopyBytes((ushort)m_messagePeriod, buffer, 5);

                return buffer;
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

                baseAttributes.Add("Defined Message Period", $"{(ushort)MessagePeriod}: {MessagePeriod}");

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Calculates checksum of given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer image over which to calculate checksum.</param>
        /// <param name="offset">Start index into <paramref name="buffer"/> to calculate checksum.</param>
        /// <param name="length">Length of data within <paramref name="buffer"/> to calculate checksum.</param>
        /// <returns>Checksum over specified portion of <paramref name="buffer"/>.</returns>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            // SEL Fast Message uses CRC Modbus to calculate checksum for frames
            return buffer.ModBusCrcChecksum(offset, length);
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
            info.AddValue("messagePeriod", m_messagePeriod, typeof(MessagePeriod));
        }

        #endregion
    }
}