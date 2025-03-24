//******************************************************************************************************
//  MetadataRecordAnalogFields.cs - Gbtc
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
/// Defines specific fields for <see cref="MetadataRecord"/>s that are of type <see cref="DataType.Analog"/>.
/// </summary>
/// <seealso cref="MetadataRecord"/>
public class MetadataRecordAnalogFields : ISupportBinaryImage
{
    // **************************************************************************************************
    // *                                        Binary Structure                                        *
    // **************************************************************************************************
    // * # Of Bytes Byte Index Data Type  Property Name                                                 *
    // * ---------- ---------- ---------- --------------------------------------------------------------*
    // * 24         0-23       Char(24)   EngineeringUnits                                              *
    // * 4          24-27      Single     HighAlarm                                                     *
    // * 4          28-31      Single     LowAlarm                                                      *
    // * 4          32-35      Single     HighRange                                                     *
    // * 4          36-39      Single     LowRange                                                      *
    // * 4          40-43      Single     HighWarning                                                   *
    // * 4          44-47      Single     LowWarning                                                    *
    // * 4          48-51      Single     ExceptionLimit                                                *
    // * 4          52-55      Single     CompressionLimit                                              *
    // * 4          56-59      Single     AlarmDelay                                                    *
    // * 4          60-63      Int32      DisplayDigits                                                 *
    // **************************************************************************************************

    #region [ Members ]

    // Constants

    /// <summary>
    /// Specifies the number of bytes in the binary image of <see cref="MetadataRecordAnalogFields"/>.
    /// </summary>
    public const int FixedLength = 64;

    // Fields
    private string m_engineeringUnits;
    private readonly MetadataFileLegacyMode m_legacyMode;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataRecordAnalogFields"/> class.
    /// </summary>
    /// <param name="legacyMode">Legacy mode of <see cref="MetadataFile"/>.</param>
    internal MetadataRecordAnalogFields(MetadataFileLegacyMode legacyMode)
    {
        m_engineeringUnits = string.Empty;
        m_legacyMode = legacyMode;
    }

    internal MetadataRecordAnalogFields(BinaryReader reader)
    {
        m_legacyMode = MetadataFileLegacyMode.Disabled;
        m_engineeringUnits = reader.ReadString();
        HighAlarm = reader.ReadSingle();
        LowAlarm = reader.ReadSingle();
        HighRange = reader.ReadSingle();
        LowRange = reader.ReadSingle();
        HighWarning = reader.ReadSingle();
        LowWarning = reader.ReadSingle();
        ExceptionLimit = reader.ReadSingle();
        CompressionLimit = reader.ReadSingle();
        AlarmDelay = reader.ReadSingle();
        DisplayDigits = reader.ReadInt32();
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the engineering units of archived data values for the <see cref="MetadataRecord"/>.
    /// </summary>
    /// <remarks>
    /// Maximum length for <see cref="EngineeringUnits"/> is 24 characters.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
    public string EngineeringUnits
    {
        get => m_engineeringUnits;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            m_engineeringUnits = m_legacyMode == MetadataFileLegacyMode.Enabled ? value.TruncateRight(24) : value;
        }
    }

    /// <summary>
    /// Gets or sets the value above which archived data for the <see cref="MetadataRecord"/> is assigned a quality of <see cref="Quality.ValueAboveHiHiAlarm"/>.
    /// </summary>
    public float HighAlarm { get; set; }

    /// <summary>
    /// Gets or sets the value above which archived data for the <see cref="MetadataRecord"/> is assigned a quality of <see cref="Quality.ValueBelowLoLoAlarm"/>.
    /// </summary>
    public float LowAlarm { get; set; }

    /// <summary>
    /// Gets or sets the value above which archived data for the <see cref="MetadataRecord"/> is assigned a quality of <see cref="Quality.UnreasonableHigh"/>.
    /// </summary>
    public float HighRange { get; set; }

    /// <summary>
    /// Gets or sets the value above which archived data for the <see cref="MetadataRecord"/> is assigned a quality of <see cref="Quality.UnreasonableLow"/>.
    /// </summary>
    public float LowRange { get; set; }

    /// <summary>
    /// Gets or sets the value above which archived data for the <see cref="MetadataRecord"/> is assigned a quality of <see cref="Quality.ValueAboveHiAlarm"/>.
    /// </summary>
    public float HighWarning { get; set; }

    /// <summary>
    /// Gets or sets the value above which archived data for the <see cref="MetadataRecord"/> is assigned a quality of <see cref="Quality.ValueBelowLoAlarm"/>.
    /// </summary>
    public float LowWarning { get; set; }

    /// <summary>
    /// Gets or sets the amount, expressed in <see cref="EngineeringUnits"/>,  by which data values for the <see cref="MetadataRecord"/> must change before being sent for archival by the data aquisition source.
    /// </summary>
    public float ExceptionLimit { get; set; }

    /// <summary>
    /// Gets or sets the amount, expressed in <see cref="EngineeringUnits"/>, by which data values for the <see cref="MetadataRecord"/>  must changed before being archived.
    /// </summary>
    public float CompressionLimit { get; set; }

    /// <summary>
    /// Gets or sets the time (in seconds) to wait before consecutive alarm notifications are sent for the <see cref="MetadataRecord"/>.
    /// </summary>
    public float AlarmDelay { get; set; }

    /// <summary>
    /// Gets or sets the number of digits after the decimal point to be displayed for the <see cref="MetadataRecord"/>.
    /// </summary>
    public int DisplayDigits { get; set; }

    /// <summary>
    /// Gets the length of the <see cref="MetadataRecordAnalogFields"/>.
    /// </summary>
    public int BinaryLength => FixedLength;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes <see cref="MetadataRecordAnalogFields"/> from the specified <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Binary image to be used for initializing <see cref="MetadataRecordAnalogFields"/>.</param>
    /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="buffer"/>.</param>
    /// <param name="length">Valid number of bytes in <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
    /// <returns>Number of bytes used from the <paramref name="buffer"/> for initializing <see cref="MetadataRecordAnalogFields"/>.</returns>
    public int ParseBinaryImage(byte[] buffer, int startIndex, int length)
    {
        // Binary image does not have sufficient data.
        if (length < FixedLength)
            return 0;
        
        // Binary image has sufficient data.
        EngineeringUnits = Encoding.ASCII.GetString(buffer, startIndex, 24).Trim();
        HighAlarm = LittleEndian.ToSingle(buffer, startIndex + 24);
        LowAlarm = LittleEndian.ToSingle(buffer, startIndex + 28);
        HighRange = LittleEndian.ToSingle(buffer, startIndex + 32);
        LowRange = LittleEndian.ToSingle(buffer, startIndex + 36);
        HighWarning = LittleEndian.ToSingle(buffer, startIndex + 40);
        LowWarning = LittleEndian.ToSingle(buffer, startIndex + 44);
        ExceptionLimit = LittleEndian.ToSingle(buffer, startIndex + 48);
        CompressionLimit = LittleEndian.ToSingle(buffer, startIndex + 52);
        AlarmDelay = LittleEndian.ToSingle(buffer, startIndex + 56);
        DisplayDigits = LittleEndian.ToInt32(buffer, startIndex + 60);

        return FixedLength;
    }

    /// <summary>
    /// Generates binary image of the <see cref="MetadataRecordAnalogFields"/> and copies it into the given buffer, for <see cref="BinaryLength"/> bytes.
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

        Buffer.BlockCopy(Encoding.ASCII.GetBytes(m_engineeringUnits.PadRight(24).TruncateRight(24)), 0, buffer, startIndex, 24);
        Buffer.BlockCopy(LittleEndian.GetBytes(HighAlarm), 0, buffer, startIndex + 24, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(LowAlarm), 0, buffer, startIndex + 28, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(HighRange), 0, buffer, startIndex + 32, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(LowRange), 0, buffer, startIndex + 36, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(HighWarning), 0, buffer, startIndex + 40, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(LowWarning), 0, buffer, startIndex + 44, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(ExceptionLimit), 0, buffer, startIndex + 48, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(CompressionLimit), 0, buffer, startIndex + 52, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(AlarmDelay), 0, buffer, startIndex + 56, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(DisplayDigits), 0, buffer, startIndex + 60, 4);

        return length;
    }

    internal void WriteImage(BinaryWriter writer)
    {
        writer.Write(m_engineeringUnits);
        writer.Write(HighAlarm);
        writer.Write(LowAlarm);
        writer.Write(HighRange);
        writer.Write(LowRange);
        writer.Write(HighWarning);
        writer.Write(LowWarning);
        writer.Write(ExceptionLimit);
        writer.Write(CompressionLimit);
        writer.Write(AlarmDelay);
        writer.Write(DisplayDigits);
    }

    #endregion
}