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
//  04/27/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using PCS.IO.Checksums;
using PCS.Parsing;

namespace PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents the SEL Fast Message implementation of a <see cref="ICommandFrame"/> that can be sent or received.
    /// </summary>
    /// <remarks>
    /// SEL Fast Message command frames are designed only to be sent to a device, not received from a device. As a result
    /// this frame does not implement <see cref="ISupportFrameImage{T}"/> for automated frame parsing. This class
    /// exposes a constructor that accepts a binary image in order to manually parse a command frame.
    /// </remarks>
    [Serializable()]
    public class CommandFrame : CommandFrameBase
    {
        #region [ Members ]

        // Fields
        private MessagePeriod m_messagePeriod;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from the given <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to parse a received SEL Fast Message command frame. Typically
        /// command frames are sent to a device. This constructor would used if this code was being used
        /// inside of a phasor measurement device.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is not large enough to parse frame.</exception>
        public CommandFrame(byte[] binaryImage, int startIndex, int length)
            : base(new CommandCellCollection(0), PhasorProtocols.DeviceCommand.ReservedBits)
        {
            if (length < 16)
                throw new ArgumentOutOfRangeException("length");

            // Validate check-sum
            if (!ChecksumIsValid(binaryImage, startIndex))
                throw new InvalidOperationException("Invalid binary image detected - check sum of " + this.GetType().Name + " did not match");
            
            // Validate SEL Fast Message data image
            if (binaryImage[startIndex] != Common.HeaderByte1 || binaryImage[startIndex + 1] != Common.HeaderByte2)
                throw new InvalidOperationException("Bad data stream, expected header bytes 0xA546 as first bytes in SEL Fast Message command frame, got 0x" + binaryImage[startIndex].ToString("X").PadLeft(2, '0') + binaryImage[startIndex + 1].ToString("X").PadLeft(2, '0'));

            Command = (DeviceCommand)binaryImage[startIndex + 9];

            if (Command == DeviceCommand.EnableUnsolicitedMessages)
                m_messagePeriod = (MessagePeriod)EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 14);
            
            // Validate check-sum
            int sumLength = FrameSize - 2;

            if (EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + sumLength) != CalculateChecksum(binaryImage, startIndex, sumLength))
                throw new InvalidOperationException("Invalid binary image detected - check sum of " + this.GetType().Name + " did not match");
        }

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from specified parameters.
        /// </summary>
        /// <param name="command">The <see cref="PhasorProtocols.DeviceCommand"/> for this <see cref="CommandFrame"/>.</param>
        /// <param name="messagePeriod">The desired <see cref="SelFastMessage.MessagePeriod"/> for SEL device connection.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an SEL Fast Message command frame.
        /// </remarks>
        public CommandFrame(PhasorProtocols.DeviceCommand command, MessagePeriod messagePeriod)
            : base(new CommandCellCollection(0), command)
        {
            if (command != PhasorProtocols.DeviceCommand.EnableRealTimeData && command != PhasorProtocols.DeviceCommand.DisableRealTimeData)
                throw new ArgumentException("SEL Fast Message does not support " + command + " device command.", "command");

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
                switch (base.Command)
                {
                    case PhasorProtocols.DeviceCommand.EnableRealTimeData:
                        return DeviceCommand.EnableUnsolicitedMessages;
                    case PhasorProtocols.DeviceCommand.DisableRealTimeData:
                        return DeviceCommand.DisableUnsolicitedMessages;
                    default:
                        return DeviceCommand.Undefined;
                }
            }
            set
            {
                switch (value)
                {
                    case DeviceCommand.EnableUnsolicitedMessages:
                        base.Command = PhasorProtocols.DeviceCommand.EnableRealTimeData;
                        break;
                    case DeviceCommand.DisableUnsolicitedMessages:
                        base.Command = PhasorProtocols.DeviceCommand.DisableRealTimeData;
                        break;
                    default:
                        base.Command = PhasorProtocols.DeviceCommand.ReservedBits;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the desired message period for the SEL device.
        /// </summary>
        public MessagePeriod MessagePeriod
        {
            get
            {
                return m_messagePeriod;
            }
            set
            {
                m_messagePeriod = value;
            }
        }

        /// <summary>
        /// Gets frame size based on <see cref="Command"/>.
        /// </summary>
        public byte FrameSize
        {
            get
            {
                return (byte)(Command == DeviceCommand.EnableUnsolicitedMessages ? 18 : 16);
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                return 9;
            }
        }

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
        protected override int BodyLength
        {
            get
            {
                // Total frame size - header length - crc value length
                return FrameSize - HeaderLength - 2;
            }
        }

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
                    EndianOrder.BigEndian.CopyBytes((ushort)m_messagePeriod, buffer, 5);

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

                baseAttributes.Add("Defined Message Period", (ushort)MessagePeriod + ": " + MessagePeriod);

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
            // SEL Fast Message uses CRC16 to calculate checksum for frames
            return buffer.Crc16Checksum(offset, length);
        }

        /// <summary>
        /// Appends checksum onto <paramref name="buffer"/> starting at <paramref name="startIndex"/>.
        /// </summary>
        /// <param name="buffer">Buffer image on which to append checksum.</param>
        /// <param name="startIndex">Index into <paramref name="buffer"/> where checksum should be appended.</param>
        /// <remarks>
        /// We override default implementation since SEL Fast Message implements check sum for frames in little-endian.
        /// </remarks>
        protected override void AppendChecksum(byte[] buffer, int startIndex)
        {
            EndianOrder.BigEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex);
        }

        /// <summary>
        /// Determines if checksum in the <paramref name="buffer"/> is valid.
        /// </summary>
        /// <param name="buffer">Buffer image to validate.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to perform checksum.</param>
        /// <returns>Flag that determines if checksum over <paramref name="buffer"/> is valid.</returns>
        /// <remarks>
        /// We override default implementation since SEL Fast Message implements check sum for frames in little-endian.
        /// </remarks>
        protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
        {
            int sumLength = BinaryLength - 2;
            return EndianOrder.BigEndian.ToUInt16(buffer, startIndex + sumLength) == CalculateChecksum(buffer, startIndex, sumLength);
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
            info.AddValue("messagePeriod", m_messagePeriod, typeof(MessagePeriod));
        }

        #endregion
    }
}