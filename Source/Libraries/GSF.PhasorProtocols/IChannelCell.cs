//******************************************************************************************************
//  IChannelCell.cs - Gbtc
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
//  02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Runtime.Serialization;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of any kind of <see cref="IChannelFrame"/> cell.
    /// </summary>
    /// <remarks>
    /// This phasor protocol implementation defines a "cell" as a portion of a "frame", i.e., a logical unit of data.
    /// For example, a <see cref="IDataCell"/> (derived from <see cref="IChannelCell"/>) could be defined as a PMU
    /// within a frame of data, a <see cref="IDataFrame"/>, that contains multiple PMU's coming from a PDC.
    /// </remarks>
    public interface IChannelCell : IChannel, ISerializable
    {
        /// <summary>
        /// Gets a reference to the parent <see cref="IChannelFrame"/> for this <see cref="IChannelCell"/>.
        /// </summary>
        IChannelFrame Parent { get; set; }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="ChannelCellBase"/>.
        /// </summary>
        new IChannelCellParsingState State { get; set; }

        /// <summary>
        /// Gets the numeric ID code for this <see cref="IChannelCell"/>.
        /// </summary>
        /// <remarks>
        /// Most phasor measurement devices define some kind of numeric identifier (e.g., a hardware identifier coded into the device ROM); this is the
        /// abstract representation of this identifier.
        /// </remarks>
        ushort IDCode { get; set; }
    }
}