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
//  03/20/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************
// ReSharper disable UnusedParameter.Local

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSF.Parsing;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Represents the common header for a SEL CWS frame of data.
/// </summary>
public class CommonFrameHeader : CommonHeaderBase<FrameType>
{
    #region [ Members ]

    // Constants

    /// <summary>
    /// Total fixed length of <see cref="CommonFrameHeader"/>.
    /// </summary>
    public const ushort FixedLength = 16;

    // Fields
    private FrameType m_frameType;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="CommonFrameHeader"/> from <paramref name="frameType"/>.
    /// </summary>
    /// <param name="frameType">Type of the frame.</param>
    public CommonFrameHeader(FrameType frameType)
    {
        m_frameType = frameType;
    }

    /// <summary>
    /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Buffer that contains data to parse.</param>
    /// <param name="startIndex">Start index into buffer where valid data begins.</param>
    /// <param name="length">Length of valid data in <paramref name="buffer"/>.</param>
    public CommonFrameHeader(byte[] buffer, int startIndex, int length)
    {
        if (length < FixedLength)
            throw new ArgumentException($"Insufficient data length for SEL CWS Common Frame Header. Expected at least {FixedLength} bytes, received {length} bytes.");

        // Validate SEL CWS data image
        if (buffer[startIndex + 1] != Common.Version)
            throw new InvalidOperationException($"Bad data stream, expected version byte 0x01 as second byte in SEL CWS frame, got 0x{buffer[startIndex + 1]:X2}");

        // Validate frame type
        if (buffer[startIndex] != (byte)FrameType.ConfigurationFrame && buffer[startIndex] != (byte)FrameType.DataFrame)
            throw new InvalidOperationException($"Bad data stream, expected frame type of 0x00 or 0x01 as first byte in SEL CWS frame, got 0x{buffer[startIndex]:X2}");

        m_frameType = (FrameType)buffer[startIndex];

        // Parse frame length
        FrameLength = (int)BigEndian.ToUInt32(buffer, startIndex + 2) + 6;

        // Parse channel ID
        ChannelID = BigEndian.ToUInt64(buffer, startIndex + 6);

        // Parse packet count
        PacketCount = BigEndian.ToUInt16(buffer, startIndex + 14);
    }

    /// <summary>
    /// Creates a new <see cref="CommonFrameHeader"/> from serialization parameters.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
    /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
    protected CommonFrameHeader(SerializationInfo info, StreamingContext context)
    {
        // Deserialize common frame header
        m_frameType = (FrameType)(info.GetValue("frameType", typeof(FrameType)) ?? FrameType.DataFrame);
        FrameLength = info.GetInt32("frameLength");
        ChannelID = info.GetUInt64("channelID");
        PacketCount = info.GetUInt16("packetCount");
        NominalFrequency = (LineFrequency)(info.GetValue("nominalFrequency", typeof(LineFrequency)) ?? Common.DefaultNominalFrequency);
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets the <see cref="TypeID"/> of this SEL CWS frame.
    /// </summary>
    public override FrameType TypeID
    {
        get => m_frameType;
        set => m_frameType = value;
    }

    /// <summary>
    /// Gets the length of this SEL CWS frame.
    /// </summary>
    public int FrameLength { get; }

    /// <summary>
    /// Gets the channel ID associated with this SEL CWS frame.
    /// </summary>
    public ulong ChannelID { get; }

    /// <summary>
    /// Gets the packet count for this SEL CWS frame.
    /// </summary>
    public ushort PacketCount { get; }

    /// <summary>
    /// Gets the nominal <see cref="LineFrequency"/> of the SEL CWS device.
    /// </summary>
    public LineFrequency NominalFrequency { get; set; }

    /// <summary>
    /// Gets the fundamental frame type of this frame.
    /// </summary>
    /// <remarks>
    /// Frames are generally classified as data, configuration or header frames. This returns the general frame classification.
    /// </remarks>
    public FundamentalFrameType FundamentalFrameType
    {
        get
        {
            // Translate specific frame type to fundamental frame type
            return TypeID switch
            {
                FrameType.DataFrame => FundamentalFrameType.DataFrame,
                FrameType.ConfigurationFrame  => FundamentalFrameType.ConfigurationFrame,
                _ => FundamentalFrameType.Undetermined,
            };
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
        attributes.Add("Frame Type", $"{(ushort)m_frameType}: {m_frameType}");
        attributes.Add("Frame Length", FrameLength.ToString());
        attributes.Add("Version", $"0x{Common.Version:X2}");
        attributes.Add("Channel ID", ChannelID.ToString());
        attributes.Add("Packet Count", PacketCount.ToString());
        attributes.Add("Nominal Frequency", NominalFrequency.ToString());
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
        info.AddValue("frameLength", FrameLength);
        info.AddValue("channelID", ChannelID);
        info.AddValue("packetCount", PacketCount);
        info.AddValue("nominalFrequency", NominalFrequency, typeof(LineFrequency));
    }

    #endregion
}