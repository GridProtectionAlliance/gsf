//******************************************************************************************************
//  CommonFrameHeader.cs - Gbtc
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
    /// Represents the common header for all Macrodyne frames of data.
    /// </summary>
    [Serializable]
    public class CommonFrameHeader : CommonHeaderBase<FrameType>, ISerializable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/>.
        /// </summary>
        public const ushort FixedLength = 2;

        // Fields
        private ProtocolVersion m_protocolVersion;
        private StatusFlags m_statusFlags;
        private readonly ConfigurationFrame m_configurationFrame;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader()
        {
            m_protocolVersion = ProtocolVersion.M;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/>.
        /// </summary>
        /// <param name="protocolVersion">Macrodyne protocol to use for parsing.</param>
        /// <param name="typeID">Frame type common header represents.</param>
        public CommonFrameHeader(ProtocolVersion protocolVersion, FrameType typeID)
        {
            m_protocolVersion = protocolVersion;
            TypeID = typeID;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        /// <param name="protocolVersion">Macrodyne protocol to use for parsing.</param>
        /// <param name="configurationFrame">Configuration frame, if available.</param>
        public CommonFrameHeader(byte[] buffer, int startIndex, ProtocolVersion protocolVersion, ConfigurationFrame configurationFrame)
        {
            byte firstByte = buffer[startIndex];

            // Cache protocol version
            m_protocolVersion = protocolVersion;

            // Validate Macrodyne data image
            if (firstByte != 0xAA && firstByte != 0xBB)
                throw new InvalidOperationException($"Bad data stream, expected 0xAA or 0xBB as first byte in Macrodyne ON-LINE data frame or configuration frame request response, got 0x{buffer[startIndex].ToString("X").PadLeft(2, '0')}");

            // Determine frame type (it's either the sync byte of a data frame or the response by from a command request)
            if (firstByte == 0xAA)
            {
                TypeID = Macrodyne.FrameType.DataFrame;
            }
            else
            {
                switch (BigEndian.ToUInt16(buffer, startIndex))
                {
                    case (ushort)DeviceCommand.RequestOnlineDataFormat:
                        TypeID = Macrodyne.FrameType.ConfigurationFrame;
                        break;
                    case (ushort)DeviceCommand.RequestUnitIDBufferValue:
                        TypeID = Macrodyne.FrameType.HeaderFrame;
                        break;
                    default:
                        throw new InvalidOperationException($"Bad data stream, expected 0xBB24 or 0xBB48 in response to Macrodyne device command, got 0xBB{buffer[startIndex + 1].ToString("X").PadLeft(2, '0')}");
                }
            }

            // Cache config frame, if defined, for future use
            m_configurationFrame = configurationFrame;

            // Parse relevant common header values
            if (TypeID == Macrodyne.FrameType.DataFrame)
            {
                switch (m_protocolVersion)
                {
                    case ProtocolVersion.M:
                        m_statusFlags = (StatusFlags)buffer[startIndex + 1];
                        break;
                    case ProtocolVersion.G:
                        if (buffer[startIndex + 1] != 0x02)
                            throw new InvalidOperationException($"Bad data stream, expected 0xAA02 for Macrodyne 1690G version devices, got 0xAA{buffer[startIndex + 1].ToString("X").PadLeft(2, '0')}");
                        break;
                }
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
            m_statusFlags = (StatusFlags)info.GetValue("statusFlags", typeof(StatusFlags));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines the Macrodyne protocol version.
        /// </summary>
        public ProtocolVersion ProtocolVersion
        {
            get => m_protocolVersion;
            set => m_protocolVersion = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Macrodyne.StatusFlags"/> of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        /// <remarks>
        /// Status flags are only relevant for 1690M implementations and are not provided for 1690G implementations.
        /// </remarks>
        public StatusFlags StatusFlags
        {
            get => m_statusFlags;
            set => m_statusFlags = value;
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
                // Translate Macrodyne specific frame type to fundamental frame type
                switch (TypeID)
                {
                    case Macrodyne.FrameType.DataFrame:
                        return FundamentalFrameType.DataFrame;
                    case Macrodyne.FrameType.ConfigurationFrame:
                        return FundamentalFrameType.ConfigurationFrame;
                    case Macrodyne.FrameType.HeaderFrame:
                        return FundamentalFrameType.HeaderFrame;
                }

                return FundamentalFrameType.Undetermined;
            }
        }

        /// <summary>
        /// Gets the Macrodyne data length.
        /// </summary>
        public ushort DataLength => (ushort)(FrameLength - FixedLength - 1);

        /// <summary>
        /// Gets the Macrodyne frame length.
        /// </summary>
        public ushort FrameLength
        {
            get
            {
                switch (TypeID)
                {
                    case Macrodyne.FrameType.DataFrame:
                        if (!(m_configurationFrame is null))
                            return m_configurationFrame.DataFrameLength;
                        break;
                    case Macrodyne.FrameType.ConfigurationFrame:
                        return FixedLength + 3;
                    case Macrodyne.FrameType.HeaderFrame:
                        return FixedLength + 9;
                }

                return FixedLength;
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

                switch (TypeID)
                {
                    case Macrodyne.FrameType.DataFrame:
                        buffer[0] = PhasorProtocols.Common.SyncByte;
                        buffer[1] = (byte)m_statusFlags;
                        break;
                    case Macrodyne.FrameType.ConfigurationFrame:
                        BigEndian.CopyBytes((ushort)DeviceCommand.RequestOnlineDataFormat, buffer, 0);
                        break;
                    case Macrodyne.FrameType.HeaderFrame:
                        BigEndian.CopyBytes((ushort)DeviceCommand.RequestUnitIDBufferValue, buffer, 0);
                        break;
                }

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
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize unique common frame header values
            info.AddValue("statusFlags", m_statusFlags, typeof(StatusFlags));
        }

        #endregion
    }
}