//******************************************************************************************************
//  IChannel.cs - Gbtc
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

using System.Collections.Generic;
using GSF.Parsing;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of any data type that can
    /// be parsed or generated.<br/>
    /// This is the base interface implemented by all parsing/generating classes in the phasor
    /// protocols library; it is the root of the parsing/generating interface hierarchy.
    /// </summary>
    public interface IChannel : ISupportBinaryImage
    {
        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="IChannel"/> object.
        /// </summary>
        Dictionary<string, string> Attributes { get; }

        /// <summary>
        /// Gets or sets the parsing state for this <see cref="IChannel"/> object.
        /// </summary>
        IChannelParsingState State { get; set; }

        /// <summary>
        /// Gets or sets a user definable reference to an object associated with this <see cref="IChannel"/> object.
        /// </summary>
        /// <remarks>
        /// Classes implementing <see cref="IChannel"/> should only track this value.
        /// </remarks>
        object Tag { get; set; }
    }
}