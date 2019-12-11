//******************************************************************************************************
//  ICommonHeader.cs - Gbtc
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
//  11/17/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.Parsing
{
    /// <summary>
    /// Defines the common header of a frame image for a set of parsed types, consisting at least of a type ID.
    /// </summary>
    /// <remarks>
    /// Header implementations can extend this interface as necessary to accomodate protocol specific header images.
    /// </remarks>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    public interface ICommonHeader<out TTypeIdentifier>
    {
        /// <summary>
        /// Gets or sets the identifier used for identifying the <see cref="Type"/> to be parsed.
        /// </summary>
        TTypeIdentifier TypeID { get; }

        /// <summary>
        /// Gets or sets any additional state information that might be needed for parsing.
        /// </summary>
        object State { get; set; }
    }
}
