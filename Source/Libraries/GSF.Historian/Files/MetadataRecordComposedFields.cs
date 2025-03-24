//******************************************************************************************************
//  MetadataRecordComposedFields.cs - Gbtc
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using GSF.Parsing;

namespace GSF.Historian.Files;

/// <summary>
/// Defines specific fields for <see cref="MetadataRecord"/>s that are of type <see cref="DataType.Composed"/>.
/// </summary>
/// <seealso cref="MetadataRecord"/>
public class MetadataRecordComposedFields : ISupportBinaryImage
{
    // **************************************************************************************************
    // *                                        Binary Structure                                        *
    // **************************************************************************************************
    // * # Of Bytes Byte Index Data Type  Property Name                                                 *
    // * ---------- ---------- ---------- --------------------------------------------------------------*
    // * 4           0-3         Single      HighAlarm                                                  *
    // * 4           4-7         Single      LowAlarm                                                   *
    // * 4           8-11        Single      HighRange                                                  *
    // * 4           12-15       Single      LowRange                                                   *
    // * 4           16-19       Single      LowWarning                                                 *
    // * 4           20-23       Single      HighWarning                                                *
    // * 4           24-27       Int32       DisplayDigits                                              *
    // * 48          28-75       Int32(12)   InputPointers                                              *
    // * 24          76-99       Char(24)    EngineeringUnits                                           *
    // * 256         100-355     Char(256)   Equation                                                   *
    // * 4           356-359     Single      CompressionLimit                                           *
    // **************************************************************************************************

    #region [ Members ]

    // Constants

    /// <summary>
    /// Specifies the number of bytes in the binary image of <see cref="MetadataRecordComposedFields"/>.
    /// </summary>
    public const int FixedLength = 360;

    // Fields
    private readonly List<int> m_inputPointers;
    private string m_engineeringUnits;
    private string m_equation;
    private readonly MetadataFileLegacyMode m_legacyMode;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataRecordComposedFields"/> class.
    /// </summary>
    /// <param name="legacyMode">Legacy mode of <see cref="MetadataFile"/>.</param>
    internal MetadataRecordComposedFields(MetadataFileLegacyMode legacyMode)
    {
        m_engineeringUnits = string.Empty;
        m_equation = string.Empty;
        m_inputPointers = [];

        for (int i = 0; i < 12; i++)
            m_inputPointers.Add(0);

        m_legacyMode = legacyMode;
    }

    internal MetadataRecordComposedFields(BinaryReader reader)
    {
        m_legacyMode = MetadataFileLegacyMode.Disabled;
        HighAlarm = reader.ReadSingle();
        LowAlarm = reader.ReadSingle();
        HighRange = reader.ReadSingle();
        LowRange = reader.ReadSingle();
        LowWarning = reader.ReadSingle();
        HighWarning = reader.ReadSingle();
        DisplayDigits = reader.ReadInt32();
        m_inputPointers = [];

        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
            m_inputPointers.Add(reader.ReadInt32());

        m_engineeringUnits = reader.ReadString();
        m_equation = reader.ReadString();
        CompressionLimit = reader.ReadSingle();
    }

    #endregion

    #region [ Properties ]

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
    /// Gets or sets the value above which archived data for the <see cref="MetadataRecord"/> is assigned a quality of <see cref="Quality.ValueBelowLoAlarm"/>.
    /// </summary>
    public float LowWarning { get; set; }

    /// <summary>
    /// Gets or sets the value above which archived data for the <see cref="MetadataRecord"/> is assigned a quality of <see cref="Quality.ValueAboveHiAlarm"/>.
    /// </summary>
    public float HighWarning { get; set; }

    /// <summary>
    /// Gets or sets the number of digits after the decimal point to be displayed for the <see cref="MetadataRecord"/>.
    /// </summary>
    public int DisplayDigits { get; set; }

    /// <summary>
    /// Gets or sets the historian identifiers being used in the <see cref="Equation"/>.
    /// </summary>
    public IList<int> InputPointers => m_inputPointers.AsReadOnly();

    /// <summary>
    /// Gets or sets the engineering units of archived data values for the <see cref="MetadataRecord"/>.
    /// </summary>
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
    /// Gets or sets the mathematical equation used for calculating the data value for the <see cref="MetadataRecord"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
    public string Equation
    {
        get => m_equation;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            m_equation = m_legacyMode == MetadataFileLegacyMode.Enabled ? value.TruncateRight(256) : value;
        }
    }

    /// <summary>
    /// Gets or sets the amount, expressed in <see cref="EngineeringUnits"/>, by which data values for the <see cref="MetadataRecord"/>  must changed before being archived.
    /// </summary>
    public float CompressionLimit { get; set; }

    /// <summary>
    /// Gets the length of the <see cref="MetadataRecordComposedFields"/>.
    /// </summary>
    public int BinaryLength => FixedLength;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes <see cref="MetadataRecordComposedFields"/> from the specified <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Binary image to be used for initializing <see cref="MetadataRecordComposedFields"/>.</param>
    /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="buffer"/>.</param>
    /// <param name="length">Valid number of bytes in <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
    /// <returns>Number of bytes used from the <paramref name="buffer"/> for initializing <see cref="MetadataRecordComposedFields"/>.</returns>
    public int ParseBinaryImage(byte[] buffer, int startIndex, int length)
    {
        // Binary image does not have sufficient data.
        if (length < FixedLength)
            return 0;
        
        // Binary image has sufficient data.
        HighAlarm = LittleEndian.ToSingle(buffer, startIndex);
        LowAlarm = LittleEndian.ToSingle(buffer, startIndex + 4);
        HighRange = LittleEndian.ToSingle(buffer, startIndex + 8);
        LowRange = LittleEndian.ToSingle(buffer, startIndex + 12);
        LowWarning = LittleEndian.ToSingle(buffer, startIndex + 16);
        HighWarning = LittleEndian.ToSingle(buffer, startIndex + 20);
        DisplayDigits = LittleEndian.ToInt32(buffer, startIndex + 24);

        for (int i = 0; i < m_inputPointers.Count; i++)
            m_inputPointers[i] = LittleEndian.ToInt32(buffer, startIndex + 28 + i * 4);

        EngineeringUnits = Encoding.ASCII.GetString(buffer, startIndex + 76, 24).Trim();
        Equation = Encoding.ASCII.GetString(buffer, startIndex + 100, 256).Trim();
        CompressionLimit = LittleEndian.ToSingle(buffer, startIndex + 356);

        return FixedLength;
    }

    /// <summary>
    /// Generates binary image of the <see cref="MetadataRecordComposedFields"/> and copies it into the given buffer, for <see cref="BinaryLength"/> bytes.
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

        Buffer.BlockCopy(LittleEndian.GetBytes(HighAlarm), 0, buffer, startIndex, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(LowAlarm), 0, buffer, startIndex + 4, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(HighRange), 0, buffer, startIndex + 8, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(LowRange), 0, buffer, startIndex + 12, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(LowWarning), 0, buffer, startIndex + 16, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(HighWarning), 0, buffer, startIndex + 20, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(DisplayDigits), 0, buffer, startIndex + 24, 4);

        for (int i = 0; i < m_inputPointers.Count; i++)
            Buffer.BlockCopy(LittleEndian.GetBytes(m_inputPointers[i]), 0, buffer, startIndex + 28 + i * 4, 4);

        Buffer.BlockCopy(Encoding.ASCII.GetBytes(m_engineeringUnits.PadRight(24).TruncateRight(24)), 0, buffer, startIndex + 76, 24);
        Buffer.BlockCopy(Encoding.ASCII.GetBytes(m_equation.PadRight(256).TruncateRight(256)), 0, buffer, startIndex + 100, 256);
        Buffer.BlockCopy(LittleEndian.GetBytes(CompressionLimit), 0, buffer, startIndex + 356, 4);

        return length;
    }

    internal void WriteImage(BinaryWriter writer)
    {
        writer.Write(HighAlarm);
        writer.Write(LowAlarm);
        writer.Write(HighRange);
        writer.Write(LowRange);
        writer.Write(LowWarning);
        writer.Write(HighWarning);
        writer.Write(DisplayDigits);
        writer.Write(m_inputPointers.Count);

        foreach (int inputPointer in m_inputPointers)
            writer.Write(inputPointer);

        writer.Write(m_engineeringUnits);
        writer.Write(m_equation);
        writer.Write(CompressionLimit);
    }

    #endregion
}