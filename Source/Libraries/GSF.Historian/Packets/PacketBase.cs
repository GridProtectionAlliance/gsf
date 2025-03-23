//******************************************************************************************************
//  PacketBase.cs - Gbtc
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
//  07/27/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
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
using GSF.Parsing;

namespace GSF.Historian.Packets;

/// <summary>
/// Base class for a binary packet received by a historian.
/// </summary>
/// <seealso cref="IPacket"/>
public abstract class PacketBase : IPacket
{
    #region [ Members ]

    // Constants

    /// <summary>
    /// Specifies the number of bytes in the binary image of the packet.
    /// </summary>
    /// <remarks>A value of -1 indicates that the binary image of the packet is of variable length.</remarks>
    public const int FixedLength = -1;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the packet.
    /// </summary>
    /// <param name="packetID">Numeric identifier for the packet type.</param>
    protected PacketBase(short packetID)
    {
        TypeID = packetID;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets the length of the packet's binary representation.
    /// </summary>
    public abstract int BinaryLength { get; }

    /// <summary>
    /// Gets or sets the current <see cref="IArchive"/>.
    /// </summary>
    public IArchive Archive { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Delegate"/> that processes the packet.
    /// </summary>
    /// <remarks>
    /// <see cref="Func{TResult}"/> returns an <see cref="IEnumerable{T}"/> object containing the binary data to be sent back to the packet sender.
    /// </remarks>
    public Func<IEnumerable<byte[]>> ProcessHandler { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Delegate"/> that pre-processes the packet.
    /// </summary>
    /// <remarks>
    /// <see cref="Func{TResult}"/> returns an <see cref="IEnumerable{T}"/> object containing the binary data to be sent back to the packet sender.
    /// </remarks>
    public Func<IEnumerable<byte[]>> PreProcessHandler { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="PacketCommonHeader"/> for the packet.
    /// </summary>
    public ICommonHeader<short> CommonHeader { get; set; }

    /// <summary>
    /// Gets or sets the numeric identifier for the packet type.
    /// </summary>
    public short TypeID { get; }

    /// <summary>
    /// Gets or sets the data source identifier of the frame image.
    /// </summary>
    public Guid Source { get; set; }

    /// <summary>
    /// Gets flag that determines if frame image can be queued for publication or should be processed immediately.
    /// </summary>
    /// <remarks>
    /// Some frames, e.g., a configuration or key frame, may be critical to processing of other frames. In this
    /// case, these types of frames should be published immediately so that subsequent frame parsing can have
    /// access to needed critical information.
    /// </remarks>
    public bool AllowQueuedPublication => true;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes packet by parsing the specified <paramref name="buffer"/> containing a binary image.
    /// </summary>
    /// <param name="buffer">Buffer containing binary image to parse.</param>
    /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start parsing.</param>
    /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
    /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
    /// <remarks>
    /// Implementers should validate <paramref name="startIndex"/> and <paramref name="length"/> against <paramref name="buffer"/> length.
    /// The <see cref="GSF.ArrayExtensions.ValidateParameters{T}"/> method can be used to perform this validation.
    /// </remarks>
    public abstract int ParseBinaryImage(byte[] buffer, int startIndex, int length);

    /// <summary>
    /// Generates binary image of the packet and copies it into the given buffer, for <see cref="BinaryLength"/> bytes.
    /// </summary>
    /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
    /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
    /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
    /// <remarks>
    /// Implementers should validate <paramref name="startIndex"/> and <see cref="BinaryLength"/> against <paramref name="buffer"/> length.
    /// The <see cref="GSF.ArrayExtensions.ValidateParameters{T}"/> method can be used to perform this validation.
    /// </remarks>
    public abstract int GenerateBinaryImage(byte[] buffer, int startIndex);

    /// <summary>
    /// Extracts time-series data from the packet.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IDataPoint"/>s if the packet contains time-series data; otherwise null.</returns>
    public abstract IEnumerable<IDataPoint> ExtractTimeSeriesData();

    #endregion
}