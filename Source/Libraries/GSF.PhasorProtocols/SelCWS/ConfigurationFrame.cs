//******************************************************************************************************
//  ConfigurationFrame.cs - Gbtc
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
//  02/08/2007 - J. Ritchie Carroll & Jian Ryan Zuo
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************
// ReSharper disable VirtualMemberCallInConstructor

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using GSF.Parsing;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Represents the SEL CWS implementation of a <see cref="IConfigurationFrame"/> that can be sent or received.
/// </summary>
[Serializable]
public class ConfigurationFrame : ConfigurationFrameBase, ISupportSourceIdentifiableFrameImage<SourceChannel, FrameType>
{
    #region [ Members ]

    /// <summary>
    /// Represents the Length of the fixed part of this header.
    /// </summary>
    protected const int FixedHeaderLength = CommonFrameHeader.FixedLength + 8;

    // Fields
    private CommonFrameHeader m_frameHeader;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="ConfigurationFrame"/>.
    /// </summary>
    /// <remarks>
    /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse an SEL CWS configuration frame.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ConfigurationFrame()
        : base(0, new ConfigurationCellCollection(), 0, 0)
    {
        FrameRate = Common.DefaultFrameRate;
    }

    /// <summary>
    /// Creates a new <see cref="ConfigurationFrame"/> from serialization parameters.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
    /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
    protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        // Deserialize configuration frame
        m_frameHeader = (CommonFrameHeader)info.GetValue("frameHeader", typeof(CommonFrameHeader));
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame"/>.
    /// </summary>
    public new ConfigurationCellCollection Cells => base.Cells as ConfigurationCellCollection;

    /// <summary>
    /// Gets or sets the nominal <see cref="LineFrequency"/> of this <see cref="ConfigurationFrame"/>.
    /// </summary>
    public LineFrequency NominalFrequency
    {
        // Since SEL CWS only supports a single device there will only be one cell, so we just share this value
        // with our only child and expose the value at the parent level for convenience
        get => Cells.Count == 0 ? Common.DefaultNominalFrequency : Cells[0].NominalFrequency;
        set
        {
            if (Cells.Count > 0)
                Cells[0].NominalFrequency = value;
        }
    }

    /// <summary>
    /// Gets the <see cref="FrameType"/> of this <see cref="ConfigurationFrame"/>.
    /// </summary>
    public virtual FrameType TypeID => SelCWS.FrameType.ConfigurationFrame;

    /// <summary>
    /// Gets or sets current <see cref="CommonFrameHeader"/>.
    /// </summary>
    public CommonFrameHeader CommonHeader
    {
        // Make sure frame header exists
        get => m_frameHeader ??= new CommonFrameHeader(SelCWS.FrameType.ConfigurationFrame);
        set
        {
            m_frameHeader = value;

            if (m_frameHeader is not null)
                State = (m_frameHeader.State as IConfigurationFrameParsingState)!;
        }
    }

    // This interface implementation satisfies ISupportFrameImage<FrameType>.CommonHeader
    ICommonHeader<FrameType> ISupportFrameImage<FrameType>.CommonHeader
    {
        get => CommonHeader;
        set => CommonHeader = (value as CommonFrameHeader)!;
    }

    /// <summary>
    /// Gets the SEL CWS protocol version of this <see cref="ConfigurationFrame"/>.
    /// </summary>
    public byte Version => Common.Version;

    /// <summary>
    /// Gets the channel ID of this <see cref="ConfigurationFrame"/>.
    /// </summary>
    public ulong ChannelID => CommonHeader.ChannelID;

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
        int index = startIndex;
        
        // Skip past header that was already parsed...
        index += CommonFrameHeader.FixedLength;

        switch (Version)
        {
            case 0x01:
            {
                // Version 1 of the SEL CWS protocol has fixed configuration values
                // Note: We interpret analogs as phasor magnitudes
                ushort analogCount = BigEndian.ToUInt16(buffer, index);

                if (analogCount != 6)
                    throw new InvalidOperationException($"SEL CWS version 1 configuration frame expected six analog POW points, got {analogCount:N0}.");

                index += 2;

                ushort digitalCount = BigEndian.ToUInt16(buffer, index);

                if (digitalCount != 0)
                    throw new InvalidOperationException($"SEL CWS version 1 configuration frame expected zero digital points, got {digitalCount:N0}.");

                index += 2;

                int sampleRate = BigEndian.ToInt32(buffer, index);

                if (sampleRate != Common.DefaultFrameRate)
                    throw new InvalidOperationException($"SEL CWS version 1 configuration frame expected sample rate of {Common.DefaultFrameRate:N0} SPS, got {sampleRate:N0} SPS.");

                index += 4;
                
                break;
            }
        }

        Debug.Assert(index - startIndex == FixedHeaderLength, "Length mismatch during SEL CWS configuration frame header parsing.");

        return FixedHeaderLength;
    }

    /// <inheritdoc/>
    protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
    {
        int index = startIndex;

        // Parse scalars
        float[] scalars = new float[Common.MaximumAnalogValues];

        for (int i = 0; i < Common.MaximumAnalogValues; i++)
        {
            scalars[i] = BigEndian.ToSingle(buffer, index);
            index += 4;
        }

        // Parse station name (defined as ChannelName in documentation), up to null terminator
        (string stationName, int parsedLength) = ParseNullTerminatedString(buffer, index, 21);
        index += parsedLength;

        string[] analogNames = new string[Common.MaximumAnalogValues];

        // Parse analog names (defined as SignalNames in documentation), up to null terminators
        for (int i = 0; i < Common.MaximumAnalogValues; i++)
        {
            (string analogName, int analogParsedLength) = ParseNullTerminatedString(buffer, index, 21);
            analogNames[i] = analogName;
            index += analogParsedLength;
        }

        // SEL CWS configuration frames only support a single cell/device
        ConfigurationCell cell = new(this, Common.DefaultNominalFrequency, scalars, stationName, analogNames);
        Cells.Add(cell);

        return index - startIndex;
    }

    private (string, int) ParseNullTerminatedString(byte[] buffer, int startIndex, int maxLength)
    {
        int index = startIndex;

        while (index < startIndex + maxLength && buffer[index] != 0)
            index++;

        int parsedLength = index - startIndex;

        return (Encoding.UTF8.GetString(buffer, startIndex, parsedLength), parsedLength + 1);
    }

    /// <summary>
    /// Determines if checksum in the <paramref name="buffer"/> is valid.
    /// </summary>
    /// <param name="buffer">Buffer image to validate.</param>
    /// <param name="startIndex">Start index into <paramref name="buffer"/> to perform checksum.</param>
    /// <returns>Flag that determines if checksum over <paramref name="buffer"/> is valid.</returns>
    /// <remarks>
    /// SEL CWS doesn't use checksums - this always returns true.
    /// </remarks>
    protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
    {
        return true;
    }

    /// <summary>
    /// Method is not implemented.
    /// </summary>
    /// <exception cref="NotImplementedException">SEL CWS doesn't use checksums.</exception>
    /// <returns>An <see cref="UInt16"/>.</returns>
    /// <param name="buffer">A <see cref="Byte"/> array.</param>
    /// <param name="length">An <see cref="Int32"/> for the offset.</param>
    /// <param name="offset">An <see cref="Int32"/> for the length.</param>
    protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);

        // Serialize configuration frame
        info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
    }

    #endregion
}