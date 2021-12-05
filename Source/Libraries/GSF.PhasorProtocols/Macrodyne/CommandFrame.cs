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
//  04/30/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using GSF.Parsing;

namespace GSF.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the Macrodyne implementation of a <see cref="ICommandFrame"/> that can be sent or received.
    /// </summary>
    /// <remarks>
    /// Macrodyne command frames are designed only to be sent to a device, not received from a device. As a result
    /// this frame does not implement <see cref="ISupportFrameImage{TTypeIdentifier}"/> for automated frame parsing.
    /// </remarks>
    [Serializable]
    public class CommandFrame : CommandFrameBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from specified parameters.
        /// </summary>
        /// <param name="command">The <see cref="GSF.PhasorProtocols.DeviceCommand"/> for this <see cref="CommandFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an Macrodyne command frame.
        /// </remarks>
        public CommandFrame(PhasorProtocols.DeviceCommand command)
            : base(new CommandCellCollection(0), command)
        {
            if (command != PhasorProtocols.DeviceCommand.EnableRealTimeData && command != PhasorProtocols.DeviceCommand.DisableRealTimeData && command != PhasorProtocols.DeviceCommand.SendConfigurationFrame1 && command != PhasorProtocols.DeviceCommand.SendConfigurationFrame2 && command != PhasorProtocols.DeviceCommand.SendHeaderFrame)
                throw new ArgumentException($"Macrodyne does not support {command} device command.", nameof(command));
        }

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommandFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
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
                return base.Command switch
                {
                    PhasorProtocols.DeviceCommand.EnableRealTimeData => DeviceCommand.StartOnlineData,
                    PhasorProtocols.DeviceCommand.DisableRealTimeData => DeviceCommand.StopOnlineData,
                    PhasorProtocols.DeviceCommand.SendConfigurationFrame1 or PhasorProtocols.DeviceCommand.SendConfigurationFrame2 => DeviceCommand.RequestOnlineDataFormat,
                    PhasorProtocols.DeviceCommand.SendHeaderFrame => DeviceCommand.RequestUnitIDBufferValue,
                    _ => DeviceCommand.Undefined,
                };
            }
            set
            {
                base.Command = value switch
                {
                    DeviceCommand.StartOnlineData => PhasorProtocols.DeviceCommand.EnableRealTimeData,
                    DeviceCommand.StopOnlineData => PhasorProtocols.DeviceCommand.DisableRealTimeData,
                    DeviceCommand.RequestOnlineDataFormat => PhasorProtocols.DeviceCommand.SendConfigurationFrame2,
                    DeviceCommand.RequestUnitIDBufferValue => PhasorProtocols.DeviceCommand.SendHeaderFrame,
                    _ => PhasorProtocols.DeviceCommand.ReservedBits,
                };
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public override int BinaryLength =>
            // Total frame size
            2;

        /// <summary>
        /// Gets the binary body image of this <see cref="CommandFrame"/>.
        /// </summary>
        public override byte[] BinaryImage
        {
            get
            {
                byte[] buffer = new byte[BinaryLength];

                BigEndian.CopyBytes((ushort)Command, buffer, 0);

                return buffer;
            }
        }

        /// <summary>
        /// Calculates checksum of given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer image over which to calculate checksum.</param>
        /// <param name="offset">Start index into <paramref name="buffer"/> to calculate checksum.</param>
        /// <param name="length">Length of data within <paramref name="buffer"/> to calculate checksum.</param>
        /// <returns>Checksum over specified portion of <paramref name="buffer"/>.</returns>
        /// <remarks>
        /// The Macrodyne does not use checksums for sending commands.
        /// </remarks>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}