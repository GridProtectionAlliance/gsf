//******************************************************************************************************
//  CommonHeaderBase.cs - Gbtc
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
    /// Represents the base class for a common binary image header implementation.
    /// </summary>
    /// <remarks>
    /// Here is an example of a possible derived class constructor that has an integer TypeID as the
    /// first 4 bytes in the image:
    /// <code>
    /// public CommonHeader(object state, byte[] binaryImage, int startIndex, int length)
    /// {
    ///     if (length > 3)
    ///     {
    ///         TypeID = LittleEndian.ToInt32(binaryImage, startIndex);
    ///         State = state;
    ///     }
    ///     else
    ///         throw new InvalidOperationException("Malformed image");
    /// }
    /// </code>
    /// </remarks>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    public abstract class CommonHeaderBase<TTypeIdentifier> : ICommonHeader<TTypeIdentifier>
    {
        /// <summary>
        /// Gets or sets the identifier used for identifying the <see cref="Type"/> to be parsed.
        /// </summary>
        public virtual TTypeIdentifier TypeID { get; set; }

        /// <summary>
        /// Gets or sets any additional state information that might be needed for parsing.
        /// </summary>
        public object State { get; set; }
    }
}