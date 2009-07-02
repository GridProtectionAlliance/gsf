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
//  04/30/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using TVA.IO.Checksums;
using TVA.Parsing;

namespace TVA.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the Macrodyne implementation of a <see cref="ICommandFrame"/> that can be sent or received.
    /// </summary>
    /// <remarks>
    /// Macrodyne command frames are designed only to be sent to a device, not received from a device. As a result
    /// this frame does not implement <see cref="ISupportFrameImage{T}"/> for automated frame parsing.
    /// </remarks>
    [Serializable()]
    public class CommandFrame : CommandFrameBase
    {
        #region [ Members ]

        // Fields

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from specified parameters.
        /// </summary>
        /// <param name="command">The <see cref="PhasorProtocols.DeviceCommand"/> for this <see cref="CommandFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an Macrodyne command frame.
        /// </remarks>
        public CommandFrame(PhasorProtocols.DeviceCommand command)
            : base(new CommandCellCollection(0), command)
        {
            if (command != PhasorProtocols.DeviceCommand.EnableRealTimeData && command != PhasorProtocols.DeviceCommand.DisableRealTimeData)
                throw new ArgumentException("Macrodyne does not support " + command + " device command.", "command");
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
            //m_messagePeriod = (MessagePeriod)info.GetValue("messagePeriod", typeof(MessagePeriod));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="DeviceCommand"/> for this <see cref="CommandFrame"/>.
        /// </summary>
        public new DeviceCommand Command
        {
            // Interpret Macrodyne specific device command
            get
            {
                switch (base.Command)
                {
                    case PhasorProtocols.DeviceCommand.EnableRealTimeData:
                        return DeviceCommand.StartOnlineData;
                    case PhasorProtocols.DeviceCommand.DisableRealTimeData:
                        return DeviceCommand.StopOnlineData;
                    case PhasorProtocols.DeviceCommand.SendConfigurationFrame1:
                    case PhasorProtocols.DeviceCommand.SendConfigurationFrame2:
                        return DeviceCommand.RequestOnlineDataFormat;
                    default:
                        return DeviceCommand.Undefined;
                }
            }
            set
            {
                switch (value)
                {
                    case DeviceCommand.StartOnlineData:
                        base.Command = PhasorProtocols.DeviceCommand.EnableRealTimeData;
                        break;
                    case DeviceCommand.StopOnlineData:
                        base.Command = PhasorProtocols.DeviceCommand.DisableRealTimeData;
                        break;
                    case DeviceCommand.RequestOnlineDataFormat:
                        base.Command = PhasorProtocols.DeviceCommand.SendConfigurationFrame2;
                        break;
                    default:
                        base.Command = PhasorProtocols.DeviceCommand.ReservedBits;
                        break;
                }
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

                //buffer[0] = Common.HeaderByte1;
                //buffer[1] = Common.HeaderByte2;
                //buffer[2] = FrameSize;

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
                return 0; //FrameSize - HeaderLength - 2;
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
                //if (Command == DeviceCommand.EnableUnsolicitedMessages)
                //    EndianOrder.BigEndian.CopyBytes((ushort)m_messagePeriod, buffer, 5);

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

                //baseAttributes.Add("Defined Message Period", (ushort)MessagePeriod + ": " + MessagePeriod);

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
            // Macrodyne uses 8-bit Xor checksum for frames
            return buffer.Xor8CheckSum(offset, length);
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
            //info.AddValue("messagePeriod", m_messagePeriod, typeof(MessagePeriod));
        }

        #endregion
    }
}