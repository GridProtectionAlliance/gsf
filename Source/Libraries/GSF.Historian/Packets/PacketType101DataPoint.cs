﻿//******************************************************************************************************
//  PacketType101DataPoint.cs - Gbtc
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
//  06/04/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/21/2009 - Pinal C. Patel
//       Added overloaded constructor that takes in IMeasurement.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  11/30/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.Historian.Files;
using GSF.TimeSeries;

namespace GSF.Historian.Packets;

/// <summary>
/// Represents time-series data transmitted in <see cref="PacketType101"/>.
/// </summary>
/// <seealso cref="PacketType101"/>
public class PacketType101DataPoint : ArchiveDataPoint
{
    // **************************************************************************************************
    // *                                        Binary Structure                                        *
    // **************************************************************************************************
    // * # Of Bytes Byte Index Data Type  Property Name                                                 *
    // * ---------- ---------- ---------- --------------------------------------------------------------*
    // * 4          0-3        Int32      HistorianID                                                   *
    // * 4          4-7        Int32      Time                                                          *
    // * 2          8-9        Int16      Flags (Quality & Milliseconds)                                *
    // * 4          10-13      Single     Value                                                         *
    // **************************************************************************************************

    #region [ Members ]

    // Constants

    /// <summary>
    /// Specifies the number of bytes in the binary image of <see cref="PacketType101DataPoint"/>.
    /// </summary>
    public new const int FixedLength = 14;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketType101DataPoint"/> class.
    /// </summary>
    /// <param name="dataPoint">A time-series data point.</param>
    public PacketType101DataPoint(IDataPoint dataPoint)
        : base(dataPoint)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketType101DataPoint"/> class.
    /// </summary>
    /// <param name="measurement">Object that implements the <see cref="IMeasurement"/> interface.</param>
    public PacketType101DataPoint(IMeasurement measurement)
        : base(measurement)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketType101DataPoint"/> class.
    /// </summary>
    /// <param name="historianID">Historian identifier of <see cref="PacketType101DataPoint"/>.</param>
    /// <param name="time"><see cref="TimeTag"/> of <see cref="PacketType101DataPoint"/>.</param>
    /// <param name="value">Floating-point value of <see cref="PacketType101DataPoint"/>.</param>
    /// <param name="quality"><see cref="Quality"/> of <see cref="PacketType101DataPoint"/>.</param>
    public PacketType101DataPoint(int historianID, TimeTag time, float value, Quality quality)
        : base(historianID, time, value, quality)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketType101DataPoint"/> class.
    /// </summary>
    /// <param name="buffer">Binary image to be used for initializing <see cref="PacketType101DataPoint"/>.</param>
    /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="buffer"/>.</param>
    /// <param name="length">Valid number of bytes in <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
    public PacketType101DataPoint(byte[] buffer, int startIndex, int length)
        : base(1, buffer, startIndex, length)
    {
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Gets the length of the <see cref="PacketType101DataPoint"/>.
    /// </summary>
    public override int BinaryLength => FixedLength;

    /// <summary>
    /// Initializes <see cref="PacketType101DataPoint"/> from the specified <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Binary image to be used for initializing <see cref="PacketType101DataPoint"/>.</param>
    /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="buffer"/>.</param>
    /// <param name="length">Valid number of bytes in <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
    /// <returns>Number of bytes used from the <paramref name="buffer"/> for initializing <see cref="PacketType101DataPoint"/>.</returns>
    public override int ParseBinaryImage(byte[] buffer, int startIndex, int length)
    {
        // Binary image does not have sufficient data.
        if (length < FixedLength)
            return 0;
        
        // Binary image has sufficient data.
        HistorianID = LittleEndian.ToInt32(buffer, startIndex);
        Flags = LittleEndian.ToInt16(buffer, startIndex + 8);
        Value = LittleEndian.ToSingle(buffer, startIndex + 10);
        Time = new TimeTag(LittleEndian.ToInt32(buffer, startIndex + 4) +   // Seconds
                           (decimal)(Flags.GetMaskedValue(MillisecondMask) >> 5) / 1000);         // Milliseconds

        return FixedLength;
    }

    /// <summary>
    /// Generates binary image of the <see cref="PacketType101DataPoint"/> and copies it into the given buffer, for <see cref="BinaryLength"/> bytes.
    /// </summary>
    /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
    /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
    /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="startIndex"/> or <see cref="BinaryLength"/> is less than 0 -or- 
    /// <paramref name="startIndex"/> and <see cref="BinaryLength"/> will exceed <paramref name="buffer"/> length.
    /// </exception>
    public override int GenerateBinaryImage(byte[] buffer, int startIndex)
    {
        int length = BinaryLength;

        buffer.ValidateParameters(startIndex, length);

        Buffer.BlockCopy(LittleEndian.GetBytes(HistorianID), 0, buffer, startIndex, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes((int)Time.Value), 0, buffer, startIndex + 4, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes((short)Flags), 0, buffer, startIndex + 8, 2);
        Buffer.BlockCopy(LittleEndian.GetBytes(Value), 0, buffer, startIndex + 10, 4);

        return length;
    }

    #endregion
}