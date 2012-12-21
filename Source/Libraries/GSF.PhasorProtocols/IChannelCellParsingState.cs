//******************************************************************************************************
//  IChannelCellParsingState.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  ---------------------------------------------------------------------------------------------------//  02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of the parsing state of any kind of <see cref="IChannelCell"/>.
    /// </summary>
    public interface IChannelCellParsingState : IChannelParsingState
    {
        /// <summary>
        /// Gets or sets the number of phasor elements associated with the <see cref="IChannelCell"/> being parsed.
        /// </summary>
        int PhasorCount { get; set; }

        /// <summary>
        /// Gets or sets the number of analog elements associated with the <see cref="IChannelCell"/> being parsed.
        /// </summary>
        int AnalogCount { get; set; }

        /// <summary>
        /// Gets or sets the number of digital elements associated with the <see cref="IChannelCell"/> being parsed.
        /// </summary>
        int DigitalCount { get; set; }
    }
}