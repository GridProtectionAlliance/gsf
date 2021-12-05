//******************************************************************************************************
//  IChannelFrameParsingState.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Defines function signature for creating new <see cref="IChannelCell"/> objects.
    /// </summary>
    /// <param name="parent">Reference to parent <see cref="IChannelFrame"/>.</param>
    /// <param name="state">Current parsing state of associated <see cref="IChannelFrame"/>.</param>
    /// <param name="index">Index of cell within its collection.</param>
    /// <param name="buffer">Binary image to parse <see cref="IChannelCell"/> from.</param>
    /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
    /// <param name="parsedLength">Returns the total number of bytes parsed from <paramref name="buffer"/>.</param>
    /// <returns>New <see cref="IChannelCell"/> object.</returns>
    /// <typeparam name="T">Specific <see cref="IChannelCell"/> type of object that gets created by referenced function.</typeparam>
    public delegate T CreateNewCellFunction<T>(IChannelFrame parent, IChannelFrameParsingState<T> state, int index, byte[] buffer, int startIndex, out int parsedLength) where T : IChannelCell;

    /// <summary>
    /// Represents a protocol independent interface representation of the parsing state of any kind of <see cref="IChannelFrame"/>.
    /// </summary>
    /// <typeparam name="T">Specific <see cref="IChannelCell"/> type that the <see cref="IChannelFrameParsingState{T}"/> references.</typeparam>
    public interface IChannelFrameParsingState<T> : IChannelParsingState where T : IChannelCell
    {
        /// <summary>
        /// Gets reference to delegate used to create a new <see cref="IChannelCell"/> object.
        /// </summary>
        CreateNewCellFunction<T> CreateNewCell { get; }

        /// <summary>
        /// Gets or sets number of cells that are expected in associated <see cref="IChannelFrame"/> being parsed.
        /// </summary>
        int CellCount { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if header lengths should be trusted over parsed byte count.
        /// </summary>
        /// <remarks>
        /// It is expected that this will normally be left as <c>true</c>.
        /// </remarks>
        bool TrustHeaderLength { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if frame's check-sum should be validated.
        /// </summary>
        /// <remarks>
        /// It is expected that this will normally be left as <c>true</c>.
        /// </remarks>
        bool ValidateCheckSum { get; set; }
    }
}