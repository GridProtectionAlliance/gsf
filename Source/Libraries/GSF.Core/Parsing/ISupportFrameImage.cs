//******************************************************************************************************
//  ISupportFrameImage.cs - Gbtc
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
//  03/01/2007 - Pinal C. Patel
//       Original version of source code generated.
//  09/10/2008 - J. Ritchie Carroll
//       Converted to C#.
//  10/28/2008 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/23/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.Parsing
{
    /// <summary>
    /// Specifies that this <see cref="System.Type"/> can produce or consume a frame of data represented as a binary image.
    /// </summary>
    /// <remarks>
    /// Related types of protocol data that occur as frames in a stream can implement this interface for automated parsing
    /// via the <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> class.
    /// </remarks>
    /// <typeparam name="TTypeIdentifier">Type of the frame identifier.</typeparam>
    public interface ISupportFrameImage<TTypeIdentifier> : ISupportBinaryImage
    {
        /// <summary>
        /// Gets or sets current <see cref="ICommonHeader{TTypeIdentifier}"/>.
        /// </summary>
        /// <remarks>
        /// If used, this will need to be set before call to <see cref="ISupportBinaryImage.ParseBinaryImage"/>.
        /// </remarks>
        ICommonHeader<TTypeIdentifier> CommonHeader { get; set; }

        /// <summary>
        /// Gets the identifier that can be used for identifying the <see cref="Type"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="TypeID"/> must be unique across all siblings implementing a common <see cref="Type"/> or interface.
        /// </para>
        /// <para>
        /// Output types implement this class so they have a consistently addressable identification property.
        /// </para>
        /// </remarks>
        TTypeIdentifier TypeID { get; }

        /// <summary>
        /// Gets flag that determines if frame image can queued for publication or should be processed immediately.
        /// </summary>
        /// <remarks>
        /// Some frames, e.g., a configuration or key frame, may be critical to processing of other frames. In this
        /// case, these types of frames should be published immediately so that subsequent frame parsing can have
        /// access to needed critical information.
        /// </remarks>
        bool AllowQueuedPublication { get; }
    }
}