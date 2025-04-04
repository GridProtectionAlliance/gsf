//******************************************************************************************************
//  MetadataRecordConstantFields.cs - Gbtc
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
using GSF.Parsing;

namespace GSF.Historian.Files;

/// <summary>
/// Defines specific fields for <see cref="MetadataRecord"/>s that are of type <see cref="DataType.Constant"/>.
/// </summary>
/// <seealso cref="MetadataRecord"/>
public class MetadataRecordConstantFields : ISupportBinaryImage
{
    // **************************************************************************************************
    // *                                        Binary Structure                                        *
    // **************************************************************************************************
    // * # Of Bytes Byte Index Data Type  Property Name                                                 *
    // * ---------- ---------- ---------- --------------------------------------------------------------*
    // * 4          0-3        Single     Value                                                         *
    // * 4          4-7        Int32      DisplayDigits                                                 *
    // **************************************************************************************************

    #region [ Members ]

    // Constants

    /// <summary>
    /// Specifies the number of bytes in the binary image of <see cref="MetadataRecordConstantFields"/>.
    /// </summary>
    public const int FixedLength = 8;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataRecordConstantFields"/> class.
    /// </summary>
    internal MetadataRecordConstantFields()
    {
    }

    internal MetadataRecordConstantFields(BinaryReader reader)
    {
        Value = reader.ReadSingle();
        DisplayDigits = reader.ReadInt32();
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the constant value of the <see cref="MetadataRecord"/>.
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// Gets or sets the number of digits after the decimal point to be displayed for the <see cref="MetadataRecord"/>.
    /// </summary>
    public int DisplayDigits { get; set; }

    /// <summary>
    /// Gets the length of the <see cref="MetadataRecordConstantFields"/>.
    /// </summary>
    public int BinaryLength => FixedLength;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes <see cref="MetadataRecordConstantFields"/> from the specified <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Binary image to be used for initializing <see cref="MetadataRecordConstantFields"/>.</param>
    /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="buffer"/>.</param>
    /// <param name="length">Valid number of bytes in <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
    /// <returns>Number of bytes used from the <paramref name="buffer"/> for initializing <see cref="MetadataRecordConstantFields"/>.</returns>
    public int ParseBinaryImage(byte[] buffer, int startIndex, int length)
    {
        // Binary image does not have sufficient data.
        if (length < FixedLength)
            return 0;
        
        // Binary image has sufficient data.
        Value = LittleEndian.ToSingle(buffer, startIndex);
        DisplayDigits = LittleEndian.ToInt32(buffer, startIndex + 4);

        return FixedLength;
    }

    /// <summary>
    /// Generates binary image of the <see cref="MetadataRecordConstantFields"/> and copies it into the given buffer, for <see cref="BinaryLength"/> bytes.
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

        Buffer.BlockCopy(LittleEndian.GetBytes(Value), 0, buffer, startIndex, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(DisplayDigits), 0, buffer, startIndex + 4, 4);

        return length;
    }

    internal void WriteImage(BinaryWriter writer)
    {
        writer.Write(Value);
        writer.Write(DisplayDigits);
    }

    #endregion
}