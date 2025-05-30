//******************************************************************************************************
//  MetadataRecordDigitalFields.cs - Gbtc
//
//  Copyright � 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  02/21/2007 - Pinal C. Patel
//       Generated original version of code based on DatAWare system specifications by Brian B. Fox, GSF.
//  01/23/2008 - Pinal C. Patel
//       Added AlarmDelay property to expose delay in sending alarm notification.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  11/30/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Text;
using GSF.Parsing;

namespace GSF.Historian.Files;

/// <summary>
/// Defines specific fields for <see cref="MetadataRecord"/>s that are of type <see cref="DataType.Digital"/>.
/// </summary>
/// <seealso cref="MetadataRecord"/>
public class MetadataRecordDigitalFields : ISupportBinaryImage
{
    // **************************************************************************************************
    // *                                        Binary Structure                                        *
    // **************************************************************************************************
    // * # Of Bytes Byte Index Data Type  Property Name                                                 *
    // * ---------- ---------- ---------- --------------------------------------------------------------*
    // * 24         0-23       Char(24)   SetDescription                                                *
    // * 24         24-47      Char(24)   ClearDescription                                              *
    // * 4          48-51      Int32      AlarmState                                                    *
    // * 4          52-55      Single     AlarmDelay                                                    *
    // **************************************************************************************************

    #region [ Members ]

    // Constants

    /// <summary>
    /// Specifies the number of bytes in the binary image of <see cref="MetadataRecordDigitalFields"/>.
    /// </summary>
    public const int FixedLength = 56;

    // Fields
    private string m_setDescription;
    private string m_clearDescription;
    private int m_alarmState;
    private readonly MetadataFileLegacyMode m_legacyMode;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataRecordDigitalFields"/> class.
    /// </summary>
    /// <param name="legacyMode">Legacy mode of <see cref="MetadataFile"/>.</param>
    internal MetadataRecordDigitalFields(MetadataFileLegacyMode legacyMode)
    {
        m_setDescription = string.Empty;
        m_clearDescription = string.Empty;
        m_legacyMode = legacyMode;
    }

    internal MetadataRecordDigitalFields(BinaryReader reader)
    {
        m_legacyMode = MetadataFileLegacyMode.Disabled;
        m_setDescription = reader.ReadString();
        m_clearDescription = reader.ReadString();
        m_alarmState = reader.ReadInt32();
        AlarmDelay = reader.ReadSingle();
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the text description of the data value for the <see cref="MetadataRecord"/> when it is 1.
    /// </summary>
    /// <remarks>
    /// Maximum length for <see cref="SetDescription"/> is 24 characters.
    /// </remarks>
    public string SetDescription
    {
        get => m_setDescription;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            m_setDescription = m_legacyMode == MetadataFileLegacyMode.Enabled ? value.TruncateRight(24) : value;
        }
    }

    /// <summary>
    /// Gets or sets the text description of the data value for the <see cref="MetadataRecord"/> when it is 0.
    /// </summary>
    /// <remarks>
    /// Maximum length for <see cref="ClearDescription"/> is 24 characters.
    /// </remarks>
    public string ClearDescription
    {
        get => m_clearDescription;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            m_clearDescription = m_legacyMode == MetadataFileLegacyMode.Enabled ? value.TruncateRight(24) : value;
        }
    }

    /// <summary>
    /// Gets or sets the value (0 or 1) that indicates alarm state for the <see cref="MetadataRecord"/>.
    /// </summary>
    /// <remarks>A value of -1 indicates no alarm state.</remarks>
    /// <exception cref="ArgumentException">The value being assigned is not -1, 0 or 1.</exception>
    public int AlarmState
    {
        get => m_alarmState;
        set
        {
            if (value is < -1 or > 1)
                throw new ArgumentException("Value must be either -1, 0 or 1");

            m_alarmState = value;
        }
    }

    /// <summary>
    /// Gets or sets the time (in seconds) to wait before consecutive alarm notifications are sent for the <see cref="MetadataRecord"/>.
    /// </summary>
    public float AlarmDelay { get; set; }

    /// <summary>
    /// Gets the length of the <see cref="MetadataRecordDigitalFields"/>.
    /// </summary>
    public int BinaryLength => FixedLength;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes <see cref="MetadataRecordDigitalFields"/> from the specified <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Binary image to be used for initializing <see cref="MetadataRecordDigitalFields"/>.</param>
    /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="buffer"/>.</param>
    /// <param name="length">Valid number of bytes in <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
    /// <returns>Number of bytes used from the <paramref name="buffer"/> for initializing <see cref="MetadataRecordDigitalFields"/>.</returns>
    public int ParseBinaryImage(byte[] buffer, int startIndex, int length)
    {
        // Binary image does not have sufficient data.
        if (length < FixedLength)
            return 0;
        
        // Binary image has sufficient data.
        SetDescription = Encoding.ASCII.GetString(buffer, startIndex, 24).Trim();
        ClearDescription = Encoding.ASCII.GetString(buffer, startIndex + 24, 24).Trim();
        AlarmState = LittleEndian.ToInt32(buffer, startIndex + 48);
        AlarmDelay = LittleEndian.ToSingle(buffer, startIndex + 52);

        return FixedLength;
    }

    /// <summary>
    /// Generates binary image of the <see cref="MetadataRecordDigitalFields"/> and copies it into the given buffer, for <see cref="BinaryLength"/> bytes.
    /// </summary>
    /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
    /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
    /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="startIndex"/> or <see cref="BinaryLength"/> is less than 0 -or- 
    /// <paramref name="startIndex"/> and <see cref="BinaryLength"/> will exceed <paramref name="buffer"/> length.
    /// </exception>
    public virtual int GenerateBinaryImage(byte[] buffer, int startIndex)
    {
        int length = BinaryLength;

        buffer.ValidateParameters(startIndex, length);

        Buffer.BlockCopy(Encoding.ASCII.GetBytes(m_setDescription.PadRight(24).TruncateRight(24)), 0, buffer, startIndex, 24);
        Buffer.BlockCopy(Encoding.ASCII.GetBytes(m_clearDescription.PadRight(24).TruncateRight(24)), 0, buffer, startIndex + 24, 24);
        Buffer.BlockCopy(LittleEndian.GetBytes(m_alarmState), 0, buffer, startIndex + 48, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(AlarmDelay), 0, buffer, startIndex + 52, 4);

        return length;
    }

    internal void WriteImage(BinaryWriter writer)
    {
        writer.Write(m_setDescription);
        writer.Write(m_clearDescription);
        writer.Write(m_alarmState);
        writer.Write(AlarmDelay);
    }

    #endregion
}