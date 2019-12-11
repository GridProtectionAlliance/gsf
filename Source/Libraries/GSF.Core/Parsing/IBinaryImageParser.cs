//******************************************************************************************************
//  IBinaryImageParser.cs - Gbtc
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
    /// This interface represents the protocol independent representation of a streaming data parser.
    /// </summary>
    public interface IBinaryImageParser : IProvideStatus
    {
        /// <summary>
        /// Occurs when data image fails deserialization due to an exception.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the remaining portion of the binary image that failed to parse.
        /// </remarks>
        event EventHandler<EventArgs<byte[]>> DataDiscarded;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while attempting to parse data.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered while parsing data.
        /// </remarks>
        event EventHandler<EventArgs<Exception>> ParsingException;

        /// <summary>
        /// Start the streaming data parser.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the streaming data parser.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the data parser is currently enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets the total number of buffer images processed so far.
        /// </summary>
        long TotalProcessedBuffers { get; }

        /// <summary>
        /// Writes a sequence of bytes onto the <see cref="IBinaryImageParser"/> stream for parsing.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        void Write(byte[] buffer, int offset, int count);
    }
}