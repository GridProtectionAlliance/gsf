//******************************************************************************************************
//  ISupportSourceIdentifiableFrameImage.cs - Gbtc
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
//  11/06/2012 - J. Ritchie Carroll
//       Original version of source code generated.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.Parsing
{
    /// <summary>
    /// Specifies that this <see cref="System.Type"/> can produce or consume a frame of data represented as a binary image
    /// that can be identified by its data source.
    /// </summary>
    /// <remarks>
    /// Related types of protocol data that occur as frames in a stream can implement this interface for automated parsing
    /// via the <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier, TTypeIdentifier, TOutputType}"/> class.
    /// </remarks>
    /// <typeparam name="TSourceIdentifier">Type of identifier for the data source.</typeparam>
    /// <typeparam name="TTypeIdentifier">Type of the frame identifier.</typeparam>
    public interface ISupportSourceIdentifiableFrameImage<TSourceIdentifier, TTypeIdentifier> : ISupportFrameImage<TTypeIdentifier>
    {
        /// <summary>
        /// Gets or sets the data source identifier of the frame image.
        /// </summary>
        TSourceIdentifier Source { get; set; }
    }
}