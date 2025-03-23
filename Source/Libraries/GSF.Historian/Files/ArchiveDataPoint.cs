//******************************************************************************************************
//  ArchiveDataPoint.cs - Gbtc
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
//  02/23/2007 - Pinal C. Patel
//       Generated original version of code based on DatAWare system specifications by Brian B. Fox, GSF.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//  09/10/2009 - Pinal C. Patel
//       Added constructor that takes in IMeasurement.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  03/15/2010 - Pinal C. Patel
//       Implemented IFormattable.ToString() overloads.
//  04/20/2010 - J. Ritchie Carroll
//       Added construction overload for IMeasurement that accepts specific quality.
//  09/16/2010 - J. Ritchie Carroll
//       Modified formatted time to include milliseconds.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  11/30/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Globalization;
using GSF.TimeSeries;

// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace GSF.Historian.Files;

/// <summary>
/// Represents time-series data stored in <see cref="ArchiveFile"/>.
/// </summary>
/// <seealso cref="ArchiveFile"/>
/// <seealso cref="ArchiveFileAllocationTable"/>
/// <seealso cref="ArchiveDataBlock"/>
/// <seealso cref="ArchiveDataBlockPointer"/>
public class ArchiveDataPoint : IDataPoint
{
    // **************************************************************************************************
    // *                                        Binary Structure                                        *
    // **************************************************************************************************
    // * # Of Bytes Byte Index Data Type  Property Name                                                 *
    // * ---------- ---------- ---------- --------------------------------------------------------------*
    // * 4          0-3        Int32      Time                                                          *
    // * 2          4-5        Int16      Flags (Quality & Milliseconds)                                *
    // * 4          6-9        Single     Value                                                         *
    // **************************************************************************************************

    #region [ Members ]

    // Constants

    /// <summary>
    /// Specifies the number of bytes in the binary image of <see cref="ArchiveDataPoint"/>.
    /// </summary>
    public const int FixedLength = 10;

    /// <summary>
    /// Specifies the bit-mask for <see cref="Quality"/> stored in <see cref="Flags"/>.
    /// </summary>
    protected const Bits QualityMask = Bits.Bit00 | Bits.Bit01 | Bits.Bit02 | Bits.Bit03 | Bits.Bit04;

    /// <summary>
    /// Specifies the bit-mask for <see cref="TimeTag"/> milliseconds stored in <see cref="Flags"/>.
    /// </summary>
    protected const Bits MillisecondMask = Bits.Bit05 | Bits.Bit06 | Bits.Bit07 | Bits.Bit08 | Bits.Bit09 | Bits.Bit10 | Bits.Bit11 | Bits.Bit12 | Bits.Bit13 | Bits.Bit14 | Bits.Bit15;

    // Fields
    private int m_historianID;
    private TimeTag m_time;
    private float m_value;
    private int m_flags;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveDataPoint"/> class.
    /// </summary>
    /// <param name="historianID">Historian identifier of <see cref="ArchiveDataPoint"/>.</param>
    public ArchiveDataPoint(int historianID)
    {
        m_time = TimeTag.MinValue;
        HistorianID = historianID;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveDataPoint"/> class.
    /// </summary>
    /// <param name="dataPoint">A time-series data point.</param>
    public ArchiveDataPoint(IDataPoint dataPoint)
        : this(dataPoint.HistorianID, dataPoint.Time, dataPoint.Value, dataPoint.Quality)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveDataPoint"/> class from a <see cref="IMeasurement"/> value.
    /// </summary>
    /// <param name="measurement">Object that implements the <see cref="IMeasurement"/> interface.</param>
    public ArchiveDataPoint(IMeasurement measurement)
        : this(measurement, measurement.HistorianQuality())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveDataPoint"/> class from a <see cref="IMeasurement"/> value.
    /// </summary>
    /// <param name="measurement">Object that implements the <see cref="IMeasurement"/> interface.</param>
    /// <param name="quality">Specific <see cref="Quality"/> value to apply to new <see cref="ArchiveDataPoint"/>.</param>
    public ArchiveDataPoint(IMeasurement measurement, Quality quality)
        : this((int)measurement.Key.ID)
    {
        Time = new TimeTag((DateTime)measurement.Timestamp);
        Value = (float)measurement.AdjustedValue;
        Quality = quality;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveDataPoint"/> class.
    /// </summary>
    /// <param name="historianID">Historian identifier of <see cref="ArchiveDataPoint"/>.</param>
    /// <param name="time"><see cref="TimeTag"/> of <see cref="ArchiveDataPoint"/>.</param>
    /// <param name="value">Floating-point value of <see cref="ArchiveDataPoint"/>.</param>
    /// <param name="quality"><see cref="Quality"/> of <see cref="ArchiveDataPoint"/>.</param>
    public ArchiveDataPoint(int historianID, TimeTag time, float value, Quality quality)
        : this(historianID)
    {
        Time = time;
        Value = value;
        Quality = quality;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveDataPoint"/> class.
    /// </summary>
    /// <param name="historianID">Historian identifier of <see cref="ArchiveDataPoint"/>.</param>
    /// <param name="buffer">Binary image to be used for initializing <see cref="ArchiveDataPoint"/>.</param>
    /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="buffer"/>.</param>
    /// <param name="length">Valid number of bytes in <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
    public ArchiveDataPoint(int historianID, byte[] buffer, int startIndex, int length)
        : this(historianID)
    {
        ParseBinaryImage(buffer, startIndex, length);
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the historian identifier of <see cref="ArchiveDataPoint"/>.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not positive or -1.</exception>
    public int HistorianID
    {
        get => m_historianID;
        set
        {
            if (value < 1 && value != -1)
                throw new ArgumentException("Value must be positive or -1");

            m_historianID = value;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="TimeTag"/> of <see cref="ArchiveDataPoint"/>.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not between 01/01/1995 and 01/19/2063.</exception>
    public virtual TimeTag Time
    {
        get => m_time;
        set
        {
            if (value.CompareTo(TimeTag.MinValue) < 0 || value.CompareTo(TimeTag.MaxValue) > 0)
                throw new TimeTagException("Value must between 01/01/1995 and 01/19/2063");

            m_time = value;
            Flags = Flags.SetMaskedValue(MillisecondMask, m_time.ToDateTime().Millisecond << 5);
        }
    }

    /// <summary>
    /// Gets or sets the floating-point value of <see cref="ArchiveDataPoint"/>.
    /// </summary>
    public virtual float Value
    {
        get => m_value;
        set => m_value = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="Quality"/> of <see cref="ArchiveDataPoint"/>.
    /// </summary>
    public virtual Quality Quality
    {
        get => (Quality)Flags.GetMaskedValue(QualityMask);
        set => Flags = Flags.SetMaskedValue(QualityMask, (int)value);
    }

    /// <summary>
    /// Gets or sets the associated <see cref="MetadataRecord"/> with this <see cref="ArchiveDataPoint"/>.
    /// </summary>
    public virtual MetadataRecord Metadata { get; set; }

    /// <summary>
    /// Gets a boolean value that indicates whether <see cref="ArchiveDataPoint"/> contains any data.
    /// </summary>
    public virtual bool IsEmpty =>
        m_time.CompareTo(TimeTag.MinValue) == 0 &&
        m_value == 0 &&
        Quality == Quality.Unknown;

    /// <summary>
    /// Gets the length of the <see cref="ArchiveDataPoint"/>.
    /// </summary>
    public virtual int BinaryLength => FixedLength;

    /// <summary>
    /// Gets or sets the 32-bit word used for storing data of <see cref="ArchiveDataPoint"/>.
    /// </summary>
    protected virtual int Flags
    {
        get => m_flags;
        set => m_flags = value;
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes <see cref="ArchiveDataPoint"/> from the specified <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">Binary image to be used for initializing <see cref="ArchiveDataPoint"/>.</param>
    /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="buffer"/>.</param>
    /// <param name="length">Valid number of bytes in <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
    /// <returns>Number of bytes used from the <paramref name="buffer"/> for initializing <see cref="ArchiveDataPoint"/>.</returns>
    public virtual int ParseBinaryImage(byte[] buffer, int startIndex, int length)
    {
        // Binary image does not have sufficient data.
        if (length < FixedLength)
            return 0;

        // Binary image has sufficient data.
        Flags = LittleEndian.ToInt16(buffer, startIndex + 4);
        Value = LittleEndian.ToSingle(buffer, startIndex + 6);

        TimeTag value = new(LittleEndian.ToInt32(buffer, startIndex) +                      // Seconds
                           (decimal)(m_flags.GetMaskedValue(MillisecondMask) >> 5) / 1000); // Milliseconds

        // Make sure to properly validate timestamps for newly initialized or possibly corrupted blocks
        if (value.CompareTo(TimeTag.MinValue) < 0 || value.CompareTo(TimeTag.MaxValue) > 0)
            value = TimeTag.MinValue;

        Time = value;

        return FixedLength;
    }

    /// <summary>
    /// Generates binary image of the <see cref="ArchiveDataPoint"/> and copies it into the given buffer, for <see cref="BinaryLength"/> bytes.
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

        Buffer.BlockCopy(LittleEndian.GetBytes((int)m_time.Value), 0, buffer, startIndex, 4);
        Buffer.BlockCopy(LittleEndian.GetBytes((short)m_flags), 0, buffer, startIndex + 4, 2);
        Buffer.BlockCopy(LittleEndian.GetBytes(m_value), 0, buffer, startIndex + 6, 4);

        return length;
    }

    /// <summary>
    /// Compares the current <see cref="ArchiveDataPoint"/> object to <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">Object against which the current <see cref="ArchiveDataPoint"/> object is to be compared.</param>
    /// <returns>
    /// Negative value if the current <see cref="ArchiveDataPoint"/> object is less than <paramref name="obj"/>, 
    /// Zero if the current <see cref="ArchiveDataPoint"/> object is equal to <paramref name="obj"/>, 
    /// Positive value if the current <see cref="ArchiveDataPoint"/> object is greater than <paramref name="obj"/>.
    /// </returns>
    public virtual int CompareTo(object obj)
    {
        if (obj is not ArchiveDataPoint other)
            return 1;

        int result = m_historianID.CompareTo(other.HistorianID);
        return result == 0 ? m_time.CompareTo(other.Time) : result;
    }

    /// <summary>
    /// Determines whether the current <see cref="ArchiveDataPoint"/> object is equal to <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">Object against which the current <see cref="ArchiveDataPoint"/> object is to be compared for equality.</param>
    /// <returns>true if the current <see cref="ArchiveDataPoint"/> object is equal to <paramref name="obj"/>; otherwise false.</returns>
    public override bool Equals(object obj)
    {
        return CompareTo(obj) == 0;
    }

    /// <summary>
    /// Returns the text representation of <see cref="ArchiveDataPoint"/> object.
    /// </summary>
    /// <returns>A <see cref="string"/> value.</returns>
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>
    /// Returns the text representation of <see cref="ArchiveDataPoint"/> object in the specified <paramref name="format"/>.
    /// </summary>
    /// <param name="format">Format of text output (I for ID, T for Time, V for Value, Q for Quality).</param>
    /// <returns>A <see cref="string"/> value.</returns>
    public virtual string ToString(string format)
    {
        return ToString(format, null);
    }

    /// <summary>
    /// Returns the text representation of <see cref="ArchiveDataPoint"/> object using the specified <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.</param>
    /// <returns>A <see cref="string"/> value.</returns>
    public virtual string ToString(IFormatProvider provider)
    {
        return ToString("", provider);
    }

    /// <summary>
    /// Returns the text representation of <see cref="ArchiveDataPoint"/> object in the specified <paramref name="format"/> 
    /// using the specified <paramref name="provider"/>.
    /// </summary>
    /// <param name="format">Format of text output (I for ID, T for Time, V for Value, Q for Quality).</param>
    /// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.</param>
    /// <returns>A <see cref="string"/> value.</returns>
    public virtual string ToString(string format, IFormatProvider provider)
    {
        provider ??= CultureInfo.InvariantCulture;

        return format.ToUpperInvariant() switch
        {
            "I" or "ID" => m_historianID.ToString(provider),
            "T" or "TIME" => m_time.ToString(),
            "U" or "UNIXTIME" => new UnixTimeTag(m_time.ToDateTime()).Value.ToString("0.000", provider),
            "V" or "VALUE" => m_value.ToString(provider),
            "Q" or "QUALITY" => Quality.ToString(),
            "N" or "NAME" => Metadata is null ? "" : Metadata.Name,
            "D" or "DESCRIPTION" => Metadata is null ? "" : Metadata.Description,
            "S" or "SOURCE" => Metadata is null ? "" : Metadata.PlantCode,
            "S1" or "SYNONYM1" => Metadata is null ? "" : Metadata.Synonym1,
            "S2" or "SYNONYM2" => Metadata is null ? "" : Metadata.Synonym2,
            "S3" or "SYNONYM3" => Metadata is null ? "" : Metadata.Synonym3,
            _ => throw new FormatException($"Invalid format identifier specified for ArchiveDataPoint: {format}")
        };
    }

    /// <summary>
    /// Returns the hash code for the current <see cref="ArchiveDataPoint"/> object.
    /// </summary>
    /// <returns>A 32-bit signed integer value.</returns>
    public override int GetHashCode()
    {
        return m_historianID.GetHashCode();
    }

    #endregion
}