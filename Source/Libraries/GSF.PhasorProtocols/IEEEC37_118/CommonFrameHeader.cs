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
//  11/12/2004 - J. Ritchie Carroll
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
using GSF.Parsing;

namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the common header for all IEEE C37.118 frames of data.
    /// </summary>
    [Serializable]
    public class CommonFrameHeader : ICommonHeader<FrameType>, ISerializable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/>.
        /// </summary>
        public const ushort FixedLength = 14;

        // Fields
        private FrameType m_frameType;
        private byte m_version;
        private ushort m_frameLength;
        private ushort m_idCode;
        private Ticks m_timestamp;
        private uint m_timebase;
        private uint m_timeQualityFlags;
        private readonly int m_framesPerSecond;
        private readonly double m_ticksPerFrame;
        private IChannelParsingState m_state;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from specified parameters.
        /// </summary>
        /// <param name="configurationFrame">IEEE C37.118 <see cref="ConfigurationFrame1"/> if available.</param>
        /// <param name="typeID">The IEEE C37.118 specific frame type of this frame.</param>
        /// <param name="idCode">The ID code of this frame.</param>
        /// <param name="timestamp">The timestamp of this frame.</param>
        public CommonFrameHeader(ConfigurationFrame1 configurationFrame, FrameType typeID, ushort idCode, Ticks timestamp)
        {
            m_frameType = typeID;
            m_idCode = idCode;
            m_timestamp = timestamp;
            m_version = 1;
            m_timebase = (UInt24)100000;

            if (configurationFrame is null)
                return;

            // Hang on to configured frame rate and ticks per frame
            m_framesPerSecond = configurationFrame.FrameRate;
            m_ticksPerFrame = Ticks.PerSecond / (double)m_framesPerSecond;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="configurationFrame">IEEE C37.118 <see cref="ConfigurationFrame1"/> if already parsed.</param>
        /// <param name="buffer">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        public CommonFrameHeader(ConfigurationFrame1 configurationFrame, byte[] buffer, int startIndex)
        {
            const byte VersionNumberMask = (byte)IEEEC37_118.FrameType.VersionNumberMask;

            if (buffer[startIndex] != PhasorProtocols.Common.SyncByte)
                throw new InvalidOperationException($"Bad data stream, expected sync byte 0xAA as first byte in IEEE C37.118 frame, got 0x{buffer[startIndex].ToString("X").PadLeft(2, '0')}");

            // Strip out frame type and version information...
            m_frameType = (FrameType)(buffer[startIndex + 1] & ~VersionNumberMask);
            m_version = (byte)(buffer[startIndex + 1] & VersionNumberMask);

            m_frameLength = BigEndian.ToUInt16(buffer, startIndex + 2);
            m_idCode = BigEndian.ToUInt16(buffer, startIndex + 4);

            uint secondOfCentury = BigEndian.ToUInt32(buffer, startIndex + 6);
            uint fractionOfSecond = BigEndian.ToUInt32(buffer, startIndex + 10);

            // Without timebase, the best timestamp you can get is down to the whole second
            m_timestamp = new UnixTimeTag((decimal)secondOfCentury).ToDateTime().Ticks;

            if (!(configurationFrame is null))
            {
                // If config frame is available, frames have enough information for sub-second time resolution
                m_timebase = configurationFrame.Timebase;

                // "Actual fractional seconds" are obtained by taking fractionOfSecond and dividing by timebase.
                // Since we are converting to ticks, we need to multiply by Ticks.PerSecond.
                // We do the multiplication first so that the whole operation can be done using integer arithmetic.
                // m_timebase / 2L is added before dividing by timebase in order to round the result.
                long ticksBeyondSecond = (fractionOfSecond & ~Common.TimeQualityFlagsMask) * Ticks.PerSecond;
                m_timestamp += (ticksBeyondSecond + m_timebase / 2L) / m_timebase;

                // Hang on to configured frame rate and ticks per frame
                m_framesPerSecond = configurationFrame.FrameRate;
                m_ticksPerFrame = Ticks.PerSecond / (double)m_framesPerSecond;
            }

            m_timeQualityFlags = fractionOfSecond & Common.TimeQualityFlagsMask;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommonFrameHeader(SerializationInfo info, StreamingContext context)
        {
            // Deserialize common frame header
            m_frameType = (FrameType)info.GetValue("frameType", typeof(FrameType));
            m_version = info.GetByte("version");
            m_frameLength = info.GetUInt16("frameLength");
            m_timebase = info.GetUInt32("timebase");
            m_timeQualityFlags = info.GetUInt32("timeQualityFlags");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets timestamp of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public Ticks Timestamp
        {
            get => m_timestamp;
            set => m_timestamp = value;
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 specific frame type of this frame.
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
            get => m_frameType;
            set => m_frameType = value;
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 version of this frame.
        /// </summary>
        public byte Version
        {
            get => m_version;
            set => m_version = value;
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 frame length of this frame.
        /// </summary>
        public ushort FrameLength
        {
            get => m_frameLength;
            set => m_frameLength = value;
        }

        /// <summary>
        /// Gets or sets the length of the data in the IEEE C37.118 frame (i.e., the <see cref="FrameLength"/> minus the header length and checksum: <see cref="FrameLength"/> - 8).
        /// </summary>
        public ushort DataLength
        {
            get =>
                // Data length will be frame length minus common header length minus crc16
                (ushort)(FrameLength - FixedLength - 2);
            set
            {
                if (value > Common.MaximumDataLength)
                    throw new OverflowException($"Data length value cannot exceed {Common.MaximumDataLength}");

                FrameLength = (ushort)(value + FixedLength + 2);
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 ID code of this frame.
        /// </summary>
        public ushort IDCode
        {
            get => m_idCode;
            set => m_idCode = value;
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 resolution of fractional time stamps.
        /// </summary>
        public uint Timebase
        {
            get => m_timebase;
            set => m_timebase = value;
        }

        /// <summary>
        /// Gets the IEEE C37.118 second of century.
        /// </summary>
        public uint SecondOfCentury
        {
            get
            {
                decimal seconds = Math.Truncate(TimeTag.Value);

                if (seconds < 0 || seconds > uint.MaxValue)
                    return 0;

                return (uint)seconds;
            }
        }

        /// <summary>
        /// Gets the IEEE C37.118 fraction of second.
        /// </summary>
        public UInt24 FractionOfSecond
        {
            get
            {
                if (m_framesPerSecond > 0)
                {
                    // If frames per second is available, a higher precision FRACSEC can be calculated
                    long frameIndex = (long)Math.Round(m_timestamp.DistanceBeyondSecond() / m_ticksPerFrame);
                    return (UInt24)(uint)((m_timebase * frameIndex + m_framesPerSecond / 2L) / m_framesPerSecond);
                }

                // Fraction of second is determined by taking the "actual fractional second" of the timestamp and multiplying by timebase.
                // Multiplication is done here before division so that the whole operation can be done using integer arithmetic.
                // Ticks.PerSecond / 2L is added before dividing in order to round the result.
                return (UInt24)(uint)((m_timestamp.DistanceBeyondSecond() * m_timebase + Ticks.PerSecond / 2L) / Ticks.PerSecond);
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 <see cref="TimeQualityFlags"/>.
        /// </summary>
        public TimeQualityFlags TimeQualityFlags
        {
            get => (TimeQualityFlags)(m_timeQualityFlags & ~(uint)TimeQualityFlags.TimeQualityIndicatorCodeMask);
            set => m_timeQualityFlags = (m_timeQualityFlags & (uint)TimeQualityFlags.TimeQualityIndicatorCodeMask) | (uint)value;
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 <see cref="TimeQualityIndicatorCode"/>.
        /// </summary>
        public TimeQualityIndicatorCode TimeQualityIndicatorCode
        {
            get => (TimeQualityIndicatorCode)(m_timeQualityFlags & (uint)TimeQualityFlags.TimeQualityIndicatorCodeMask);
            set => m_timeQualityFlags = (m_timeQualityFlags & ~(uint)TimeQualityFlags.TimeQualityIndicatorCodeMask) | (uint)value;
        }

        /// <summary>
        /// Gets time as a <see cref="UnixTimeTag"/> representing seconds of current <see cref="Timestamp"/>.
        /// </summary>
        public UnixTimeTag TimeTag => new UnixTimeTag(m_timestamp);

        /// <summary>
        /// Gets or sets the parsing state for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public IChannelParsingState State
        {
            get => m_state;
            set => m_state = value;
        }

        // Gets or sets any additional state information - satisfies ICommonHeader<FrameType>.State interface property
        object ICommonHeader<FrameType>.State
        {
            get => m_state;
            set => m_state = value as IChannelParsingState;
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
                // Translate IEEE C37.118 specific frame type to fundamental frame type
                switch (m_frameType)
                {
                    case IEEEC37_118.FrameType.DataFrame:
                        return FundamentalFrameType.DataFrame;
                    case IEEEC37_118.FrameType.ConfigurationFrame1:
                    case IEEEC37_118.FrameType.ConfigurationFrame2:
                    case IEEEC37_118.FrameType.ConfigurationFrame3:
                        return FundamentalFrameType.ConfigurationFrame;
                    case IEEEC37_118.FrameType.HeaderFrame:
                        return FundamentalFrameType.HeaderFrame;
                    case IEEEC37_118.FrameType.CommandFrame:
                        return FundamentalFrameType.CommandFrame;
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
                buffer[1] = (byte)((byte)TypeID | Version);
                BigEndian.CopyBytes(FrameLength, buffer, 2);
                BigEndian.CopyBytes(IDCode, buffer, 4);
                BigEndian.CopyBytes(SecondOfCentury, buffer, 6);
                BigEndian.CopyBytes(FractionOfSecond | (int)m_timeQualityFlags, buffer, 10);

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
            attributes.Add("Frame Type", $"{(ushort)TypeID}: {TypeID}");
            attributes.Add("Frame Length", FrameLength.ToString());
            attributes.Add("Version", Version.ToString());
            attributes.Add("Second of Century", SecondOfCentury.ToString());
            attributes.Add("Fraction of Second", FractionOfSecond.ToString());

            uint timeQualityFlags = (uint)TimeQualityFlags;

            attributes.Add("Time Quality Flags", timeQualityFlags.ToString());

            if (timeQualityFlags > 0)
                attributes.Add("Leap Second State", TimeQualityFlags.ToString());
            else
                attributes.Add("Leap Second State", "No leap second is currently pending");

            attributes.Add("Time Quality Indicator Code", $"{(uint)TimeQualityIndicatorCode}: {TimeQualityIndicatorCode}");
            attributes.Add("Time Base", Timebase.ToString());
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize unique common frame header values
            info.AddValue("frameType", m_frameType, typeof(FrameType));
            info.AddValue("version", m_version);
            info.AddValue("frameLength", m_frameLength);
            info.AddValue("timebase", m_timebase);
            info.AddValue("timeQualityFlags", m_timeQualityFlags);
        }

        #endregion
    }
}