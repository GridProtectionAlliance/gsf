//******************************************************************************************************
//  PacketParser.cs - Gbtc
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
//  03/28/2007 - Pinal C. Patel
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
using GSF.Parsing;

namespace GSF.Historian.Packets;

/// <summary>
/// Represents a data parser that can parse binary data in to <see cref="IPacket"/>s.
/// </summary>
public class PacketParser : MultiSourceFrameImageParserBase<Guid, short, IPacket>
{
    #region [ Properties ]

    /// <summary>
    /// Returns false since the protocol implementation of <see cref="IPacket"/> does not use synchronization.
    /// </summary>
    public override bool ProtocolUsesSyncBytes => false;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Returns an <see cref="PacketCommonHeader"/> object.
    /// </summary>
    /// <param name="buffer">Buffer containing data to parse.</param>
    /// <param name="offset">Offset index into <paramref name="buffer"/> that represents where to start parsing.</param>
    /// <param name="length">Maximum length of valid data from <paramref name="offset"/>.</param>
    /// <returns>A <see cref="PacketCommonHeader"/> object parsed from the <paramref name="buffer"/>, or <c>null</c> if not enough buffer is available..</returns>
    protected override ICommonHeader<short> ParseCommonHeader(byte[] buffer, int offset, int length)
    {
        // Return null if there is not enough buffer to parse common header
        return length > 1 ? new PacketCommonHeader(buffer, offset, length) : null;
    }

    #endregion
}