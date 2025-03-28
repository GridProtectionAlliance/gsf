//******************************************************************************************************
//  PacketType2.cs - Gbtc
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
//  07/27/2007 - Pinal C. Patel
//       Generated original version of code based on DatAWare system specifications by Brian B. Fox, GSF.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/23/2009 - Pinal C. Patel
//       Edited code comments.
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
using System.Text;
using GSF.Historian.Files;

// ReSharper disable VirtualMemberCallInConstructor

namespace GSF.Historian.Packets;

/// <summary>
/// Represents a packet to be used for sending single time (expanded format) series data point to a historian for archival.
/// </summary>
public class PacketType2 : PacketBase
{
    // **************************************************************************************************
    // *                                        Binary Structure                                        *
    // **************************************************************************************************
    // * # Of Bytes Byte Index Data Type  Property Name                                                 *
    // * ---------- ---------- ---------- --------------------------------------------------------------*
    // * 2          0-1        Int16      TypeID (packet identifier)                                    *
    // * 4          2-5        Int32      HistorianID                                                   *
    // * 2          6-7        Int16      Year                                                          *
    // * 1          8          Byte       Month                                                         *
    // * 1          9          Byte       Day                                                           *
    // * 1          10         Byte       Hour                                                          *
    // * 1          11         Byte       Minute                                                        *
    // * 1          12         Byte       Second                                                        *
    // * 1          13         Byte       Quality                                                       *
    // * 2          14-15      Int16      Milliseconds                                                  *
    // * 2          16-17      Int16      GmtOffset                                                     *
    // * 4          18-21      Single     Value                                                         *
    // **************************************************************************************************

    #region [ Members ]

    // Constants

    /// <summary>
    /// Specifies the number of bytes in the binary image of <see cref="PacketType2"/>.
    /// </summary>
    public new const int FixedLength = 22;

    // Fields
    private int m_historianID;
    private short m_year;
    private short m_month;
    private short m_day;
    private short m_hour;
    private short m_minute;
    private short m_second;
    private short m_millisecond;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketType2"/> class.
    /// </summary>
    public PacketType2()
        : base(2)
    {
        ProcessHandler = Process;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketType2"/> class.
    /// </summary>
    /// <param name="buffer">Binary image to be used for initializing <see cref="PacketType2"/>.</param>
    /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="buffer"/>.</param>
    /// <param name="length">Valid number of bytes in <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
    public PacketType2(byte[] buffer, int startIndex, int length)
        : this()
    {
        ParseBinaryImage(buffer, startIndex, length);
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the historian identifier of the time-series data.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not positive.</exception>
    public int HistorianID
    {
        get => m_historianID;
        set
        {
            if (value < 1)
                throw new ArgumentException("Value must be positive.");

            m_historianID = value;
        }
    }

    /// <summary>
    /// Gets or sets the year-part of the time.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not between 1995 and 2063.</exception>
    public short Year
    {
        get => m_year;
        set
        {
            if (value < TimeTag.MinValue.ToDateTime().Year || value > TimeTag.MaxValue.ToDateTime().Year)
                throw new ArgumentException("Value must 1995 and 2063.");

            m_year = value;
        }
    }

    /// <summary>
    /// Gets or sets the month-part of the time.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not between 1 and 12.</exception>
    public short Month
    {
        get => m_month;
        set
        {
            if (value is < 1 or > 12)
                throw new ArgumentException("Value must be between 1 and 12.");

            m_month = value;
        }
    }

    /// <summary>
    /// Gets or sets the day-part of the time.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not between 1 and 31.</exception>
    public short Day
    {
        get => m_day;
        set
        {
            if (value is < 1 or > 31)
                throw new ArgumentException("Value must be between 1 and 31.");

            m_day = value;
        }
    }

    /// <summary>
    /// Gets or sets the hour-part of the time.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not between 0 and 23.</exception>
    public short Hour
    {
        get => m_hour;
        set
        {
            if (value is < 0 or > 23)
                throw new ArgumentException("Value must be between 0 and 23.");

            m_hour = value;
        }
    }

    /// <summary>
    /// Gets or sets the minute-part of the time.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not between 0 and 59.</exception>
    public short Minute
    {
        get => m_minute;
        set
        {
            if (value is < 0 or > 59)
                throw new ArgumentException("Value must be between 0 and 59.");

            m_minute = value;
        }
    }

    /// <summary>
    /// Gets or sets the second-part of the time.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not between 0 and 59.</exception>
    public short Second
    {
        get => m_second;
        set
        {
            if (value is < 0 or > 59)
                throw new ArgumentException("Value must be between 0 and 59.");

            m_second = value;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="Quality"/> of the time-series data.
    /// </summary>
    public Quality Quality { get; set; }

    /// <summary>
    /// Gets or sets the millisecond-part of the time.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not between 0 and 999.</exception>
    public short Millisecond
    {
        get => m_millisecond;
        set
        {
            if (value is < 0 or > 999)
                throw new ArgumentException("Value must be between 0 and 999.");

            m_millisecond = value;
        }
    }

    /// <summary>
    /// Gets or sets the difference, in hours, between the local time and Greenwich Mean Time (Universal Coordinated Time).
    /// </summary>
    public short GmtOffset { get; set; }

    /// <summary>
    /// Gets or sets the value of the time-series data.
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// Gets the length of the <see cref="PacketType2"/>.
    /// </summary>
    public override int BinaryLength => FixedLength;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes <see cref="PacketType2"/> from the specified <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Binary image to be used for initializing <see cref="PacketType2"/>.</param>
    /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="buffer"/>.</param>
    /// <param name="length">Valid number of bytes in <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
    /// <returns>Number of bytes used from the <paramref name="buffer"/> for initializing <see cref="PacketType2"/>.</returns>
    public override int ParseBinaryImage(byte[] buffer, int startIndex, int length)
    {
        // Binary image does not have sufficient data.
        if (length < FixedLength)
            return 0;
        
        // Binary image has sufficient data.
        short packetID = LittleEndian.ToInt16(buffer, startIndex);

        if (packetID != TypeID)
            throw new ArgumentException($"Unexpected packet id '{packetID}' (expected '{TypeID}')");

        // We have a binary image with the correct packet id.
        HistorianID = LittleEndian.ToInt32(buffer, startIndex + 2);
        Year = LittleEndian.ToInt16(buffer, startIndex + 6);
        Month = Convert.ToInt16(buffer[startIndex + 8]);
        Day = Convert.ToInt16(buffer[startIndex + 9]);
        Hour = Convert.ToInt16(buffer[startIndex + 10]);
        Minute = Convert.ToInt16(buffer[startIndex + 11]);
        Second = Convert.ToInt16(buffer[startIndex + 12]);
        Quality = (Quality)buffer[startIndex + 13];
        Millisecond = LittleEndian.ToInt16(buffer, startIndex + 14);
        GmtOffset = LittleEndian.ToInt16(buffer, startIndex + 16);
        Value = LittleEndian.ToSingle(buffer, startIndex + 18);

        // We'll send an "ACK" to the sender if this is the last packet in the transmission.
        if (length == FixedLength)
            PreProcessHandler = PreProcess;

        return FixedLength;
    }

    /// <summary>
    /// Generates binary image of the <see cref="PacketType2"/> and copies it into the given buffer, for <see cref="BinaryLength"/> bytes.
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

        Buffer.BlockCopy(LittleEndian.GetBytes(TypeID), 0, buffer, startIndex, 2);
        Buffer.BlockCopy(LittleEndian.GetBytes(m_historianID), 0, buffer, startIndex + 2, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes(m_year), 0, buffer, startIndex + 6, 2);
        Buffer.BlockCopy(LittleEndian.GetBytes(m_month), 0, buffer, startIndex + 8, 1);
        Buffer.BlockCopy(LittleEndian.GetBytes(m_day), 0, buffer, startIndex + 9, 1);
        Buffer.BlockCopy(LittleEndian.GetBytes(m_hour), 0, buffer, startIndex + 10, 1);
        Buffer.BlockCopy(LittleEndian.GetBytes(m_minute), 0, buffer, startIndex + 11, 1);
        Buffer.BlockCopy(LittleEndian.GetBytes(m_second), 0, buffer, startIndex + 12, 1);
        Buffer.BlockCopy(LittleEndian.GetBytes((int)Quality), 0, buffer, startIndex + 13, 1);
        Buffer.BlockCopy(LittleEndian.GetBytes(m_millisecond), 0, buffer, startIndex + 14, 2);
        Buffer.BlockCopy(LittleEndian.GetBytes(GmtOffset), 0, buffer, startIndex + 16, 2);
        Buffer.BlockCopy(LittleEndian.GetBytes(Value), 0, buffer, startIndex + 18, 4);

        return length;
    }

    /// <summary>
    /// Extracts time-series data from <see cref="PacketType2"/>.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> object of <see cref="ArchiveDataPoint"/>s.</returns>
    public override IEnumerable<IDataPoint> ExtractTimeSeriesData()
    {
        DateTime timestamp = new(m_year, m_month, m_day, m_hour + GmtOffset, m_minute, m_second, m_millisecond, DateTimeKind.Utc);

        return [new ArchiveDataPoint(m_historianID, new TimeTag(timestamp), Value, Quality)];
    }

    /// <summary>
    /// Processes <see cref="PacketType2"/>.
    /// </summary>
    /// <returns>A null reference.</returns>
    protected virtual IEnumerable<byte[]> Process()
    {
        if (Archive is null)
            return null;
        
        foreach (IDataPoint dataPoint in ExtractTimeSeriesData())
            Archive.WriteData(dataPoint);

        return null;
    }

    /// <summary>
    /// Pre-processes <see cref="PacketType2"/>.
    /// </summary>
    /// <returns>A <see cref="byte"/> array for "ACK".</returns>
    protected virtual IEnumerable<byte[]> PreProcess()
    {
        return [Encoding.ASCII.GetBytes("ACK")];
    }

    #endregion
}