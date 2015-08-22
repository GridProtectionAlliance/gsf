//*******************************************************************************************************
//  IBinaryImageParser.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Parsing
{
    /// <summary>
    /// This interface represents the protocol independent representation of a streaming data parser.
    /// </summary>
    public interface IBinaryImageParser : IProvideStatus
    {
        /// <summary>
        /// Occurs when data image fails deserialized due to an exception.
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