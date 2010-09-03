//******************************************************************************************************
//  DataPacket.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/23/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************


namespace TimeSeriesFramework
{   
    /// <summary>
    /// Represents a data transmission packet.
    /// </summary>
    /// <remarks>
    /// Data Structure:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Data Element Size</term>
    ///         <description>Data Element Description</description>
    ///     </listheader>
    ///     <item>
    ///         <term>1 byte</term>
    ///         <description><see cref="ServerResponse.DataPacket"/>, response code.</description>
    ///     </item>
    ///     <item>
    ///         <term>1 byte</term>
    ///         <description><see cref="ServerCommand.Subscribe"/>, in response to subscribe command code.</description>
    ///     </item>
    ///     <item>
    ///         <term>1 byte</term>
    ///         <description><see cref="DataPacketFlags"/> specific to data packet.</description>
    ///     </item>
    ///     <item>
    ///         <term>[8 bytes]</term>
    ///         <description>Optional data timestamp (only included for synchronized data packets.</description>
    ///     </item>
    ///     <item>
    ///         <term>4 bytes</term>
    ///         <description>Total number of serialized <see cref="IMeasurement"/> values that follow.</description>
    ///     </item>
    ///     <item>
    ///         <term>N bytes</term>
    ///         <description>Array of serialized <see cref="IMeasurement"/> values.</description>
    ///     </item>
    /// </list>
    /// <br/>
    /// Bytes of all numeric values are encoded in big-endian order.
    /// </remarks>
    public class DataPacket // : ISupportFrameImage<int>
    {
    //    #region [ Members ]

    //    // Constants

    //    /// <summary>
    //    /// Fixed length of <see cref="DataPacket"/>.
    //    /// </summary>
    //    public const int FixedLength = 36;

    //    // Fields
    //    private DataPacketFlags m_flags;
    //    private Ticks m_timestamp;
    //    private IMeasurement[] m_values;
    //    private bool m_includeTime;

    //    #endregion

    //    #region [ Properties ]

    //    /// <summary>
    //    /// Gets or sets <see cref="DataPacketFlags"/> associated with this <see cref="DataPacket"/>.
    //    /// </summary>
    //    public DataPacketFlags Flags
    //    {
    //        get
    //        {
    //            return m_flags;
    //        }
    //        set
    //        {
    //            m_flags = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets timestamp of this <see cref="DataPacket"/>.
    //    /// </summary>
    //    /// <remarks>
    //    /// <see cref="Ticks"/> is implicitly castable to <see cref="DateTime"/>.
    //    /// </remarks>
    //    public Ticks Timestamp
    //    {
    //        get
    //        {
    //            return m_timestamp;
    //        }
    //        set
    //        {
    //            m_timestamp = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets flags that determines if time is included in binary images.
    //    /// </summary>
    //    public bool IncludeTime
    //    {
    //        get
    //        {
    //            return m_includeTime;
    //        }
    //        set
    //        {
    //            m_includeTime = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets binary image of this <see cref="DataPacket"/>.
    //    /// </summary>
    //    public byte[] BinaryImage
    //    {
    //        get
    //        {
    //            return null;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets length of <see cref="BinaryImage"/> of this <see cref="DataPacket"/>.
    //    /// </summary>
    //    public int BinaryLength
    //    {
    //        get
    //        {
    //            return FixedLength + (m_includeTime ? 8 : 0); ;
    //        }
    //    }

    //    // Required by ISupportFrameImage...
    //    int ISupportFrameImage<int>.TypeID
    //    {
    //        get
    //        {
    //            // There is only one frame type for this data publication protocol
    //            return 0;
    //        }
    //    }

    //    // Required by ISupportFrameImage...
    //    ICommonHeader<int> ISupportFrameImage<int>.CommonHeader { get; set; }

    //    #endregion

    //    #region [ Methods ]

    //    /// <summary>
    //    /// Initializes a new <see cref="DataPacket"/> from the <paramref name="binaryImage"/>.
    //    /// </summary>
    //    /// <param name="binaryImage">Binary image to be used for initialization.</param>
    //    /// <param name="startIndex">0-based starting index in the <paramref name="binaryImage"/> to be used for initialization.</param>
    //    /// <param name="length">Valid number of bytes within binary image.</param>
    //    /// <returns>The number of bytes used for initialization in the <paramref name="binaryImage"/> (i.e., the number of bytes parsed).</returns>
    //    public int Initialize(byte[] binaryImage, int startIndex, int length)
    //    {
    //        // Validate CRC in binary image
    //        if (binaryImage.CrcCCITTChecksum(startIndex, 34) != EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 34))
    //            throw new InvalidOperationException("CRC check failed for NASPInet DataPacket");

    //        // Extract signal ID from image
    //        m_signalID = EndianOrder.BigEndian.ToGuid(binaryImage, startIndex);

    //        // Extract timestamp from image
    //        m_timestamp = EndianOrder.BigEndian.ToInt64(binaryImage, startIndex + 16);

    //        // Extract status flags from image (we do this before data so we can check flags)
    //        m_statusFlags = (StatusFlags)EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 32);

    //        // Extract data image from image
    //        if (DataIsEncrypted)
    //            m_encryptedData = binaryImage.BlockCopy(startIndex + 24, 8);
    //        else
    //            m_value = new BigBinaryValue(binaryImage, startIndex + 24, 8);

    //        return FixedLength;
    //    }

    //    #endregion
    }
}
