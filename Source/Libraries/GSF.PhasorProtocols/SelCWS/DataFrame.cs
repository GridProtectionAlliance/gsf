//******************************************************************************************************
//  DataFrame.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  11/04/2025 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using GSF.Parsing;

namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Represents the SEL CWS implementation of a <see cref="IDataFrame"/> that can be sent or received.
/// </summary>
[Serializable]
public class DataFrame : DataFrameBase, ISupportSourceIdentifiableFrameImage<SourceChannel, FrameType>
{
    #region [ Members ]

    // Fields
    private CommonFrameHeader m_frameHeader;
    private long m_nanosecondTimestamp;
    private bool m_initialDataFrame;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="DataFrame"/>.
    /// </summary>
    /// <remarks>
    /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse an SEL CWS data frame.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public DataFrame()
        : base(new DataCellCollection(), 0, null)
    {
        // Only DataFrame that is automatically parsed is the initial one that contains the nanosecond timestamp
        m_initialDataFrame = true;
    }

    /// <summary>
    /// Creates a new <see cref="DataFrame"/> from specified parameters.
    /// </summary>
    /// <param name="nanosecondTimestamp">The exact timestamp, in nanoseconds, of the data represented by this <see cref="DataFrame"/>.</param>
    /// <param name="configurationFrame">The <see cref="ConfigurationFrame"/> associated with this <see cref="DataFrame"/>.</param>
    /// <remarks>
    /// This constructor is used by a consumer to generate a SEL CWS data frame.
    /// </remarks>
    public DataFrame(long nanosecondTimestamp, ConfigurationFrame configurationFrame)
        : base(new DataCellCollection(), nanosecondTimestamp / 100 + UnixTimeTag.BaseTicks.Value, configurationFrame)
    {
        m_nanosecondTimestamp = nanosecondTimestamp;
    }

    /// <summary>
    /// Creates a new <see cref="DataFrame"/> from serialization parameters.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
    /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
    protected DataFrame(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        // Deserialize configuration frame
        m_frameHeader = info.GetValue("frameHeader", typeof(CommonFrameHeader)) as CommonFrameHeader;
        NanosecondTimestamp = info.GetInt64("nanosecondTimestamp");
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets reference to the <see cref="DataCellCollection"/> for this <see cref="DataFrame"/>.
    /// </summary>
    public new DataCellCollection Cells => (base.Cells as DataCellCollection)!;

    /// <summary>
    /// Gets or sets <see cref="ConfigurationFrame"/> associated with this <see cref="DataFrame"/>.
    /// </summary>
    public new ConfigurationFrame ConfigurationFrame
    {
        get => (base.ConfigurationFrame as ConfigurationFrame)!;
        set => base.ConfigurationFrame = value;
    }

    /// <summary>
    /// Gets or sets the nanosecond timestamp.
    /// </summary>
    public long NanosecondTimestamp
    {
        get => m_nanosecondTimestamp;
        set
        {
            m_nanosecondTimestamp = value;
            Timestamp = m_nanosecondTimestamp / 100 + UnixTimeTag.BaseTicks.Value;
        }
    }

    /// <summary>
    /// Gets the <see cref="FrameType"/> of this <see cref="DataFrame"/>.
    /// </summary>
    public virtual FrameType TypeID => SelCWS.FrameType.DataFrame;

    /// <summary>
    /// Gets flag that determines if frame image can be queued for publication or should be processed immediately.
    /// </summary>
    public override bool AllowQueuedPublication => false;

    /// <summary>
    /// Gets or sets current <see cref="CommonFrameHeader"/>.
    /// </summary>
    public CommonFrameHeader CommonHeader
    {
        get => m_frameHeader!;
        set
        {
            m_frameHeader = value;

            if (m_frameHeader is not null)
                State = (m_frameHeader.State as IDataFrameParsingState)!;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// This property is overridden so the length can be adjusted for lack of a checksum.
    /// </remarks>
    public override int BinaryLength
    {
        get
        {
            if (TrustHeaderLength && ParsedBinaryLength > 0)
                return ParsedBinaryLength;

            // Subtract two bytes for no CRC
            return base.BinaryLength - 2;
        }
    }

    /// <inheritdoc />
    protected override int BodyLength
    {
        get
        {
            int length = ConfigurationFrame is null ? 24 : base.BodyLength;

            // Add nanosecond timestamp length for initial frame
            if (m_initialDataFrame)
                length += 8;

            return length;
        }
    }

    // This interface implementation satisfies ISupportFrameImage<FrameType>.CommonHeader
    ICommonHeader<FrameType> ISupportFrameImage<FrameType>.CommonHeader
    {
        get => CommonHeader;
        set => CommonHeader = (value as CommonFrameHeader)!;
    }

    /// <inheritdoc/>
    public override Dictionary<string, string> Attributes
    {
        get
        {
            Dictionary<string, string> baseAttributes = base.Attributes;

            CommonHeader.AppendHeaderAttributes(baseAttributes);
            baseAttributes.Add("Nanosecond Timestamp", $"{m_nanosecondTimestamp:N0}");

            return baseAttributes;
        }
    }

    #endregion

    #region [ Methods ]

    /// <inheritdoc/>
    protected override int ParseHeaderImage(byte[] buffer, int startIndex, int length)
    {
        // We already parsed the frame header for initial data frame, so we just skip past it...
        return m_initialDataFrame ? CommonFrameHeader.FixedLength : 0;
    }

    /// <inheritdoc/>
    protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
    {
        int index = startIndex;

        if (m_initialDataFrame)
        {
            // Parse nanosecond timestamp
            NanosecondTimestamp = BigEndian.ToInt64(buffer, index);
            index += 8;
        }

        index += base.ParseBodyImage(buffer, index, length - (index - startIndex));
        return index - startIndex;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SEL CWS doesn't use checksums - this always returns true.
    /// </remarks>
    protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
    {
        return true;
    }

    /// <inheritdoc/>
    protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);

        // Serialize configuration frame
        info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
        info.AddValue("nanosecondTimestamp", m_nanosecondTimestamp);
    }

    #endregion
}